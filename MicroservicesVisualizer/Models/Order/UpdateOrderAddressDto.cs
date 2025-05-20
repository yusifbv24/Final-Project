namespace MicroservicesVisualizer.Models.Order
{
    public class UpdateOrderAddressDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
    }
}
