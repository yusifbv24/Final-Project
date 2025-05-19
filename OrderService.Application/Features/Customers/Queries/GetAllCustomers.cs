using AutoMapper;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Customers.Queries
{
    public class GetAllCustomers
    {
        public record Query : IRequest<IEnumerable<CustomerDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<CustomerDto>>
        {
            private readonly ICustomerRepository _customerRepository;
            private readonly IMapper _mapper;

            public Handler(ICustomerRepository customerRepository, IMapper mapper)
            {
                _customerRepository = customerRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<CustomerDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var customers = await _customerRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<CustomerDto>>(customers);
            }
        }
    }
}
