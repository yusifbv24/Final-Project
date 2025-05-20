using Microsoft.AspNetCore.SignalR.Client;
using MicroservicesVisualizer.Hubs;
using MicroservicesVisualizer.Services;
using MicroservicesVisualizer.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure HTTP clients for microservices
builder.Services.AddHttpClient<IInventoryService, InventoryService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("MicroserviceUrls:InventoryService") ?? "http://localhost:5105/");
});

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("MicroserviceUrls:OrderService") ?? "http://localhost:5155/");
});

builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("MicroserviceUrls:ProductService") ?? "http://localhost:5104/");
});

builder.Services.AddHttpClient<ISupplierService, SupplierService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("MicroserviceUrls:SupplierService") ?? "http://localhost:5281/");
});

// Add SignalR services and hub connections
builder.Services.AddSignalRServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();