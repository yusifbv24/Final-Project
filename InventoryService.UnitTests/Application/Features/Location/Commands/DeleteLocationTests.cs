using FluentAssertions;
using InventoryService.Application.Features.Location.Commands;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Location.Commands
{
    public class DeleteLocationTests
    {
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DeleteLocation.Handler _handler;

        public DeleteLocationTests()
        {
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new DeleteLocation.Handler(
                _locationRepositoryMock.Object,
                _inventoryRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_WithLocationWithoutInventory_DeletesLocation()
        {
            // Arrange
            var locationId = 1;
            var location = new InventoryService.Domain.Entities.Location("Test Location", "TL001", "Test Description");
            var command = new DeleteLocation.Command(locationId);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);
            _inventoryRepositoryMock.Setup(x => x.GetByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryService.Domain.Entities.Inventory>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _locationRepositoryMock.Verify(x => x.DeleteAsync(location, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithLocationWithInventory_ThrowsInvalidOperationException()
        {
            // Arrange
            var locationId = 1;
            var location = new InventoryService.Domain.Entities.Location("Test Location", "TL001", "Test Description");
            var inventories = new List<InventoryService.Domain.Entities.Inventory>
            {
                new(1, locationId, 10),
                new(2, locationId, 20)
            };
            var command = new DeleteLocation.Command(locationId);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);
            _inventoryRepositoryMock.Setup(x => x.GetByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventories);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Cannot delete location with ID {locationId} as it has inventory items. Move or delete the inventory items first.");
        }

        [Fact]
        public async Task Handle_WithNonExistentLocation_ThrowsNotFoundException()
        {
            // Arrange
            var command = new DeleteLocation.Command(999);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Location?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Location with ID 999 not found");
        }
    }
}
