using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Inventory.Queries;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Inventory.Queries
{
    public class GetInventoryByProductIdTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<IProductServiceClient> _productServiceClientMock;
        private readonly IMapper _mapper;
        private readonly GetInventoryByProductId.Handler _handler;

        public GetInventoryByProductIdTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _productServiceClientMock = new Mock<IProductServiceClient>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetInventoryByProductId.Handler(
                _inventoryRepositoryMock.Object,
                _productServiceClientMock.Object,
                _mapper);
        }

        [Fact]
        public async Task Handle_WithExistingProduct_ReturnsInventories()
        {
            // Arrange
            var productId = 1;
            var inventories = new List<InventoryService.Domain.Entities.Inventory>
            {
                new(productId, 1, 100),
                new(productId, 2, 50)
            };
            var query = new GetInventoryByProductId.Query(productId);

            _productServiceClientMock.Setup(x => x.ProductExistsAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _inventoryRepositoryMock.Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(x => x.ProductId.Should().Be(productId));
        }

        [Fact]
        public async Task Handle_WithNonExistingProduct_ThrowsNotFoundException()
        {
            // Arrange
            var productId = 999;
            var query = new GetInventoryByProductId.Query(productId);

            _productServiceClientMock.Setup(x => x.ProductExistsAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Product with ID {productId} not found");
        }
    }
}
