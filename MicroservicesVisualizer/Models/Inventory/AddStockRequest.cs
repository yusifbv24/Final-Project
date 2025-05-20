namespace MicroservicesVisualizer.Models.Inventory
{
    public class AddStockRequest
    {
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
