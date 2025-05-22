using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // OrderItem mappings
            CreateMap<OrderItem, OrderItemDto>();
        }
    }
}
