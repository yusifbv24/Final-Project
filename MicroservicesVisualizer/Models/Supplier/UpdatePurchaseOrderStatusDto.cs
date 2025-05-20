using System.Text.Json.Serialization;

namespace MicroservicesVisualizer.Models.Supplier
{
    public class UpdatePurchaseOrderStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PurchaseOrderStatus Status { get; set; }
    }
}
