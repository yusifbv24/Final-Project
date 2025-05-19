namespace ProductService.Application.Events
{
    public record ProductCreatedEvent(int ProductId,string Name,string SKU, decimal Price, int CategoryId, DateTime CreatedAt);
}
