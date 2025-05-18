using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Inventory.Queries
{
    public class GetAllInventory
    {
        public record Query : IRequest<IEnumerable<InventoryDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<InventoryDto>>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IMapper _mapper;

            public Handler(IInventoryRepository inventoryRepository, IMapper mapper)
            {
                _inventoryRepository = inventoryRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<InventoryDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var inventories = await _inventoryRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
            }
        }
    }
}
