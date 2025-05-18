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
    public class CreateInventory
    {
        public record Command(CreateInventoryDto InventoryDto) : IRequest<InventoryDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.InventoryDto.ProductId)
                    .GreaterThan(0).WithMessage("Valid product ID is required");

                RuleFor(x => x.InventoryDto.LocationId)
                    .GreaterThan(0).WithMessage("Valid location ID is required");

                RuleFor(x => x.InventoryDto.Quantity)
                    .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");
            }
        }

        public class Handler : IRequestHandler<Command, InventoryDto>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly ILocationRepository _locationRepository;
            private readonly IProductServiceClient _productServiceClient;
            private readonly IMessagePublisher _messagePublisher;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(
                IInventoryRepository inventoryRepository,
                ILocationRepository locationRepository,
                IProductServiceClient productServiceClient,
                IMessagePublisher messagePublisher,
                IUnitOfWork unitOfWork,
                IMapper mapper)
            {
                _inventoryRepository = inventoryRepository;
                _locationRepository = locationRepository;
                _productServiceClient = productServiceClient;
                _messagePublisher = messagePublisher;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<InventoryDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validate product exists in ProductService
                var productExists = await _productServiceClient.ProductExistsAsync(
                    request.InventoryDto.ProductId, cancellationToken);

                if (!productExists)
                    throw new NotFoundException($"Product with ID {request.InventoryDto.ProductId} not found");

                // Validate location exists
                var locationExists = await _locationRepository.ExistsByIdAsync(
                    request.InventoryDto.LocationId, cancellationToken);

                if (!locationExists)
                    throw new NotFoundException($"Location with ID {request.InventoryDto.LocationId} not found");

                // Check if inventory for this product and location already exists
                var existingInventory = await _inventoryRepository.GetByProductAndLocationAsync(
                    request.InventoryDto.ProductId, request.InventoryDto.LocationId, cancellationToken);

                if (existingInventory != null)
                    throw new InvalidOperationException($"Inventory for Product ID {request.InventoryDto.ProductId} at Location ID {request.InventoryDto.LocationId} already exists");

                var inventory = _mapper.Map<Domain.Entities.Inventory>(request.InventoryDto);
                var result = await _inventoryRepository.AddAsync(inventory, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish inventory created event
                await _messagePublisher.PublishAsync(
                    new InventoryCreatedEvent
                    {
                        InventoryId = result.Id,
                        ProductId = result.ProductId,
                        LocationId = result.LocationId,
                        Quantity = result.Quantity,
                        CreatedAt = result.CreatedAt
                    },
                    "inventory.created",
                    cancellationToken);

                // Fetch complete inventory with location details
                var savedInventory = await _inventoryRepository.GetByIdAsync(result.Id, cancellationToken);
                return _mapper.Map<InventoryDto>(savedInventory);
            }
        }
    }
}
