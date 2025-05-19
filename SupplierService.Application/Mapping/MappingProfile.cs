using AutoMapper;
using SupplierService.Application.DTOs;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Supplier mappings
            CreateMap<Supplier, SupplierDto>();

            // PurchaseOrder mappings
            CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            // PurchaseOrderItem mappings
            CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>();
        }
    }
}
