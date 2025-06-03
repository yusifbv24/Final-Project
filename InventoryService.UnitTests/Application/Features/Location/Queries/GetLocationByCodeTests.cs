using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Features.Location.Queries;
using InventoryService.Application.Mapping;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using Moq;

namespace InventoryService.UnitTests.Application.Features.Location.Queries
{
    public class GetLocationByCodeTests
    {
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetLocationByCode.Handler _handler;

        public GetLocationByCodeTests()
        {
            _locationRepositoryMock = new Mock<ILocationRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _handler = new GetLocationByCode.Handler(_locationRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_WithExistingCode_ReturnsLocation()
        {
            // Arrange
            var code = "TL001";
            var location = new InventoryService.Domain.Entities.Location("Test Location", code, "Test Description");
            var query = new GetLocationByCode.Query(code);

            _locationRepositoryMock.Setup(x => x.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(code);
            result.Name.Should().Be("Test Location");
        }

        [Fact]
        public async Task Handle_WithNonExistingCode_ThrowsNotFoundException()
        {
            // Arrange
            var code = "NOTEXIST";
            var query = new GetLocationByCode.Query(code);

            _locationRepositoryMock.Setup(x => x.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryService.Domain.Entities.Location?)null);

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Location with code '{code}' not found");
        }

        [Theory]
        [InlineData("ABC123")]
        [InlineData("abc123")]
        [InlineData("ABC-123")]
        [InlineData("ABC_123")]
        public async Task Handle_WithDifferentCodeFormats_ReturnsLocation(string code)
        {
            // Arrange
            var location = new InventoryService.Domain.Entities.Location("Test Location", code, "Test Description");
            var query = new GetLocationByCode.Query(code);

            _locationRepositoryMock.Setup(x => x.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(code);
        }
    }
}
