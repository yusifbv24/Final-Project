using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Commands
{
    public static class ReceivePurchaseOrderItem
    {
        public record Command(int PurchaseOrderId, int ItemId, int ReceivedQuantity) : IRequest<PurchaseOrderDto>;

        public class Handler : IRequestHandler<Command, PurchaseOrderDto>
        {
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
            private readonly IInventoryServiceClient _inventoryServiceClient;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IMessagePublisher _messagePublisher;

            public Handler(
                IPurchaseOrderRepository purchaseOrderRepository,
                IPurchaseOrderItemRepository purchaseOrderItemRepository,
                IInventoryServiceClient inventoryServiceClient,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IMessagePublisher messagePublisher)
            {
                _purchaseOrderRepository = purchaseOrderRepository;
                _purchaseOrderItemRepository = purchaseOrderItemRepository;
                _inventoryServiceClient = inventoryServiceClient;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _messagePublisher = messagePublisher;
            }

            public async Task<PurchaseOrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Get purchase order by id
                var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken)
                    ?? throw new NotFoundException($"Purchase order with ID {request.PurchaseOrderId} not found");

                // Get item by id
                var item = purchaseOrder.Items.FirstOrDefault(i => i.Id == request.ItemId);

                if (item == null)
                    throw new NotFoundException($"Purchase order item with ID {request.ItemId} not found in purchase order with ID {request.PurchaseOrderId}");

                // Receive items
                item.ReceiveItems(request.ReceivedQuantity);

                // Add received items to inventory
                var inventoryResult = await _inventoryServiceClient.AddInventoryStockAsync(
                    item.ProductId,
                    request.ReceivedQuantity,
                    purchaseOrder.OrderNumber,
                    cancellationToken);

                if (!inventoryResult)
                {
                    throw new InvalidOperationException($"Failed to update inventory for product ID {item.ProductId}");
                }

                // Update repositories
                await _purchaseOrderItemRepository.UpdateAsync(item, cancellationToken);
                await _purchaseOrderRepository.UpdateAsync(purchaseOrder, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event
                await _messagePublisher.PublishAsync(
                    new PurchaseOrderItemReceivedEvent
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        OrderNumber = purchaseOrder.OrderNumber,
                        ItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ReceivedQuantity = request.ReceivedQuantity,
                        Status = purchaseOrder.Status
                    },
                    "purchase-orders.item-received",
                    cancellationToken);

                // Return mapped DTO
                return _mapper.Map<PurchaseOrderDto>(purchaseOrder);
            }
        }

        public record PurchaseOrderItemReceivedEvent
        {
            public int PurchaseOrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public int ItemId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int ReceivedQuantity { get; set; }
            public PurchaseOrderStatus Status { get; set; }
        }
    }
}
