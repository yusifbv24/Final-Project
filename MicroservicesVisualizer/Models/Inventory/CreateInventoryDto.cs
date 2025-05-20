namespace MicroservicesVisualizer.Models.Inventory
{
    public class CreateInventoryDto
    {
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
    }
}
