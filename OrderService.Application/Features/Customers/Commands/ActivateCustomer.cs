using AutoMapper;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Customers.Commands
{
    public class ActivateCustomer
    {
        public record Command(int CustomerId) : IRequest<CustomerDto>;

        public class Handler : IRequestHandler<Command, CustomerDto>
        {
            private readonly ICustomerRepository _customerRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(
                ICustomerRepository customerRepository,
                IUnitOfWork unitOfWork,
                IMapper mapper)
            {
                _customerRepository = customerRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<CustomerDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
                    ?? throw new NotFoundException($"Customer with ID {request.CustomerId} not found");

                customer.Activate();
                await _customerRepository.UpdateAsync(customer, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return _mapper.Map<CustomerDto>(customer);
            }
        }
    }
}
