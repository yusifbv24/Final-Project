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
    public class GetTransactionByIdTests
    {
        private readonly Mock<IInventoryTransactionRepository> _transactionRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetTransactionById.Handler _handler;

        public GetTransactionByIdTests()
        {
            _transactionRepositoryMock = new Mock<IInventoryTransactionRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetTransactionById.Handler(_transactionRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_WithExistingId_ReturnsTransaction()
        {
            // Arrange
            var transactionId = 1;
            var transaction = new InventoryService.Domain.Entities.InventoryTransaction(
                1, TransactionType.StockIn, 10, "PO-001", "Test transaction");
            var query = new GetTransactionById.Query(transactionId);

            _transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be(TransactionType.StockIn);
            result.Quantity.Should().Be(10);
            result.Reference.Should().Be("PO-001");
        }

        [Fact]
        public async Task Handle_WithNonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var query = new GetTransactionById.Query(999);

            _transactionRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.InventoryTransaction?)null);

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Transaction with ID 999 not found");
        }
    }
}
