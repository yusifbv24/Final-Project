using System.Text.Json.Serialization;

namespace MicroservicesVisualizer.Models.Order
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}