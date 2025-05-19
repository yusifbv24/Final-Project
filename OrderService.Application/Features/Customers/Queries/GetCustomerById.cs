using AutoMapper;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Customers.Queries
{
    public class GetCustomerById
    {
        public record Query(int CustomerId) : IRequest<CustomerDto>;

        public class Handler : IRequestHandler<Query, CustomerDto>
        {
            private readonly ICustomerRepository _customerRepository;
            private readonly IMapper _mapper;

            public Handler(ICustomerRepository customerRepository, IMapper mapper)
            {
                _customerRepository = customerRepository;
                _mapper = mapper;
            }

            public async Task<CustomerDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
                    ?? throw new NotFoundException($"Customer with ID {request.CustomerId} not found");

                return _mapper.Map<CustomerDto>(customer);
            }
        }
    }
}
