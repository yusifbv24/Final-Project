namespace InventoryManagement.Web.Models.Inventory
{
    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
    }
}
