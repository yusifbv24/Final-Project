namespace OrderService.Application.DTOs
{
    public record UpdateCustomerDto
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string DefaultShippingAddress { get; init; } = string.Empty;
        public string DefaultBillingAddress { get; init; } = string.Empty;
    }
}
