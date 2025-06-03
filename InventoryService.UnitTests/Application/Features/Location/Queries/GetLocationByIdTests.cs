using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Location.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Location.Queries
{
    public class GetLocationByIdTests
    {
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetLocationById.Handler _handler;

        public GetLocationByIdTests()
        {
            _locationRepositoryMock = new Mock<ILocationRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetLocationById.Handler(_locationRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_WithExistingId_ReturnsLocation()
        {
            // Arrange
            var locationId = 1;
            var location = new InventoryService.Domain.Entities.Location("Test Location", "TL001", "Test Description");
            var query = new GetLocationById.Query(locationId);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Location");
            result.Code.Should().Be("TL001");
            result.Description.Should().Be("Test Description");
        }

        [Fact]
        public async Task Handle_WithNonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var query = new GetLocationById.Query(999);

            _locationRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Location?)null);

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Location with ID 999 not found");
        }
    }
}
