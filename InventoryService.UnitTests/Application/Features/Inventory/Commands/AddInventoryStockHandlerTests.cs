using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Inventory.Commands;
using InventoryService.Application.Hubs;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Inventory.Commands
{
    public class AddInventoryStockHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<IInventoryTransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IHubContext<InventoryHub>> _hubContextMock;
        private readonly IMapper _mapper;
        private readonly AddInventoryStock.Handler _handler;
        public AddInventoryStockHandlerTests()
        {
            _inventoryRepositoryMock= new Mock<IInventoryRepository>();
            _transactionRepositoryMock = new Mock<IInventoryTransactionRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _hubContextMock = new Mock<IHubContext<InventoryHub>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            _mapper = config.CreateMapper();

            var mockClients= new Mock<IHubClients>();
            var mockClient=new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClient.Object);
            _hubContextMock.Setup(x => x.Clients).Returns(mockClients.Object);

            _handler= new AddInventoryStock.Handler(
                _inventoryRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _messagePublisherMock.Object,
                _mapper,
                _hubContextMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidRequest_AddsStock()
        {
            // Arrange
            var inventoryId = 1;
            var quantity = 5;
            var command = new AddInventoryStock.Command(inventoryId, quantity, "PO-001", "Test notes");
            var inventory = new Domain.Entities.Inventory(1, 1, 10);

            // Mock both GetByIdAsync calls
            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Inventory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.InventoryTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.InventoryTransaction t, CancellationToken ct) => t);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Quantity.Should().Be(15); 

            _inventoryRepositoryMock.Verify(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WithNonExistentInventory_ThrowsNotFoundException()
        {
            // Arrange
            var command = new AddInventoryStock.Command(999, 5, "PO-001", "Test notes");
            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Inventory?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            _inventoryRepositoryMock.Verify(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
