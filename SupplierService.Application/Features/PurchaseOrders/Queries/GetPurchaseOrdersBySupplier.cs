using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Queries
{
    public static class GetPurchaseOrdersBySupplier
    {
        public record Query(int SupplierId) : IRequest<IEnumerable<PurchaseOrderDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<PurchaseOrderDto>>
        {
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly ISupplierRepository _supplierRepository;
            private readonly IMapper _mapper;

            public Handler(
                IPurchaseOrderRepository purchaseOrderRepository,
                ISupplierRepository supplierRepository,
                IMapper mapper)
            {
                _purchaseOrderRepository = purchaseOrderRepository;
                _supplierRepository = supplierRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<PurchaseOrderDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Verify supplier exists
                var supplierExists = await _supplierRepository.ExistsByIdAsync(request.SupplierId, cancellationToken);

                if (!supplierExists)
                    throw new NotFoundException($"Supplier with ID {request.SupplierId} not found");

                var purchaseOrders = await _purchaseOrderRepository.GetBySupplierIdAsync(request.SupplierId, cancellationToken);
                return _mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);
            }
        }
    }
}
