using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.InventoryTransaction.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.InventoryTransaction.Queries
{
    public class GetAllTransactionsTests
    {
        private readonly Mock<IInventoryTransactionRepository> _transactionRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetAllTransactions.Handler _handler;

        public GetAllTransactionsTests()
        {
            _transactionRepositoryMock = new Mock<IInventoryTransactionRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetAllTransactions.Handler(_transactionRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ReturnsAllTransactions()
        {
            // Arrange
            var transactions = new List<InventoryService.Domain.Entities.InventoryTransaction>
            {
                new(1, TransactionType.StockIn, 10, "PO-001", "Stock in"),
                new(2, TransactionType.StockOut, 5, "SO-001", "Stock out"),
                new(3, TransactionType.Adjustment, 20, "ADJ-001", "Adjustment")
            };

            _transactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var query = new GetAllTransactions.Query();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Select(x => x.Type).Should().BeEquivalentTo(new[] {
                TransactionType.StockIn,
                TransactionType.StockOut,
                TransactionType.Adjustment
            });
        }

        [Fact]
        public async Task Handle_WithNoTransactions_ReturnsEmptyList()
        {
            // Arrange
            _transactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryService.Domain.Entities.InventoryTransaction>());

            var query = new GetAllTransactions.Query();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
