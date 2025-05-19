using AutoMapper;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Orders.Queries
{
    public class GetOrdersByCustomerId
    {
        public record Query(int CustomerId) : IRequest<IEnumerable<OrderDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<OrderDto>>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly ICustomerRepository _customerRepository;
            private readonly IMapper _mapper;

            public Handler(
                IOrderRepository orderRepository,
                ICustomerRepository customerRepository,
                IMapper mapper)
            {
                _orderRepository = orderRepository;
                _customerRepository = customerRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<OrderDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate customer exists
                var customerExists = await _customerRepository.ExistsByIdAsync(request.CustomerId, cancellationToken);
                if (!customerExists)
                    throw new NotFoundException($"Customer with ID {request.CustomerId} not found");

                var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
        }
    }
}
