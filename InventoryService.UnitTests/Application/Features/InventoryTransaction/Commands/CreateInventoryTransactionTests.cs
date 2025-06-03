using AutoMapper;
using FluentAssertions;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.InventoryTransaction.Commands;
using InventoryService.Application.Hubs;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace InventoryService.UnitTests.Application.Features.InventoryTransaction.Commands
{
    public class CreateInventoryTransactionTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<IInventoryTransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IHubContext<InventoryHub>> _hubContextMock;
        private readonly IMapper _mapper;
        private readonly CreateInventoryTransaction.Handler _handler;

        public CreateInventoryTransactionTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _transactionRepositoryMock = new Mock<IInventoryTransactionRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _hubContextMock = new Mock<IHubContext<InventoryHub>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            SetupHubMock();

            _handler = new CreateInventoryTransaction.Handler(
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
        public async Task Handle_WithStockInTransaction_IncreasesInventory()
        {
            // Arrange
            var dto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockIn,
                Quantity = 10,
                Reference = "PO-001",
                Notes = "Test stock in"
            };
            var command = new CreateInventoryTransaction.Command(dto);
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 50);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.InventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
            _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.InventoryTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.InventoryTransaction t, CancellationToken ct) => t);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be(TransactionType.StockIn);
            result.Quantity.Should().Be(10);
            inventory.Quantity.Should().Be(60);

            _inventoryRepositoryMock.Verify(x => x.UpdateAsync(inventory, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WithStockOutTransaction_DecreasesInventory()
        {
            // Arrange
            var dto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockOut,
                Quantity = 10,
                Reference = "SO-001",
                Notes = "Test stock out"
            };
            var command = new CreateInventoryTransaction.Command(dto);
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 50);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.InventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
            _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.InventoryTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.InventoryTransaction t, CancellationToken ct) => t);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be(TransactionType.StockOut);
            inventory.Quantity.Should().Be(40);
        }

        [Fact]
        public async Task Handle_WithStockOutExceedingQuantity_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockOut,
                Quantity = 100,
                Reference = "SO-001",
                Notes = "Test stock out"
            };
            var command = new CreateInventoryTransaction.Command(dto);
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 50);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.InventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Not enough stock available. Current quantity: 50");
        }

        [Fact]
        public async Task Handle_WithAdjustmentTransaction_SetsNewQuantity()
        {
            // Arrange
            var dto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.Adjustment,
                Quantity = 75,
                Reference = "ADJ-001",
                Notes = "Inventory count adjustment"
            };
            var command = new CreateInventoryTransaction.Command(dto);
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 50);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.InventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
            _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.InventoryTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.InventoryTransaction t, CancellationToken ct) => t);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be(TransactionType.Adjustment);
            inventory.Quantity.Should().Be(75);
        }

        [Fact]
        public async Task Handle_WithNonExistentInventory_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new CreateInventoryTransactionDto
            {
                InventoryId = 999,
                Type = TransactionType.StockIn,
                Quantity = 10,
                Reference = "PO-001",
                Notes = "Test"
            };
            var command = new CreateInventoryTransaction.Command(dto);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.InventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Inventory?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Inventory with ID {dto.InventoryId} not found");
        }

        [Fact]
        public async Task Handle_WithTransferType_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.Transfer,
                Quantity = 10,
                Reference = "TR-001",
                Notes = "Test transfer"
            };
            var command = new CreateInventoryTransaction.Command(dto);
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 50);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.InventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Transaction type {dto.Type} is not supported by this endpoint");
        }
    }
}
