using Asp.Versioning.ApiExplorer;
using Asp.Versioning;
using SupplierService.API.Hubs;
using System.Text.Json.Serialization;
using SupplierService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SupplierService.API.Middleware;
using SupplierService.Application;
using SupplierService.Infrastructure;
using SupplierService.API.SwaggerConfiguration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// Add application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseCors("AllowAll");

// Custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<SupplierHub>("/hubs/suppliers");

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SupplierDbContext>();
        context.Database.Migrate();
        context.Database.EnsureCreated();
        await SupplierServiceSeeder.SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();