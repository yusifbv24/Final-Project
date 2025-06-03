using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.InventoryTransaction.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.InventoryTransaction.Queries
{
    public class GetTransactionsByInventoryIdTests
    {
        private readonly Mock<IInventoryTransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetTransactionsByInventoryId.Handler _handler;

        public GetTransactionsByInventoryIdTests()
        {
            _transactionRepositoryMock = new Mock<IInventoryTransactionRepository>();
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetTransactionsByInventoryId.Handler(
                _transactionRepositoryMock.Object,
                _inventoryRepositoryMock.Object,
                _mapper);
        }

        [Fact]
        public async Task Handle_WithExistingInventory_ReturnsTransactions()
        {
            // Arrange
            var inventoryId = 1;
            var transactions = new List<InventoryService.Domain.Entities.InventoryTransaction>
            {
                new(inventoryId, TransactionType.StockIn, 10, "PO-001", "Stock in"),
                new(inventoryId, TransactionType.StockOut, 5, "SO-001", "Stock out"),
                new(inventoryId, TransactionType.Adjustment, 20, "ADJ-001", "Adjustment")
            };
            var query = new GetTransactionsByInventoryId.Query(inventoryId);

            _inventoryRepositoryMock.Setup(x => x.ExistsByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _transactionRepositoryMock.Setup(x => x.GetByInventoryIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(x => x.InventoryId.Should().Be(inventoryId));
        }

        [Fact]
        public async Task Handle_WithNonExistingInventory_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 999;
            var query = new GetTransactionsByInventoryId.Query(inventoryId);

            _inventoryRepositoryMock.Setup(x => x.ExistsByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Inventory with ID {inventoryId} not found");
        }

        [Fact]
        public async Task Handle_WithInventoryButNoTransactions_ReturnsEmptyList()
        {
            // Arrange
            var inventoryId = 1;
            var query = new GetTransactionsByInventoryId.Query(inventoryId);

            _inventoryRepositoryMock.Setup(x => x.ExistsByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _transactionRepositoryMock.Setup(x => x.GetByInventoryIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryService.Domain.Entities.InventoryTransaction>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
