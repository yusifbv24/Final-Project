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
    public class CancelOrder
    {
        public record Command(int OrderId) : IRequest<OrderDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.OrderId)
                    .GreaterThan(0).WithMessage("Valid order ID is required");
            }
        }

        public class Handler : IRequestHandler<Command, OrderDto>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IInventoryServiceClient _inventoryServiceClient;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(
                IOrderRepository orderRepository,
                IInventoryServiceClient inventoryServiceClient,
                IMessagePublisher messagePublisher,
                IUnitOfWork unitOfWork,
                IMapper mapper)
            {
                _orderRepository = orderRepository;
                _inventoryServiceClient = inventoryServiceClient;
                _messagePublisher = messagePublisher;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new NotFoundException($"Order with ID {request.OrderId} not found");

                try
                {
                    var oldStatus = order.Status;
                    order.UpdateStatus(OrderStatus.Cancelled);

                    // Release inventory reservations
                    foreach (var item in order.Items)
                    {
                        await _inventoryServiceClient.ReleaseStockReservationAsync(
                            item.ProductId,
                            item.Quantity,
                            $"Order-{order.Id}",
                            cancellationToken);
                    }

                    await _orderRepository.UpdateAsync(order, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Publish order status changed event
                    await _messagePublisher.PublishAsync(
                        new OrderStatusChangedEvent
                        {
                            OrderId = order.Id,
                            OldStatus = oldStatus,
                            NewStatus = order.Status,
                            ChangedAt = order.UpdatedAt ?? DateTime.UtcNow
                        },
                        "order.status.changed",
                        cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(ex.Message);
                }

                return _mapper.Map<OrderDto>(order);
            }
        }
    }
}
