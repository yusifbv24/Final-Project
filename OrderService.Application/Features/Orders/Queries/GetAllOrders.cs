using AutoMapper;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Orders.Queries
{
    public class GetAllOrders
    {
        public record Query : IRequest<IEnumerable<OrderDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<OrderDto>>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IMapper _mapper;

            public Handler(IOrderRepository orderRepository, IMapper mapper)
            {
                _orderRepository = orderRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<OrderDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var orders = await _orderRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
        }
    }
}
