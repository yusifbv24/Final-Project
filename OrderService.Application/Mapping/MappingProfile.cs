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
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<CreateOrderDto, Order>()
                .ConstructUsing(dto => new Order(
                    dto.CustomerId,
                    dto.ShippingAddress,
                    dto.BillingAddress,
                    dto.Notes));

            // OrderItem mappings
            CreateMap<OrderItem, OrderItemDto>();

            CreateMap<CreateOrderItemDto, OrderItem>()
                .ConstructUsing((dto, context) =>
                {
                    // ProductName and UnitPrice will be set later from Product Service
                    return new OrderItem(
                        0, // OrderId will be set later
                        dto.ProductId,
                        "Product Name Placeholder", // Will be updated
                        dto.Quantity,
                        0); // Will be updated
                });

            // Customer mappings
            CreateMap<Customer, CustomerDto>();

            CreateMap<CreateCustomerDto, Customer>()
                .ConstructUsing(dto => new Customer(
                    dto.Name,
                    dto.Email,
                    dto.Phone,
                    dto.DefaultShippingAddress,
                    dto.DefaultBillingAddress));
        }
    }
}
