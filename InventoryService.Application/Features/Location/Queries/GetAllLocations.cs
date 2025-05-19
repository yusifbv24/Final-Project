using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Location.Queries
{
    public class GetAllLocations
    {
        public record Query : IRequest<IEnumerable<LocationDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<LocationDto>>
        {
            private readonly ILocationRepository _locationRepository;
            private readonly IMapper _mapper;

            public Handler(ILocationRepository locationRepository, IMapper mapper)
            {
                _locationRepository = locationRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<LocationDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var locations = await _locationRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<LocationDto>>(locations);
            }
        }
    }
}