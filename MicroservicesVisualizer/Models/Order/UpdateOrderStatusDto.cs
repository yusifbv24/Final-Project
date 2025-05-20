using System.Text.Json.Serialization;

namespace MicroservicesVisualizer.Models.Order
{
    public class UpdateOrderStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }
    }
}
