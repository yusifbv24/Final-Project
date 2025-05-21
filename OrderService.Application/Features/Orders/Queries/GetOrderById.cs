using AutoMapper;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Orders.Queries
{
    public class GetOrderById
    {
        public record Query(int Id) : IRequest<OrderDto>;

        public class Handler : IRequestHandler<Query, OrderDto>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IMapper _mapper;

            public Handler(IOrderRepository orderRepository, IMapper mapper)
            {
                _orderRepository = orderRepository;
                _mapper = mapper;
            }

            public async Task<OrderDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Order with ID {request.Id} not found");

                return _mapper.Map<OrderDto>(order);
            }
        }
    }
}
