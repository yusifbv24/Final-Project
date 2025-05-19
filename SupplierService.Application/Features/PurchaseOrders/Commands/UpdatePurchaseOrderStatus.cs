using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.PurchaseOrders.Commands
{
    public static class UpdatePurchaseOrderStatus
    {
        public record Command(int Id, UpdatePurchaseOrderStatusDto StatusDto) : IRequest<PurchaseOrderDto>;

        public class Handler : IRequestHandler<Command, PurchaseOrderDto>
        {
            private readonly IPurchaseOrderRepository _purchaseOrderRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IMessagePublisher _messagePublisher;

            public Handler(
                IPurchaseOrderRepository purchaseOrderRepository,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IMessagePublisher messagePublisher)
            {
                _purchaseOrderRepository = purchaseOrderRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _messagePublisher = messagePublisher;
            }

            public async Task<PurchaseOrderDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Get purchase order by id
                var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.Id, cancellationToken)
                     ?? throw new NotFoundException($"Purchase order with ID {request.Id} not found");

                // Update status
                purchaseOrder.UpdateStatus(request.StatusDto.Status);

                // Update repository
                await _purchaseOrderRepository.UpdateAsync(purchaseOrder, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event
                await _messagePublisher.PublishAsync(
                    new PurchaseOrderStatusUpdatedEvent
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        OrderNumber = purchaseOrder.OrderNumber,
                        SupplierId = purchaseOrder.SupplierId,
                        Status = purchaseOrder.Status
                    },
                    "purchase-orders.status-updated",
                    cancellationToken);

                // Return mapped DTO
                return _mapper.Map<PurchaseOrderDto>(purchaseOrder);
            }
        }

        public record PurchaseOrderStatusUpdatedEvent
        {
            public int PurchaseOrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public int SupplierId { get; set; }
            public PurchaseOrderStatus Status { get; set; }
        }
    }
}
