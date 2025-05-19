using AutoMapper;
using FluentValidation;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Orders.Commands
{
    public class UpdateOrderAddress
    {
        public record Command(int OrderId, UpdateOrderAddressDto AddressDto) : IRequest<OrderDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.OrderId)
                    .GreaterThan(0).WithMessage("Valid order ID is required");

                RuleFor(x => x.AddressDto.ShippingAddress)
                    .NotEmpty().WithMessage("Shipping address is required")
                    .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters");

                RuleFor(x => x.AddressDto.BillingAddress)
                    .NotEmpty().WithMessage("Billing address is required")
                    .MaximumLength(500).WithMessage("Billing address must not exceed 500 characters");
            }
        }

        public class Handler : IRequestHandler<Command, OrderDto>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(
                IOrderRepository orderRepository,
                IUnitOfWork unitOfWork,
                IMapper mapper)
            {
                _orderRepository = orderRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new NotFoundException($"Order with ID {request.OrderId} not found");

                try
                {
                    order.UpdateShippingAddress(request.AddressDto.ShippingAddress);
                    order.UpdateBillingAddress(request.AddressDto.BillingAddress);
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(ex.Message);
                }

                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return _mapper.Map<OrderDto>(order);
            }
        }
    }
}
