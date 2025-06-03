using System.Net;
using FluentAssertions;
using InventoryService.Application.DTOs;
using InventoryService.IntegrationTests.Infrastructure;

namespace InventoryService.IntegrationTests.Controllers
{
    public class LocationControllerIntegrationTests : IntegrationTestBase
    {
        public LocationControllerIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetAll_ReturnsAllLocations()
        {
            // Act
            var locations = await GetAsync<List<LocationDto>>("/api/v1/location");

            // Assert
            locations.Should().NotBeNull();
            locations.Should().HaveCount(3);
            locations.Should().Contain(l => l.Code == "MW001" && l.Name == "Main Warehouse");
        }

        [Fact]
        public async Task GetById_WithExistingId_ReturnsLocation()
        {
            // Act
            var location = await GetAsync<LocationDto>("/api/v1/location/1");

            // Assert
            location.Should().NotBeNull();
            location!.Id.Should().Be(1);
            location.Name.Should().Be("Main Warehouse");
            location.Code.Should().Be("MW001");
        }

        [Fact]
        public async Task GetByCode_WithExistingCode_ReturnsLocation()
        {
            // Act
            var location = await GetAsync<LocationDto>("/api/v1/location/by-code/MW001");

            // Assert
            location.Should().NotBeNull();
            location!.Code.Should().Be("MW001");
            location.Name.Should().Be("Main Warehouse");
        }

        [Fact]
        public async Task Create_WithValidData_CreatesNewLocation()
        {
            // Arrange
            var newLocation = new CreateLocationDto
            {
                Name = "Test Warehouse",
                Code = "TW001",
                Description = "Integration test warehouse"
            };

            // Act
            var response = await PostAsync("/api/v1/location", newLocation);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdLocation = await DeserializeResponse<LocationDto>(response);
            createdLocation.Should().NotBeNull();
            createdLocation!.Name.Should().Be("Test Warehouse");
            createdLocation.Code.Should().Be("TW001");
            createdLocation.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Create_WithDuplicateCode_Returns400()
        {
            // Arrange
            var duplicateLocation = new CreateLocationDto
            {
                Name = "Duplicate Warehouse",
                Code = "MW001", // Already exists
                Description = "This should fail"
            };

            // Act
            var response = await PostAsync("/api/v1/location", duplicateLocation);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("already exists");
        }

        [Fact]
        public async Task Update_WithValidData_UpdatesLocation()
        {
            // Arrange
            var updateDto = new UpdateLocationDto
            {
                Name = "Updated Main Warehouse",
                Code = "UMW001",
                Description = "Updated description"
            };

            // Act
            var response = await PutAsync("/api/v1/location/1", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedLocation = await DeserializeResponse<LocationDto>(response);
            updatedLocation.Should().NotBeNull();
            updatedLocation!.Name.Should().Be("Updated Main Warehouse");
            updatedLocation.Code.Should().Be("UMW001");
        }

        [Fact]
        public async Task Delete_WithNoInventory_DeletesLocation()
        {
            // Arrange - Create a location without inventory
            var newLocation = new CreateLocationDto
            {
                Name = "To Delete",
                Code = "DEL001",
                Description = "Will be deleted"
            };
            var createResponse = await PostAsync("/api/v1/location", newLocation);
            var createdLocation = await DeserializeResponse<LocationDto>(createResponse);

            // Act
            var response = await DeleteAsync($"/api/v1/location/{createdLocation!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify it was deleted
            var getResponse = await Client.GetAsync($"/api/v1/location/{createdLocation.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_WithExistingInventory_Returns400()
        {
            // Act - Try to delete location that has inventory
            var response = await DeleteAsync("/api/v1/location/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("has inventory items");
        }

        [Fact]
        public async Task V2_GetLocationsWithInventory_ReturnsLocationSummary()
        {
            // Act
            var response = await Client.GetAsync("/api/v2/location/with-inventory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var locations = await DeserializeResponse<List<LocationWithInventoryDto>>(response);
            locations.Should().NotBeNull();
            locations.Should().HaveCount(3);

            var mainWarehouse = locations!.First(l => l.Code == "MW001");
            mainWarehouse.TotalItems.Should().Be(2);
            mainWarehouse.TotalUniqueProducts.Should().Be(2);
            mainWarehouse.TotalQuantity.Should().Be(150); // 100 + 50
        }
    }
}