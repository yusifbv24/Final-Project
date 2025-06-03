using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Inventory.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Inventory.Queries
{
    public class GetInventoryByIdTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetInventoryById.Handler _handler;

        public GetInventoryByIdTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetInventoryById.Handler(_inventoryRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_WithExistingId_ReturnsInventory()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 100);
            var query = new GetInventoryById.Query(inventoryId);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(inventoryId);
            result.Quantity.Should().Be(100);
        }

        [Fact]
        public async Task Handle_WithNonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var query = new GetInventoryById.Query(999);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Inventory?)null);

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Inventory with ID 999 not found");
        }
    }
}
