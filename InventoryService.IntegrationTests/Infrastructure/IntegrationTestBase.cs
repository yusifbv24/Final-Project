using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace InventoryService.IntegrationTests.Infrastructure
{
    public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>
    {
        protected readonly HttpClient Client;
        protected readonly IntegrationTestWebAppFactory Factory;
        protected readonly JsonSerializerOptions JsonOptions;
        private readonly Respawner _respawner;
        private readonly IServiceScope _scope;

        protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            _scope = factory.Services.CreateScope();

            // Setup Respawner for database cleanup between tests
            var dbContext = _scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            _respawner = Respawner.CreateAsync(dbContext.Database.GetConnectionString()!, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            }).GetAwaiter().GetResult();
        }

        protected async Task<T?> GetAsync<T>(string url)
        {
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        protected async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await Client.PostAsync(url, content);
        }

        protected async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await Client.PutAsync(url, content);
        }

        protected async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await Client.DeleteAsync(url);
        }

        protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        protected async Task ResetDatabaseAsync()
        {
            var dbContext = _scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            await _respawner.ResetAsync(dbContext.Database.GetConnectionString()!);
            IntegrationTestWebAppFactory.SeedTestData(dbContext);
        }

        public void Dispose()
        {
            _scope?.Dispose();
            Client?.Dispose();
        }
    }
}
