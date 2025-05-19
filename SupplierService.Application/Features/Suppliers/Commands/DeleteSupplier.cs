using MediatR;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.Suppliers.Commands
{
    public static class DeleteSupplier
    {
        public record Command(int Id) : IRequest;

        public class Handler : IRequestHandler<Command>
        {
            private readonly ISupplierRepository _supplierRepository;
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMessagePublisher _messagePublisher;

            public Handler(
                ISupplierRepository supplierRepository,
                IPurchaseOrderRepository purchaseOrderRepository,
                IUnitOfWork unitOfWork,
                IMessagePublisher messagePublisher)
            {
                _supplierRepository = supplierRepository;
                _purchaseOrderRepository = purchaseOrderRepository;
                _unitOfWork = unitOfWork;
                _messagePublisher = messagePublisher;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                // Get supplier by id
                var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Supplier with ID {request.Id} not found");

                // Check if supplier has any purchase orders
                var purchaseOrders = await _purchaseOrderRepository.GetBySupplierIdAsync(request.Id, cancellationToken);

                if (purchaseOrders.Any())
                    throw new InvalidOperationException($"Cannot delete supplier with ID {request.Id} because it has associated purchase orders");

                // Delete supplier
                await _supplierRepository.DeleteAsync(supplier, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event
                await _messagePublisher.PublishAsync(
                    new SupplierDeletedEvent
                    {
                        SupplierId = supplier.Id,
                        Name = supplier.Name,
                        Email = supplier.Email
                    },
                    "suppliers.deleted",
                    cancellationToken);
            }
        }

        public record SupplierDeletedEvent
        {
            public int SupplierId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
