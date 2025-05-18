using AutoMapper;
using FluentValidation;
using InventoryService.Application.DTOs;
using InventoryService.Application.Events;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Inventory.Commands
{
    public class UpdateInventoryQuantity
    {
        public record Command(int Id, UpdateInventoryDto InventoryDto) : IRequest<InventoryDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Invalid inventory ID");

                RuleFor(x => x.InventoryDto.Quantity)
                    .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");
            }
        }

        public class Handler : IRequestHandler<Command, InventoryDto>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IMapper _mapper;

            public Handler(
                IInventoryRepository inventoryRepository,
                IUnitOfWork unitOfWork,
                IMessagePublisher messagePublisher,
                IMapper mapper)
            {
                _inventoryRepository = inventoryRepository;
                _unitOfWork = unitOfWork;
                _messagePublisher = messagePublisher;
                _mapper = mapper;
            }

            public async Task<InventoryDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var inventory = await _inventoryRepository.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException($"Inventory with ID {request.Id} not found");

                inventory.UpdateQuantity(request.InventoryDto.Quantity);

                await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish inventory updated event
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

                // Fetch complete inventory with location details
                var updatedInventory = await _inventoryRepository.GetByIdAsync(inventory.Id, cancellationToken);
                return _mapper.Map<InventoryDto>(updatedInventory);
            }
        }
    }
}
