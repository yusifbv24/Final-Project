using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.Suppliers.Queries
{
    public static class GetAllSuppliers
    {
        public record Query() : IRequest<IEnumerable<SupplierDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<SupplierDto>>
        {
            private readonly ISupplierRepository _supplierRepository;
            private readonly IMapper _mapper;

            public Handler(ISupplierRepository supplierRepository, IMapper mapper)
            {
                _supplierRepository = supplierRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<SupplierDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var suppliers = await _supplierRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
            }
        }
    }
}
