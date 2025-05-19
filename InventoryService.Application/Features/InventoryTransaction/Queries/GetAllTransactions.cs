using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.InventoryTransaction.Queries
{
    public class GetAllTransactions
    {
        public record Query : IRequest<IEnumerable<InventoryTransactionDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<InventoryTransactionDto>>
        {
            private readonly IInventoryTransactionRepository _transactionRepository;
            private readonly IMapper _mapper;

            public Handler(IInventoryTransactionRepository transactionRepository, IMapper mapper)
            {
                _transactionRepository = transactionRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<InventoryTransactionDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var transactions = await _transactionRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
            }
        }
    }
}
