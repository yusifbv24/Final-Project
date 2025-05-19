using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Commands
{
    public static class UpdatePurchaseOrder
    {
        public record Command(int Id, UpdatePurchaseOrderDto PurchaseOrderDto) : IRequest<PurchaseOrderDto>;

        public class Handler : IRequestHandler<Command, PurchaseOrderDto>
        {
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
            private readonly IProductServiceClient _productServiceClient;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IMessagePublisher _messagePublisher;

            public Handler(
                IPurchaseOrderRepository purchaseOrderRepository,
                IPurchaseOrderItemRepository purchaseOrderItemRepository,
                IProductServiceClient productServiceClient,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IMessagePublisher messagePublisher)
            {
                _purchaseOrderRepository = purchaseOrderRepository;
                _purchaseOrderItemRepository = purchaseOrderItemRepository;
                _productServiceClient = productServiceClient;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _messagePublisher = messagePublisher;
            }

            public async Task<PurchaseOrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Get purchase order by id
                var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Purchase order with ID {request.Id} not found");

                // Only draft purchase orders can be updated
                if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
                    throw new InvalidOperationException($"Cannot update purchase order with ID {request.Id} because it is not in draft status");

                // Update fields
                purchaseOrder.UpdateExpectedDeliveryDate(request.PurchaseOrderDto.ExpectedDeliveryDate);
                purchaseOrder.UpdateNotes(request.PurchaseOrderDto.Notes);

                // Update items
                foreach (var itemDto in request.PurchaseOrderDto.Items)
                {
                    // Get item by id
                    var existingItem = purchaseOrder.Items.FirstOrDefault(i => i.Id == itemDto.Id)
                        ?? throw new NotFoundException($"Purchase order item with ID {itemDto.Id} not found");

                    // Update item
                    existingItem.UpdateQuantity(itemDto.Quantity);
                    existingItem.UpdateUnitPrice(itemDto.UnitPrice);

                    // Update repository
                    await _purchaseOrderItemRepository.UpdateAsync(existingItem, cancellationToken);
                }

                // Update repository
                await _purchaseOrderRepository.UpdateAsync(purchaseOrder, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event
                await _messagePublisher.PublishAsync(
                    new PurchaseOrderUpdatedEvent
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        OrderNumber = purchaseOrder.OrderNumber,
                        SupplierId = purchaseOrder.SupplierId,
                        TotalAmount = purchaseOrder.TotalAmount
                    },
                    "purchase-orders.updated",
                    cancellationToken);

                // Return mapped DTO
                return _mapper.Map<PurchaseOrderDto>(purchaseOrder);
            }
        }
        public record PurchaseOrderUpdatedEvent
        {
            public int PurchaseOrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public int SupplierId { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
