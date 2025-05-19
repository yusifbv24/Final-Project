using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Queries
{
    public static class GetPurchaseOrderByOrderNumber
    {
        public record Query(string OrderNumber) : IRequest<PurchaseOrderDto>;

        public class Handler : IRequestHandler<Query, PurchaseOrderDto>
        {
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly IMapper _mapper;

            public Handler(IPurchaseOrderRepository purchaseOrderRepository, IMapper mapper)
            {
                _purchaseOrderRepository = purchaseOrderRepository;
                _mapper = mapper;
            }

            public async Task<PurchaseOrderDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var purchaseOrder = await _purchaseOrderRepository.GetByOrderNumberAsync(request.OrderNumber, cancellationToken)
                    ?? throw new NotFoundException($"Purchase order with order number {request.OrderNumber} not found");

                return _mapper.Map<PurchaseOrderDto>(purchaseOrder);
            }
        }
    }
}
