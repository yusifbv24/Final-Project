using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Location.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Location.Queries
{
    public class GetAllLocationsTests
    {
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetAllLocations.Handler _handler;

        public GetAllLocationsTests()
        {
            _locationRepositoryMock = new Mock<ILocationRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetAllLocations.Handler(_locationRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ReturnsAllLocations()
        {
            // Arrange
            var locations = new List<InventoryService.Domain.Entities.Location>
            {
                new("Warehouse 1", "W1", "Main warehouse"),
                new("Warehouse 2", "W2", "Secondary warehouse"),
                new("Store 1", "S1", "Retail store")
            };

            _locationRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations);

            var query = new GetAllLocations.Query();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Select(x => x.Code).Should().BeEquivalentTo(new[] { "W1", "W2", "S1" });
        }

        [Fact]
        public async Task Handle_WithNoLocations_ReturnsEmptyList()
        {
            // Arrange
            _locationRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryService.Domain.Entities.Location>());

            var query = new GetAllLocations.Query();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
