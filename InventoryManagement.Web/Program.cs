using InventoryManagement.Web.Services;
using InventoryManagement.Web.Services.ApiClients;
using InventoryManagement.Web.Services.RabbitMQ;
using InventoryManagement.Web.Services.SignalR;
using ProductService.Application.Hubs;
using InventoryService.Application.Hubs;
using OrderService.Application.Hubs;

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
builder.Services.AddSingleton<RabbitMQListener>();
builder.Services.AddHostedService<RabbitMQListener>();

// Add background service to manage hub connections
builder.Services.AddHostedService<HubConnectionManager>();

// Register the new hubs
builder.Services.AddSingleton<ProductHub>();
builder.Services.AddSingleton<InventoryHub>();
builder.Services.AddSingleton<OrderHub>();

// Register the event forwarder
builder.Services.AddHostedService<SignalREventForwarder>();

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", builder =>
    {
        builder.WithOrigins(
            "http://localhost:5104",
            "http://localhost:5105",
            "http://localhost:5106",
            "http://localhost:5147")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

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

app.UseCors("SignalRPolicy");

app.UseAuthorization();

// Map SignalR hub proxies
app.MapHub<ProductHub>("/hubs/products");
app.MapHub<InventoryHub>("/hubs/inventory");
app.MapHub<OrderHub>("/hubs/orders");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();