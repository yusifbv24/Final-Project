using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Inventory.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Inventory.Queries
{
    public class GetInventoryByLocationIdTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetInventoryByLocationId.Handler _handler;

        public GetInventoryByLocationIdTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _locationRepositoryMock = new Mock<ILocationRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetInventoryByLocationId.Handler(
                _inventoryRepositoryMock.Object,
                _locationRepositoryMock.Object,
                _mapper);
        }

        [Fact]
        public async Task Handle_WithExistingLocation_ReturnsInventories()
        {
            // Arrange
            var locationId = 1;
            var inventories = new List<InventoryService.Domain.Entities.Inventory>
            {
                new(1, locationId, 100),
                new(2, locationId, 50),
                new(3, locationId, 75)
            };
            var query = new GetInventoryByLocationId.Query(locationId);

            _locationRepositoryMock.Setup(x => x.ExistsByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _inventoryRepositoryMock.Setup(x => x.GetByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(x => x.LocationId.Should().Be(locationId));
        }

        [Fact]
        public async Task Handle_WithNonExistingLocation_ThrowsNotFoundException()
        {
            // Arrange
            var locationId = 999;
            var query = new GetInventoryByLocationId.Query(locationId);

            _locationRepositoryMock.Setup(x => x.ExistsByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Location with ID {locationId} not found");
        }

        [Fact]
        public async Task Handle_WithLocationButNoInventory_ReturnsEmptyList()
        {
            // Arrange
            var locationId = 1;
            var query = new GetInventoryByLocationId.Query(locationId);

            _locationRepositoryMock.Setup(x => x.ExistsByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _inventoryRepositoryMock.Setup(x => x.GetByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryService.Domain.Entities.Inventory>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
