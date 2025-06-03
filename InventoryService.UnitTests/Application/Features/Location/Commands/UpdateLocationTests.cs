using AutoMapper;
using FluentAssertions;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.Location.Commands;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Location.Commands
{
    public class UpdateLocationTests
    {
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly UpdateLocation.Handler _handler;

        public UpdateLocationTests()
        {
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new UpdateLocation.Handler(
                _locationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _mapper);
        }

        [Fact]
        public async Task Handle_WithValidData_UpdatesLocation()
        {
            // Arrange
            var locationId = 1;
            var location = new InventoryService.Domain.Entities.Location("Old Name", "OLD001", "Old Description");
            var dto = new UpdateLocationDto
            {
                Name = "New Name",
                Code = "NEW001",
                Description = "New Description"
            };
            var command = new UpdateLocation.Command(locationId, dto);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);
            _locationRepositoryMock.Setup(x => x.ExistsByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            result.Code.Should().Be(dto.Code);
            result.Description.Should().Be(dto.Description);

            _locationRepositoryMock.Verify(x => x.UpdateAsync(location, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithSameCode_UpdatesWithoutCheckingDuplicate()
        {
            // Arrange
            var locationId = 1;
            var location = new InventoryService.Domain.Entities.Location("Old Name", "CODE001", "Old Description");
            var dto = new UpdateLocationDto
            {
                Name = "New Name",
                Code = "CODE001", // Same code
                Description = "New Description"
            };
            var command = new UpdateLocation.Command(locationId, dto);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            _locationRepositoryMock.Verify(x => x.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithExistingCode_ThrowsInvalidOperationException()
        {
            // Arrange
            var locationId = 1;
            var location = new InventoryService.Domain.Entities.Location("Old Name", "OLD001", "Old Description");
            var dto = new UpdateLocationDto
            {
                Name = "New Name",
                Code = "EXISTING",
                Description = "New Description"
            };
            var command = new UpdateLocation.Command(locationId, dto);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);
            _locationRepositoryMock.Setup(x => x.ExistsByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Location with code '{dto.Code}' already exists");
        }

        [Fact]
        public async Task Handle_WithNonExistentLocation_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new UpdateLocationDto
            {
                Name = "New Name",
                Code = "NEW001",
                Description = "New Description"
            };
            var command = new UpdateLocation.Command(999, dto);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Location?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Location with ID 999 not found");
        }
    }
}
