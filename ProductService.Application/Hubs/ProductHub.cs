using Microsoft.AspNetCore.SignalR;

namespace ProductService.Application.Hubs
{
    public class ProductHub : Hub
    {
        public async Task NotifyProductCreated(int productId, string name)
        {
            await Clients.All.SendAsync("ProductCreated", productId, name);
        }

        public async Task NotifyProductUpdated(int productId, string name)
        {
            await Clients.All.SendAsync("ProductUpdated", productId, name);
        }

        public async Task NotifyProductDeleted(int productId)
        {
            await Clients.All.SendAsync("ProductDeleted", productId);
        }
    }
}
