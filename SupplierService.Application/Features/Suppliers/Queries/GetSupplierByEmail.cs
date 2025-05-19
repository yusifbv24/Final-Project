using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.Suppliers.Queries
{
    public static class GetSupplierByEmail
    {
        public record Query(string Email) : IRequest<SupplierDto>;

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
                var supplier = await _supplierRepository.GetByEmailAsync(request.Email, cancellationToken)
                    ?? throw new NotFoundException($"Supplier with email {request.Email} not found");

                return _mapper.Map<SupplierDto>(supplier);
            }
        }
    }

}
