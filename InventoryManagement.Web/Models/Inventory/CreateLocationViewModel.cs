using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Web.Models.Inventory
{
    public record CreateLocationViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
