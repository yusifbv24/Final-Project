namespace InventoryService.Application.DTOs
{
    public record RemoveStockRequest(int Quantity, string Reference, string Notes);
}
