using System.Text.Json.Serialization;

namespace MicroservicesVisualizer.Models.Supplier
{
    public enum PurchaseOrderStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        Received,
        Cancelled
    }
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PurchaseOrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
    }
}
