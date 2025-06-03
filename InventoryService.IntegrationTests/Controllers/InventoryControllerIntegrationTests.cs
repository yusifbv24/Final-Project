using System.Net;
using FluentAssertions;
using InventoryService.Application.DTOs;
using InventoryService.IntegrationTests.Infrastructure;

namespace InventoryService.IntegrationTests.Controllers
{
    public class InventoryControllerIntegrationTests : IntegrationTestBase
    {
        public InventoryControllerIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetAll_ReturnsAllInventories()
        {
            // Act
            var inventories = await GetAsync<List<InventoryDto>>("/api/v1/inventory");

            // Assert
            inventories.Should().NotBeNull();
            inventories.Should().HaveCount(4);
            inventories.Should().Contain(i => i.ProductId == 1 && i.LocationId == 1 && i.Quantity == 100);
        }

        [Fact]
        public async Task GetById_WithExistingId_ReturnsInventory()
        {
            // Act
            var inventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");

            // Assert
            inventory.Should().NotBeNull();
            inventory!.Id.Should().Be(1);
            inventory.ProductId.Should().Be(1);
            inventory.LocationId.Should().Be(1);
            inventory.Quantity.Should().Be(100);
            inventory.LocationName.Should().Be("Main Warehouse");
        }

        [Fact]
        public async Task GetById_WithNonExistingId_Returns404()
        {
            // Act
            var response = await Client.GetAsync("/api/v1/inventory/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByProductId_ReturnsInventoriesForProduct()
        {
            // Act
            var inventories = await GetAsync<List<InventoryDto>>("/api/v1/inventory/by-product/1");

            // Assert
            inventories.Should().NotBeNull();
            inventories.Should().HaveCount(2);
            inventories.Should().AllSatisfy(i => i.ProductId.Should().Be(1));
        }

        [Fact]
        public async Task GetByLocationId_ReturnsInventoriesForLocation()
        {
            // Act
            var inventories = await GetAsync<List<InventoryDto>>("/api/v1/inventory/by-location/1");

            // Assert
            inventories.Should().NotBeNull();
            inventories.Should().HaveCount(2);
            inventories.Should().AllSatisfy(i => i.LocationId.Should().Be(1));
        }

        [Fact]
        public async Task Create_WithValidData_CreatesNewInventory()
        {
            // Arrange
            var newInventory = new CreateInventoryDto
            {
                ProductId = 4,
                LocationId = 1,
                Quantity = 200
            };

            // Act
            var response = await PostAsync("/api/v1/inventory", newInventory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var createdInventory = await DeserializeResponse<InventoryDto>(response);
            createdInventory.Should().NotBeNull();
            createdInventory!.ProductId.Should().Be(4);
            createdInventory.LocationId.Should().Be(1);
            createdInventory.Quantity.Should().Be(200);
        }

        [Fact]
        public async Task Create_WithDuplicateProductAndLocation_Returns400()
        {
            // Arrange
            var duplicateInventory = new CreateInventoryDto
            {
                ProductId = 1,
                LocationId = 1,
                Quantity = 50
            };

            // Act
            var response = await PostAsync("/api/v1/inventory", duplicateInventory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("already exists");
        }

        [Fact]
        public async Task Create_WithNonExistentLocation_Returns404()
        {
            // Arrange
            var inventory = new CreateInventoryDto
            {
                ProductId = 1,
                LocationId = 999,
                Quantity = 50
            };

            // Act
            var response = await PostAsync("/api/v1/inventory", inventory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateQuantity_WithValidData_UpdatesInventory()
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { Quantity = 150 };

            // Act
            var response = await PutAsync("/api/v1/inventory/1/quantity", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedInventory = await DeserializeResponse<InventoryDto>(response);
            updatedInventory.Should().NotBeNull();
            updatedInventory!.Quantity.Should().Be(150);
        }

        [Fact]
        public async Task AddStock_IncreasesInventoryQuantity()
        {
            // Arrange
            var addStockRequest = new AddStockRequest(25, "PO-TEST-001", "Integration test stock addition");

            // Get initial quantity
            var initialInventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");
            var initialQuantity = initialInventory!.Quantity;

            // Act
            var response = await PostAsync("/api/v1/inventory/1/add-stock", addStockRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedInventory = await DeserializeResponse<InventoryDto>(response);
            updatedInventory.Should().NotBeNull();
            updatedInventory!.Quantity.Should().Be(initialQuantity + 25);

            // Verify transaction was created
            var transactions = await GetAsync<List<InventoryTransactionDto>>("/api/v1/inventorytransaction/by-inventory/1");
            transactions.Should().Contain(t =>
                t.Type == Domain.Entities.TransactionType.StockIn &&
                t.Quantity == 25 &&
                t.Reference == "PO-TEST-001");
        }

        [Fact]
        public async Task RemoveStock_WithSufficientStock_DecreasesInventoryQuantity()
        {
            // Arrange
            var removeStockRequest = new RemoveStockRequest(20, "SO-TEST-001", "Integration test stock removal");

            // Get initial quantity
            var initialInventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");
            var initialQuantity = initialInventory!.Quantity;

            // Act
            var response = await PostAsync("/api/v1/inventory/1/remove-stock", removeStockRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedInventory = await DeserializeResponse<InventoryDto>(response);
            updatedInventory.Should().NotBeNull();
            updatedInventory!.Quantity.Should().Be(initialQuantity - 20);
        }

        [Fact]
        public async Task RemoveStock_WithInsufficientStock_Returns400()
        {
            // Arrange
            var removeStockRequest = new RemoveStockRequest(1000, "SO-TEST-002", "This should fail");

            // Act
            var response = await PostAsync("/api/v1/inventory/1/remove-stock", removeStockRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Not enough stock");
        }

        [Fact]
        public async Task V2_Search_WithFilters_ReturnsFilteredResults()
        {
            // Act
            var response = await Client.GetAsync("/api/v2/inventory/search?productId=1&minQuantity=50");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var inventories = await DeserializeResponse<List<InventoryDto>>(response);
            inventories.Should().NotBeNull();
            inventories.Should().AllSatisfy(i =>
            {
                i.ProductId.Should().Be(1);
                i.Quantity.Should().BeGreaterThanOrEqualTo(50);
            });
        }

        [Fact]
        public async Task V2_GetLowStock_ReturnsLowStockItems()
        {
            // Act
            var response = await Client.GetAsync("/api/v2/inventory/low-stock?threshold=30");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var lowStockItems = await DeserializeResponse<List<LowStockItemDto>>(response);
            lowStockItems.Should().NotBeNull();
            lowStockItems.Should().HaveCount(1); // Only Product 3 in Store A has quantity 25
            lowStockItems![0].CurrentQuantity.Should().Be(25);
            lowStockItems[0].Threshold.Should().Be(30);
        }
    }
}
