namespace ProductService.Application.Events
{
    public record ProductUpdatedEvent(int ProductId, string Name, string SKU, decimal Price, int CategoryId);
}
