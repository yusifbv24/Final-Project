using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Queries
{
    public static class GetAllPurchaseOrders
    {
        public record Query() : IRequest<IEnumerable<PurchaseOrderDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<PurchaseOrderDto>>
        {
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly IMapper _mapper;

            public Handler(IPurchaseOrderRepository purchaseOrderRepository, IMapper mapper)
            {
                _purchaseOrderRepository = purchaseOrderRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<PurchaseOrderDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var purchaseOrders = await _purchaseOrderRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);
            }
        }
    }
}
