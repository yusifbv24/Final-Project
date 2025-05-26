using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using OrderService.Application.DTOs;
using OrderService.Application.Events;
using OrderService.Application.Hubs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
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
                RuleFor(x => x.OrderDto.CustomerName)
                    .NotEmpty().WithMessage("Customer name is required")
                    .MaximumLength(100).WithMessage("Customer name must not exceed 100 characters");

                RuleFor(x => x.OrderDto.CustomerEmail)
                    .NotEmpty().WithMessage("Customer email is required")
                    .EmailAddress().WithMessage("A valid email address is required")
                    .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

                RuleFor(x => x.OrderDto.ShippingAddress)
                    .NotEmpty().WithMessage("Shipping address is required")
                    .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters");

                RuleFor(x => x.OrderDto.Items)
                    .NotEmpty().WithMessage("Order must contain at least one item");

                RuleForEach(x => x.OrderDto.Items).ChildRules(item =>
                {
                    item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Valid product ID is required");
                    item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
                });
            }
        }

        public class Handler : IRequestHandler<Command, OrderDto>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IProductServiceClient _productServiceClient;
            private readonly IInventoryServiceClient _inventoryServiceClient;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IHubContext<OrderHub> _hubContext;


            public Handler(
                IOrderRepository orderRepository,
                IProductServiceClient productServiceClient,
                IInventoryServiceClient inventoryServiceClient,
                IMessagePublisher messagePublisher,
                IUnitOfWork unitOfWork,
                IHubContext<OrderHub> hubContext)
            {
                _orderRepository = orderRepository;
                _productServiceClient = productServiceClient;
                _inventoryServiceClient = inventoryServiceClient;
                _messagePublisher = messagePublisher;
                _unitOfWork = unitOfWork;
                _hubContext = hubContext;
            }

            public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Create order
                var order = new Order(
                    request.OrderDto.CustomerName,
                    request.OrderDto.CustomerEmail,
                    request.OrderDto.ShippingAddress);

                // Add order items
                foreach (var itemDto in request.OrderDto.Items)
                {
                    // Get product details
                    var product = await _productServiceClient.GetProductAsync(itemDto.ProductId, cancellationToken);
                    if (product == null)
                        throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found");

                    // Check stock availability
                    var isAvailable = await _inventoryServiceClient.CheckStockAvailabilityAsync(
                        itemDto.ProductId, itemDto.Quantity, cancellationToken);
                    if (!isAvailable)
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}");

                    // Add item to order
                    var orderItem = new OrderItem(
                        0, // OrderId will be set when order is saved
                        itemDto.ProductId,
                        product.Name,
                        product.Price,
                        itemDto.Quantity);

                    order.AddOrderItem(orderItem);
                }

                // Save order
                await _orderRepository.AddAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reserve inventory for each order item
                foreach (var item in order.OrderItems)
                {
                    await _inventoryServiceClient.ReserveStockAsync(
                        item.ProductId,
                        item.Quantity,
                        $"Order-{order.Id}",
                        cancellationToken);
                }

                // Publish to RabbitMQ
                var orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    CustomerName = order.CustomerName,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    Items = order.OrderItems.Select(i => new OrderItemEvent
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    }).ToList()
                };

                await _messagePublisher.PublishAsync(orderCreatedEvent, "order.created", cancellationToken);

                // Notify via SignalR
                await _hubContext.Clients.All.SendAsync("OrderCreated", order.Id, order.CustomerName, cancellationToken);

                return new OrderDto
                {
                    Id = order.Id,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    ShippingAddress = order.ShippingAddress,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    UpdatedAt = order.UpdatedAt,
                    Items = order.OrderItems.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    }).ToList()
                };
            }
        }
    }
}
