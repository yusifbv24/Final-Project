using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Location.Commands
{
    public class DeleteLocation
    {
        public record Command(int Id) : IRequest<bool>;

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly ILocationRepository _locationRepository;
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IUnitOfWork _unitOfWork;

            public Handler(
                ILocationRepository locationRepository,
                IInventoryRepository inventoryRepository,
                IUnitOfWork unitOfWork)
            {
                _locationRepository = locationRepository;
                _inventoryRepository = inventoryRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Location with ID {request.Id} not found");

                // Check if location has inventory items
                var inventories = await _inventoryRepository.GetByLocationIdAsync(request.Id, cancellationToken);
                if (inventories.Any())
                    throw new InvalidOperationException($"Cannot delete location with ID {request.Id} as it has inventory items. Move or delete the inventory items first.");

                await _locationRepository.DeleteAsync(location, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }
}
