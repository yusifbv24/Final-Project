using AutoMapper;
using FluentValidation;
using InventoryService.Application.DTOs;
using InventoryService.Application.Events;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using InventoryService.Application.Hubs;

namespace InventoryService.Application.Features.InventoryTransaction.Commands
{
    public class CreateInventoryTransaction
    {
        public record Command(CreateInventoryTransactionDto TransactionDto) : IRequest<InventoryTransactionDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.TransactionDto.InventoryId)
                    .GreaterThan(0).WithMessage("Valid inventory ID is required");

                RuleFor(x => x.TransactionDto.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be positive");

                RuleFor(x => x.TransactionDto.Reference)
                    .MaximumLength(100).WithMessage("Reference must not exceed 100 characters");

                RuleFor(x => x.TransactionDto.Notes)
                    .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
            }
        }

        public class Handler : IRequestHandler<Command, InventoryTransactionDto>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IInventoryTransactionRepository _transactionRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IMapper _mapper;
            private readonly IHubContext<InventoryHub> _hubContext;

            public Handler(
                IInventoryRepository inventoryRepository,
                IInventoryTransactionRepository transactionRepository,
                IUnitOfWork unitOfWork,
                IMessagePublisher messagePublisher,
                IMapper mapper,
                IHubContext<InventoryHub> hubContext)
            {
                _inventoryRepository = inventoryRepository;
                _transactionRepository = transactionRepository;
                _unitOfWork = unitOfWork;
                _messagePublisher = messagePublisher;
                _mapper = mapper;
                _hubContext = hubContext;
            }

            public async Task<InventoryTransactionDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validate inventory exists
                var inventory = await _inventoryRepository.GetByIdAsync(
                    request.TransactionDto.InventoryId, cancellationToken)
                    ?? throw new NotFoundException($"Inventory with ID {request.TransactionDto.InventoryId} not found");

                // Apply changes to inventory based on transaction type
                switch (request.TransactionDto.Type)
                {
                    case TransactionType.StockIn:
                        inventory.AddStock(request.TransactionDto.Quantity);
                        break;
                    case TransactionType.StockOut:
                        if (inventory.Quantity < request.TransactionDto.Quantity)
                            throw new InvalidOperationException($"Not enough stock available. Current quantity: {inventory.Quantity}");
                        inventory.RemoveStock(request.TransactionDto.Quantity);
                        break;
                    case TransactionType.Adjustment:
                        inventory.UpdateQuantity(request.TransactionDto.Quantity);
                        break;
                    default:
                        throw new InvalidOperationException($"Transaction type {request.TransactionDto.Type} is not supported by this endpoint");
                }

                await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

                // Create transaction record
                var transaction = _mapper.Map<Domain.Entities.InventoryTransaction>(request.TransactionDto);
                var result = await _transactionRepository.AddAsync(transaction, cancellationToken);
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
                        TransactionId = result.Id,
                        InventoryId = result.InventoryId,
                        ProductId = inventory.ProductId,
                        LocationId = inventory.LocationId,
                        Type = result.Type,
                        Quantity = result.Quantity,
                        TransactionDate = result.TransactionDate
                    },
                    "inventory.transaction.created",
                    cancellationToken);

                await _hubContext.Clients.All.SendAsync("InventoryUpdated", inventory.Id, inventory.ProductId, inventory.Quantity, cancellationToken);
                await _hubContext.Clients.All.SendAsync("InventoryTransactionCreated", result.Id, result.InventoryId, inventory.ProductId, result.Type.ToString(), result.Quantity, cancellationToken);

                return _mapper.Map<InventoryTransactionDto>(result);
            }
        }
    }
}