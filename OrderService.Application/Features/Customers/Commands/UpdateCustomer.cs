using AutoMapper;
using FluentValidation;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Customers.Commands
{
    public class UpdateCustomer
    {
        public record Command(int CustomerId, UpdateCustomerDto CustomerDto) : IRequest<CustomerDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.CustomerId)
                    .GreaterThan(0).WithMessage("Valid customer ID is required");

                RuleFor(x => x.CustomerDto.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

                RuleFor(x => x.CustomerDto.Email)
                    .NotEmpty().WithMessage("Email is required")
                    .EmailAddress().WithMessage("Invalid email format")
                    .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

                RuleFor(x => x.CustomerDto.Phone)
                    .NotEmpty().WithMessage("Phone is required")
                    .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

                RuleFor(x => x.CustomerDto.DefaultShippingAddress)
                    .NotEmpty().WithMessage("Default shipping address is required")
                    .MaximumLength(500).WithMessage("Default shipping address must not exceed 500 characters");

                RuleFor(x => x.CustomerDto.DefaultBillingAddress)
                    .NotEmpty().WithMessage("Default billing address is required")
                    .MaximumLength(500).WithMessage("Default billing address must not exceed 500 characters");
            }
        }

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

                // Check if email already exists (but not for this customer)
                if (customer.Email != request.CustomerDto.Email)
                {
                    var emailExists = await _customerRepository.ExistsByEmailAsync(
                        request.CustomerDto.Email, cancellationToken);

                    if (emailExists)
                        throw new InvalidOperationException($"Customer with email '{request.CustomerDto.Email}' already exists");
                }

                customer.Update(
                    request.CustomerDto.Name,
                    request.CustomerDto.Email,
                    request.CustomerDto.Phone,
                    request.CustomerDto.DefaultShippingAddress,
                    request.CustomerDto.DefaultBillingAddress
                );

                await _customerRepository.UpdateAsync(customer, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return _mapper.Map<CustomerDto>(customer);
            }
        }
    }
}
