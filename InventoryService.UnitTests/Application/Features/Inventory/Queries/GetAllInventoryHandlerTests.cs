using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Inventory.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Inventory.Queries
{
    public class GetAllInventoryHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetAllInventory.Handler _handler;
        public GetAllInventoryHandlerTests()
        {
            _inventoryRepositoryMock=new Mock<IInventoryRepository>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())).CreateMapper();
            _handler=new GetAllInventory.Handler(_inventoryRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ReturnsAllInventories()
        {
            // Arrange
            var inventories = new List<InventoryService.Domain.Entities.Inventory>
            {
                new(1, 1, 10),
                new(2, 1, 20),
                new(3, 2, 30)
            };

            _inventoryRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventories);

            // Act
            var result = await _handler.Handle(new GetAllInventory.Query(), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Select(x=>x.Quantity).Should().BeEquivalentTo([10, 20, 30]);
        }

        [Fact]
        public async Task Handle_WithNoInventories_ReturnsEmptyList()
        {
            // Arrange
            _inventoryRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryService.Domain.Entities.Inventory>());
            // Act
            var result = await _handler.Handle(new GetAllInventory.Query(), CancellationToken.None);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
