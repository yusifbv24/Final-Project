namespace InventoryService.Application.DTOs
{
    public record AddStockRequest(int Quantity, string Reference, string Notes);
}
