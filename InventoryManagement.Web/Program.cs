using InventoryManagement.Web.Services.ApiClients;
using InventoryManagement.Web.Services.RabbitMQ;
using InventoryManagement.Web.Services.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure API clients
builder.Services.AddHttpClient<ProductApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"] ?? "http://localhost:5104/");
});

builder.Services.AddHttpClient<CategoryApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"] ?? "http://localhost:5104/");
});

builder.Services.AddHttpClient<InventoryApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:InventoryService"] ?? "http://localhost:5105/");
});

builder.Services.AddHttpClient<LocationApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:InventoryService"] ?? "http://localhost:5105/");
});

builder.Services.AddHttpClient<OrderApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"] ?? "http://localhost:5106/");
});

// Add SignalR
builder.Services.AddSignalR();

// Register SignalR hub clients as singletons
builder.Services.AddSingleton<ProductHubClient>();
builder.Services.AddSingleton<InventoryHubClient>();
builder.Services.AddSingleton<OrderHubClient>();

// Register RabbitMQ listener as a hosted service
builder.Services.AddHostedService<RabbitMQListener>();
builder.Services.AddSingleton<RabbitMQListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map SignalR hub proxies
app.MapHub<ProductHubProxy>("/hubs/products");
app.MapHub<InventoryHubProxy>("/hubs/inventory");
app.MapHub<OrderHubProxy>("/hubs/orders");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start SignalR clients with proper error handling
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var productHubClient = serviceProvider.GetRequiredService<ProductHubClient>();
        var inventoryHubClient = serviceProvider.GetRequiredService<InventoryHubClient>();
        var orderHubClient = serviceProvider.GetRequiredService<OrderHubClient>();

        // Start connections with proper awaiting
        await Task.WhenAll(
            productHubClient.StartAsync(),
            inventoryHubClient.StartAsync(),
            orderHubClient.StartAsync()
        );

        logger.LogInformation("All SignalR hub clients started successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error starting SignalR hub clients");
    }
}

app.Run();