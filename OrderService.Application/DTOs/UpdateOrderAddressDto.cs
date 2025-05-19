namespace OrderService.Application.DTOs
{
    public record UpdateOrderAddressDto
    {
        public string ShippingAddress { get; init; } = string.Empty;
        public string BillingAddress { get; init; } = string.Empty;
    }
}