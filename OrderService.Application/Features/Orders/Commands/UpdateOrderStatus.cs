using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using OrderService.Application.DTOs;
using OrderService.Application.Events;
using OrderService.Application.Hubs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Orders.Commands
{
    public class UpdateOrderStatus
    {
        public record Command(int Id, UpdateOrderStatusDto StatusDto) : IRequest<OrderDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0).WithMessage("Invalid order ID");
                RuleFor(x => x.StatusDto.Status).IsInEnum().WithMessage("Invalid order status");
            }
        }

        public class Handler : IRequestHandler<Command, OrderDto>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IHubContext<OrderHub> _orderHubContext;


            public Handler(
                IOrderRepository orderRepository,
                IMessagePublisher messagePublisher,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IHubContext<OrderHub> orderHubContext)
            {
                _orderRepository = orderRepository;
                _messagePublisher = messagePublisher;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _orderHubContext = orderHubContext;
            }

            public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Order with ID {request.Id} not found");

                // Save old status for event
                var oldStatus = order.Status;

                // Update status
                order.UpdateStatus(request.StatusDto.Status);

                // Save changes
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish status changed event
                await _messagePublisher.PublishAsync(
                    new OrderStatusChangedEvent
                    {
                        OrderId = order.Id,
                        OldStatus = oldStatus,
                        NewStatus = order.Status,
                        UpdatedAt = order.UpdatedAt ?? DateTime.UtcNow
                    },
                    "order.status.changed",
                    cancellationToken);

                // Notify clients via SignalR
                await _orderHubContext.Clients.All.SendAsync("OrderStatusChanged", order.Id, order.Status.ToString());

                // Return mapped DTO
                return _mapper.Map<OrderDto>(order);
            }
        }
    }
}
