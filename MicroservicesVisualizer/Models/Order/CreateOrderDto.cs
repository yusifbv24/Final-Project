namespace MicroservicesVisualizer.Models.Order
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}
