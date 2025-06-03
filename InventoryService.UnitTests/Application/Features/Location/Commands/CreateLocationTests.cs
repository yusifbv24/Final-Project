using AutoMapper;
using FluentAssertions;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.Location.Commands;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Location.Commands
{
    public class CreateLocationTests
    {
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly CreateLocation.Handler _handler;

        public CreateLocationTests()
        {
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new CreateLocation.Handler(
                _locationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _mapper);
        }

        [Fact]
        public async Task Handle_WithValidData_CreatesLocation()
        {
            // Arrange
            var dto = new CreateLocationDto
            {
                Name = "Test Location",
                Code = "TL001",
                Description = "Test Description"
            };
            var command = new CreateLocation.Command(dto);

            _locationRepositoryMock.Setup(x => x.ExistsByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _locationRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.Location>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Location loc, CancellationToken ct) => loc);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            result.Code.Should().Be(dto.Code);
            result.Description.Should().Be(dto.Description);

            _locationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.Location>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithExistingCode_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateLocationDto
            {
                Name = "Test Location",
                Code = "EXISTING",
                Description = "Test Description"
            };
            var command = new CreateLocation.Command(dto);

            _locationRepositoryMock.Setup(x => x.ExistsByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Location with code '{dto.Code}' already exists");
        }
    }
}
