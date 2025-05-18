using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Inventory mappings
            CreateMap<Inventory, InventoryDto>()
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));

            CreateMap<CreateInventoryDto, Inventory>()
                .ConstructUsing(dto => new Inventory(dto.ProductId, dto.LocationId, dto.Quantity));

            // InventoryTransaction mappings
            CreateMap<InventoryTransaction, InventoryTransactionDto>();

            CreateMap<CreateInventoryTransactionDto, InventoryTransaction>()
                .ConstructUsing(dto => new InventoryTransaction(
                    dto.InventoryId,
                    dto.Type,
                    dto.Quantity,
                    dto.Reference,
                    dto.Notes));

            // Location mappings
            CreateMap<Location, LocationDto>();

            CreateMap<CreateLocationDto, Location>()
                .ConstructUsing(dto => new Location(dto.Name, dto.Code, dto.Description));
        }
    }
}
