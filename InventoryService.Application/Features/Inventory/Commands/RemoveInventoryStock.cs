using AutoMapper;
using FluentValidation;
using InventoryService.Application.DTOs;
using InventoryService.Application.Events;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Inventory.Commands
{
    public class RemoveInventoryStock
    {
        public record Command(int Id, int Quantity, string Reference, string Notes) : IRequest<InventoryDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Invalid inventory ID");

                RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be positive");

                RuleFor(x => x.Reference)
                    .MaximumLength(100).WithMessage("Reference must not exceed 100 characters");

                RuleFor(x => x.Notes)
                    .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
            }
        }

        public class Handler : IRequestHandler<Command, InventoryDto>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IInventoryTransactionRepository _transactionRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IMapper _mapper;

            public Handler(
                IInventoryRepository inventoryRepository,
                IInventoryTransactionRepository transactionRepository,
                IUnitOfWork unitOfWork,
                IMessagePublisher messagePublisher,
                IMapper mapper)
            {
                _inventoryRepository = inventoryRepository;
                _transactionRepository = transactionRepository;
                _unitOfWork = unitOfWork;
                _messagePublisher = messagePublisher;
                _mapper = mapper;
            }

            public async Task<InventoryDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var inventory = await _inventoryRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Inventory with ID {request.Id} not found");

                if (inventory.Quantity < request.Quantity)
                    throw new InvalidOperationException($"Not enough stock available. Current quantity: {inventory.Quantity}");

                // Remove stock from inventory
                inventory.RemoveStock(request.Quantity);
                await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

                // Create transaction record
                var transaction = new InventoryTransaction(
                    inventory.Id,
                    TransactionType.StockOut,
                    request.Quantity,
                    request.Reference,
                    request.Notes);

                await _transactionRepository.AddAsync(transaction, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish events
                await _messagePublisher.PublishAsync(
                    new InventoryUpdatedEvent
                    {
                        InventoryId = inventory.Id,
                        ProductId = inventory.ProductId,
                        LocationId = inventory.LocationId,
                        Quantity = inventory.Quantity,
                        UpdatedAt = inventory.UpdatedAt ?? DateTime.UtcNow
                    },
                    "inventory.updated",
                    cancellationToken);

                await _messagePublisher.PublishAsync(
                    new InventoryTransactionCreatedEvent
                    {
                        TransactionId = transaction.Id,
                        InventoryId = transaction.InventoryId,
                        ProductId = inventory.ProductId,
                        LocationId = inventory.LocationId,
                        Type = transaction.Type,
                        Quantity = transaction.Quantity,
                        TransactionDate = transaction.TransactionDate
                    },
                    "inventory.transaction.created",
                    cancellationToken);

                // Fetch updated inventory with location details
                var updatedInventory = await _inventoryRepository.GetByIdAsync(inventory.Id, cancellationToken);
                return _mapper.Map<InventoryDto>(updatedInventory);
            }
        }
    }
}
