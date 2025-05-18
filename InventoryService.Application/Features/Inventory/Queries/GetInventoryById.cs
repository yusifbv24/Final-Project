using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Inventory.Queries
{
    public class GetInventoryById
    {
        public record Query(int Id) : IRequest<InventoryDto>;

        public class Handler : IRequestHandler<Query, InventoryDto>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IMapper _mapper;

            public Handler(IInventoryRepository inventoryRepository, IMapper mapper)
            {
                _inventoryRepository = inventoryRepository;
                _mapper = mapper;
            }

            public async Task<InventoryDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var inventory = await _inventoryRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Inventory with ID {request.Id} not found");

                return _mapper.Map<InventoryDto>(inventory);
            }
        }
    }
}
