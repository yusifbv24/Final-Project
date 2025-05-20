namespace MicroservicesVisualizer.Models.Inventory
{
    public class LocationWithInventoryDto : LocationDto
    {
        public int TotalItems { get; set; }
        public int TotalUniqueProducts { get; set; }
        public int TotalQuantity { get; set; }
    }
}
