using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.InventoryTransaction.Queries
{
    public class GetTransactionsByInventoryId
    {
        public record Query(int InventoryId) : IRequest<IEnumerable<InventoryTransactionDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<InventoryTransactionDto>>
        {
            private readonly IInventoryTransactionRepository _transactionRepository;
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IMapper _mapper;

            public Handler(
                IInventoryTransactionRepository transactionRepository,
                IInventoryRepository inventoryRepository,
                IMapper mapper)
            {
                _transactionRepository = transactionRepository;
                _inventoryRepository = inventoryRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<InventoryTransactionDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate inventory exists
                var inventoryExists = await _inventoryRepository.ExistsByIdAsync(request.InventoryId, cancellationToken);
                if (!inventoryExists)
                    throw new NotFoundException($"Inventory with ID {request.InventoryId} not found");

                var transactions = await _transactionRepository.GetByInventoryIdAsync(request.InventoryId, cancellationToken);
                return _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
            }
        }
    }
}
