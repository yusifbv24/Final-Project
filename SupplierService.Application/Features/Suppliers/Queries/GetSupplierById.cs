using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.Suppliers.Queries
{
    public static class GetSupplierById
    {
        public record Query(int Id) : IRequest<SupplierDto>;

        public class Handler : IRequestHandler<Query, SupplierDto>
        {
            private readonly ISupplierRepository _supplierRepository;
            private readonly IMapper _mapper;

            public Handler(ISupplierRepository supplierRepository, IMapper mapper)
            {
                _supplierRepository = supplierRepository;
                _mapper = mapper;
            }

            public async Task<SupplierDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Supplier with ID {request.Id} not found");

                return _mapper.Map<SupplierDto>(supplier);
            }
        }
    }
}
