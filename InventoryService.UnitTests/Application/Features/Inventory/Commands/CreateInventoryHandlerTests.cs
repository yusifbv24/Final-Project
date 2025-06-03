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
    public class CreateInventoryHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly Mock<IProductServiceClient> _productServiceClientMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IHubContext<InventoryHub>> _hubContextMock;
        private readonly IMapper _mapper;
        private readonly CreateInventory.Handler _handler;
        public CreateInventoryHandlerTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _productServiceClientMock = new Mock<IProductServiceClient>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _hubContextMock = new Mock<IHubContext<InventoryHub>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            _mapper = config.CreateMapper();

            // Setup hub mock
            var mockClients = new Mock<IHubClients>();
            var mockClient= new Mock<IClientProxy>();
            mockClients.Setup(clients=> clients.All).Returns(mockClient.Object);
            _hubContextMock.Setup(hub => hub.Clients).Returns(mockClients.Object);

            _handler= new CreateInventory.Handler(
                _inventoryRepositoryMock.Object,
                _locationRepositoryMock.Object,
                _productServiceClientMock.Object,
                _messagePublisherMock.Object,
                _unitOfWorkMock.Object,
                _mapper,
                _hubContextMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidRequest_CreatesInventory()
        {
            //Arrange
            var dto=new CreateInventoryDto { ProductId=1, LocationId=1, Quantity=10 };
            var command= new CreateInventory.Command(dto);

            _productServiceClientMock.Setup(x=>x.ProductExistsAsync(dto.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _locationRepositoryMock.Setup(x => x.ExistsByIdAsync(dto.LocationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _inventoryRepositoryMock.Setup(x => x.GetByProductAndLocationAsync(dto.ProductId, dto.LocationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Inventory?)null);

            _inventoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.Inventory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Inventory inv,CancellationToken ct)=> inv);

            _inventoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InventoryService.Domain.Entities.Inventory(dto.ProductId, dto.LocationId, dto.Quantity));

            //Act
            var result= await _handler.Handle(command, CancellationToken.None);

            //Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(dto.ProductId);
            result.LocationId.Should().Be(dto.LocationId);
            result.Quantity.Should().Be(dto.Quantity);

            _inventoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.Inventory>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), "inventory.created", It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WithNonExistentProduct_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new CreateInventoryDto { ProductId = 999, LocationId = 1, Quantity = 10 };
            var command = new CreateInventory.Command(dto);

            _productServiceClientMock.Setup(x => x.ProductExistsAsync(dto.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var act=async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Product with ID {dto.ProductId} not found");
        }

        [Fact]
        public async Task Handle_WithNonExistentLocation_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new CreateInventoryDto { ProductId = 1, LocationId = 999, Quantity = 10 };
            var command = new CreateInventory.Command(dto);

            _productServiceClientMock.Setup(x => x.ProductExistsAsync(dto.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _locationRepositoryMock.Setup(x => x.ExistsByIdAsync(dto.LocationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var act = async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Location with ID {dto.LocationId} not found");
        }

        [Fact]
        public async Task Handle_WithExistingInventory_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateInventoryDto { ProductId = 1, LocationId = 1, Quantity = 10 };
            var command = new CreateInventory.Command(dto);
            var existingInventory = new InventoryService.Domain.Entities.Inventory(dto.ProductId, dto.LocationId, dto.Quantity);

            _productServiceClientMock.Setup(x => x.ProductExistsAsync(dto.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _locationRepositoryMock.Setup(x => x.ExistsByIdAsync(dto.LocationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _inventoryRepositoryMock.Setup(x => x.GetByProductAndLocationAsync(dto.ProductId, dto.LocationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingInventory);

            // Act & Assert
            var act = async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Inventory for Product ID {dto.ProductId} at Location ID {dto.LocationId} already exists");
        }
    }
}
