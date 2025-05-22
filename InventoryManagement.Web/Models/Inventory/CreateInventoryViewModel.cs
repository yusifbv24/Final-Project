using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Web.Models.Inventory
{
    public class CreateInventoryViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a product")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a location")]
        public int LocationId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }
    }
}
