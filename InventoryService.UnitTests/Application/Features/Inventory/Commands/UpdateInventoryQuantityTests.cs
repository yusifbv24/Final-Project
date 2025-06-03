using AutoMapper;
using FluentAssertions;
using InventoryService.Application.DTOs;
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
    public class UpdateInventoryQuantityTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IHubContext<InventoryHub>> _hubContextMock;
        private readonly IMapper _mapper;
        private readonly UpdateInventoryQuantity.Handler _handler;

        public UpdateInventoryQuantityTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _hubContextMock = new Mock<IHubContext<InventoryHub>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            SetupHubMock();

            _handler = new UpdateInventoryQuantity.Handler(
                _inventoryRepositoryMock.Object,
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
        public async Task Handle_WithValidQuantity_UpdatesInventory()
        {
            // Arrange
            var inventoryId = 1;
            var newQuantity = 50;
            var dto = new UpdateInventoryDto { Quantity = newQuantity };
            var command = new UpdateInventoryQuantity.Command(inventoryId, dto);
            var inventory = new InventoryService.Domain.Entities.Inventory(1, 1, 10);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            inventory.Quantity.Should().Be(newQuantity);

            _inventoryRepositoryMock.Verify(x => x.UpdateAsync(inventory, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), "inventory.updated", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentInventory_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new UpdateInventoryDto { Quantity = 50 };
            var command = new UpdateInventoryQuantity.Command(999, dto);

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
