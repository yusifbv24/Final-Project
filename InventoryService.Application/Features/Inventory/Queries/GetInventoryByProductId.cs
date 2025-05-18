using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Repositories;
using InventoryService.Domain.Exceptions;
using MediatR;

namespace InventoryService.Application.Features.Inventory.Queries
{
    public class GetInventoryByProductId
    {
        public record Query(int ProductId) : IRequest<IEnumerable<InventoryDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<InventoryDto>>
        {
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IProductServiceClient _productServiceClient;
            private readonly IMapper _mapper;

            public Handler(
                IInventoryRepository inventoryRepository,
                IProductServiceClient productServiceClient,
                IMapper mapper)
            {
                _inventoryRepository = inventoryRepository;
                _productServiceClient = productServiceClient;
                _mapper = mapper;
            }

            public async Task<IEnumerable<InventoryDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate product exists
                var productExists = await _productServiceClient.ProductExistsAsync(request.ProductId, cancellationToken);
                if (!productExists)
                    throw new NotFoundException($"Product with ID {request.ProductId} not found");

                var inventories = await _inventoryRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
                return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
            }
        }
    }
}
