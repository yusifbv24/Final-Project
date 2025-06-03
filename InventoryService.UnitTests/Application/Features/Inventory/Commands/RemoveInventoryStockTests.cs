using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Inventory.Commands;
using InventoryService.Application.Hubs;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Inventory.Commands
{
    public class RemoveInventoryStockTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<IInventoryTransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IHubContext<InventoryHub>> _hubContextMock;
        private readonly IMapper _mapper;
        private readonly RemoveInventoryStock.Handler _handler;

        public RemoveInventoryStockTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _transactionRepositoryMock = new Mock<IInventoryTransactionRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _hubContextMock = new Mock<IHubContext<InventoryHub>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            SetupHubMock();

            _handler = new RemoveInventoryStock.Handler(
                _inventoryRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _messagePublisherMock.Object,
                _mapper,
                _hubContextMock.Object);
        }

        private void SetupHubMock()
        {
            var mockClients = new Mock<IHubClients>();
            var mockClient = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClient.Object);
            _hubContextMock.Setup(x => x.Clients).Returns(mockClients.Object);
        }

        [Fact]
        public async Task Handle_WithValidRequest_RemovesStock()
        {
            // Arrange
            var inventoryId = 1;
            var quantityToRemove = 5;
            var command = new RemoveInventoryStock.Command(inventoryId, quantityToRemove, "SO-001", "Test removal");
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 10);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            inventory.Quantity.Should().Be(5);

            _inventoryRepositoryMock.Verify(x => x.UpdateAsync(inventory, It.IsAny<CancellationToken>()), Times.Once);
            _transactionRepositoryMock.Verify(x => x.AddAsync(
                It.Is<Domain.Entities.InventoryTransaction>(t => t.Type == TransactionType.StockOut && t.Quantity == quantityToRemove),
                It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WithInsufficientStock_ThrowsInvalidOperationException()
        {
            // Arrange
            var inventoryId = 1;
            var command = new RemoveInventoryStock.Command(inventoryId, 20, "SO-001", "Test removal");
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 10);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Not enough stock available. Current quantity: 10");
        }

        [Fact]
        public async Task Handle_WithNonExistentInventory_ThrowsNotFoundException()
        {
            // Arrange
            var command = new RemoveInventoryStock.Command(999, 5, "SO-001", "Test removal");

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Inventory?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Inventory with ID 999 not found");
        }
    }
}
