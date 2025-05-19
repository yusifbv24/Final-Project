using MediatR;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Features.Customers.Commands
{
    public class DeleteCustomer
    {
        public record Command(int CustomerId) : IRequest<bool>;

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly ICustomerRepository _customerRepository;
            private readonly IOrderRepository _orderRepository;
            private readonly IUnitOfWork _unitOfWork;

            public Handler(
                ICustomerRepository customerRepository,
                IOrderRepository orderRepository,
                IUnitOfWork unitOfWork)
            {
                _customerRepository = customerRepository;
                _orderRepository = orderRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
                    ?? throw new NotFoundException($"Customer with ID {request.CustomerId} not found");

                // Check if customer has any orders
                var customerOrders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
                if (customerOrders.Any())
                    throw new InvalidOperationException(
                        $"Cannot delete customer with ID {request.CustomerId} as they have existing orders");

                await _customerRepository.DeleteAsync(customer, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }
}
