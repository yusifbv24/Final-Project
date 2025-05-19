using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Commands
{
    public static class CreatePurchaseOrder
    {
        public record Command(CreatePurchaseOrderDto PurchaseOrderDto) : IRequest<PurchaseOrderDto>;

        public class Handler : IRequestHandler<Command, PurchaseOrderDto>
        {
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly ISupplierRepository _supplierRepository;
            private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
            private readonly IProductServiceClient _productServiceClient;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IMessagePublisher _messagePublisher;

            public Handler(
                IPurchaseOrderRepository purchaseOrderRepository,
                ISupplierRepository supplierRepository,
                IPurchaseOrderItemRepository purchaseOrderItemRepository,
                IProductServiceClient productServiceClient,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IMessagePublisher messagePublisher)
            {
                _purchaseOrderRepository = purchaseOrderRepository;
                _supplierRepository = supplierRepository;
                _purchaseOrderItemRepository = purchaseOrderItemRepository;
                _productServiceClient = productServiceClient;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _messagePublisher = messagePublisher;
            }

            public async Task<PurchaseOrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Verify supplier exists
                var supplier = await _supplierRepository.GetByIdAsync(request.PurchaseOrderDto.SupplierId, cancellationToken)
                    ?? throw new NotFoundException($"Supplier with ID {request.PurchaseOrderDto.SupplierId} not found");

                if (!supplier.IsActive)
                    throw new InvalidOperationException($"Cannot create purchase order for inactive supplier with ID {request.PurchaseOrderDto.SupplierId}");

                // Check if order number is already used
                if (await _purchaseOrderRepository.ExistsByOrderNumberAsync(request.PurchaseOrderDto.OrderNumber, cancellationToken))
                    throw new InvalidOperationException($"Purchase order with order number {request.PurchaseOrderDto.OrderNumber} already exists");

                // Create purchase order entity
                var purchaseOrder = new PurchaseOrder(
                    request.PurchaseOrderDto.SupplierId,
                    request.PurchaseOrderDto.OrderNumber,
                    request.PurchaseOrderDto.ExpectedDeliveryDate,
                    request.PurchaseOrderDto.Notes
                );

                // Add purchase order to repository
                await _purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Add items to purchase order
                foreach (var itemDto in request.PurchaseOrderDto.Items)
                {
                    // Verify product exists
                    var productInfo = await _productServiceClient.GetProductAsync(itemDto.ProductId, cancellationToken);

                    if (productInfo == null)
                        throw new NotFoundException($"Product with ID {itemDto.ProductId} not found");

                    // Create purchase order item entity
                    var item = new PurchaseOrderItem(
                        purchaseOrder.Id,
                        itemDto.ProductId,
                        productInfo.Name,
                        itemDto.Quantity,
                        itemDto.UnitPrice
                    );

                    // Add item to purchase order
                    purchaseOrder.AddItem(item);

                    // Add item to repository
                    await _purchaseOrderItemRepository.AddAsync(item, cancellationToken);
                }

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event
                await _messagePublisher.PublishAsync(
                    new PurchaseOrderCreatedEvent
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        OrderNumber = purchaseOrder.OrderNumber,
                        SupplierId = purchaseOrder.SupplierId,
                        TotalAmount = purchaseOrder.TotalAmount
                    },
                    "purchase-orders.created",
                    cancellationToken);

                // Return mapped DTO
                return _mapper.Map<PurchaseOrderDto>(purchaseOrder);
            }
        }

        public record PurchaseOrderCreatedEvent
        {
            public int PurchaseOrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public int SupplierId { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
