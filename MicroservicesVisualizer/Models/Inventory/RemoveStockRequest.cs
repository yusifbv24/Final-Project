namespace MicroservicesVisualizer.Models.Inventory
{
    public class RemoveStockRequest
    {
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
