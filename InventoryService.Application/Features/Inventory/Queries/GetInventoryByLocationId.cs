using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Inventory.Queries
{
    public class GetInventoryByLocationId
    {
        public record Query(int LocationId) : IRequest<IEnumerable<InventoryDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<InventoryDto>>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly ILocationRepository _locationRepository;
            private readonly IMapper _mapper;

            public Handler(
                IInventoryRepository inventoryRepository,
                ILocationRepository locationRepository,
                IMapper mapper)
            {
                _inventoryRepository = inventoryRepository;
                _locationRepository = locationRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<InventoryDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate location exists
                var locationExists = await _locationRepository.ExistsByIdAsync(request.LocationId, cancellationToken);
                if (!locationExists)
                    throw new NotFoundException($"Location with ID {request.LocationId} not found");

                var inventories = await _inventoryRepository.GetByLocationIdAsync(request.LocationId, cancellationToken);
                return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
            }
        }
    }
}
