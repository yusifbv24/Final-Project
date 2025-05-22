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

// Start SignalR clients
using (var scope = app.Services.CreateScope())
{
    var productHubClient = scope.ServiceProvider.GetRequiredService<ProductHubClient>();
    var inventoryHubClient = scope.ServiceProvider.GetRequiredService<InventoryHubClient>();
    var orderHubClient = scope.ServiceProvider.GetRequiredService<OrderHubClient>();

    // Start connections asynchronously
    _ = productHubClient.StartAsync();
    _ = inventoryHubClient.StartAsync();
    _ = orderHubClient.StartAsync();
}

app.Run();