using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.InventoryTransaction.Queries
{
    public class GetTransactionById
    {
        public record Query(int Id) : IRequest<InventoryTransactionDto>;

        public class Handler : IRequestHandler<Query, InventoryTransactionDto>
        {
            private readonly IInventoryTransactionRepository _transactionRepository;
            private readonly IMapper _mapper;

            public Handler(IInventoryTransactionRepository transactionRepository, IMapper mapper)
            {
                _transactionRepository = transactionRepository;
                _mapper = mapper;
            }

            public async Task<InventoryTransactionDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);

                if (transaction == null)
                    throw new NotFoundException($"Transaction with ID {request.Id} not found");

                return _mapper.Map<InventoryTransactionDto>(transaction);
            }
        }
    }
}
