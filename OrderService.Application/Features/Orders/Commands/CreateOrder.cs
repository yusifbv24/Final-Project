using AutoMapper;
using FluentValidation;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Orders.Commands
{
    public class CreateOrder
    {
        public record Command(CreateOrderDto OrderDto) : IRequest<OrderDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.OrderDto.CustomerId)
                    .GreaterThan(0).WithMessage("Valid customer ID is required");

                RuleFor(x => x.OrderDto.ShippingAddress)
                    .NotEmpty().WithMessage("Shipping address is required")
                    .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters");

                RuleFor(x => x.OrderDto.BillingAddress)
                    .NotEmpty().WithMessage("Billing address is required")
                    .MaximumLength(500).WithMessage("Billing address must not exceed 500 characters");

                RuleFor(x => x.OrderDto.Notes)
                    .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");

                RuleFor(x => x.OrderDto.Items)
                    .NotNull().WithMessage("Order must have at least one item")
                    .NotEmpty().WithMessage("Order must have at least one item");

                RuleForEach(x => x.OrderDto.Items)
                    .ChildRules(items =>
                    {
                        items.RuleFor(i => i.ProductId)
                            .GreaterThan(0).WithMessage("Valid product ID is required");

                        items.RuleFor(i => i.Quantity)
                            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
                    });
            }
        }

        public class Handler : IRequestHandler<Command, OrderDto>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly ICustomerRepository _customerRepository;
            private readonly IProductServiceClient _productServiceClient;
            private readonly IInventoryServiceClient _inventoryServiceClient;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(
                IOrderRepository orderRepository,
                ICustomerRepository customerRepository,
                IProductServiceClient productServiceClient,
                IInventoryServiceClient inventoryServiceClient,
                IMessagePublisher messagePublisher,
                IUnitOfWork unitOfWork,
                IMapper mapper)
            {
                _orderRepository = orderRepository;
                _customerRepository = customerRepository;
                _productServiceClient = productServiceClient;
                _inventoryServiceClient = inventoryServiceClient;
                _messagePublisher = messagePublisher;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validate customer exists
                var customer = await _customerRepository.GetByIdAsync(request.OrderDto.CustomerId, cancellationToken)
                    ?? throw new NotFoundException($"Customer with ID {request.OrderDto.CustomerId} not found");

                // Validate products exist and have enough stock
                var productIds = request.OrderDto.Items!.Select(i => i.ProductId).ToList();
                var products = await _productServiceClient.GetProductsAsync(productIds, cancellationToken);

                var productsDict = products.ToDictionary(p => p.Id);

                // Check missing products
                var missingProductIds = productIds.Where(id => !productsDict.ContainsKey(id)).ToList();
                if (missingProductIds.Any())
                    throw new NotFoundException($"Products with IDs {string.Join(", ", missingProductIds)} not found");

                // Check stock availability
                foreach (var item in request.OrderDto.Items!)
                {
                    var isAvailable = await _inventoryServiceClient.CheckStockAvailabilityAsync(
                        item.ProductId, item.Quantity, cancellationToken);

                    if (!isAvailable)
                    {
                        var productName = productsDict[item.ProductId].Name;
                        throw new InvalidOperationException(
                            $"Insufficient stock available for product {productName} (ID: {item.ProductId})");
                    }
                }

                // Create order
                var order = _mapper.Map<Order>(request.OrderDto);
                var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);

                // Add order items
                foreach (var itemDto in request.OrderDto.Items!)
                {
                    var product = productsDict[itemDto.ProductId];

                    var orderItem = new OrderItem(
                        createdOrder.Id,
                        itemDto.ProductId,
                        product.Name,
                        itemDto.Quantity,
                        product.Price);

                    createdOrder.AddItem(orderItem);
                }

                // Update the order with items
                await _orderRepository.UpdateAsync(createdOrder, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reserve inventory
                foreach (var item in request.OrderDto.Items!)
                {
                    await _inventoryServiceClient.ReserveStockAsync(
                        item.ProductId,
                        item.Quantity,
                        $"Order-{createdOrder.Id}",
                        cancellationToken);
                }

                // Publish order created event
                await _messagePublisher.PublishAsync(
                    new OrderCreatedEvent
                    {
                        OrderId = createdOrder.Id,
                        CustomerId = createdOrder.CustomerId,
                        Status = createdOrder.Status,
                        TotalAmount = createdOrder.TotalAmount,
                        OrderDate = createdOrder.OrderDate,
                        Items = createdOrder.Items.Select(i => new OrderItemEvent
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice
                        }).ToList()
                    },
                    "order.created",
                    cancellationToken);

                // Fetch complete order
                var completeOrder = await _orderRepository.GetByIdAsync(createdOrder.Id, cancellationToken);
                return _mapper.Map<OrderDto>(completeOrder);
            }
        }
    }
}