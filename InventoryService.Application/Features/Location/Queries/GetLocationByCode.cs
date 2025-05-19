using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Location.Queries
{
    public class GetLocationByCode
    {
        public record Query(string Code) : IRequest<LocationDto>;

        public class Handler : IRequestHandler<Query, LocationDto>
        {
            private readonly ILocationRepository _locationRepository;
            private readonly IMapper _mapper;

            public Handler(ILocationRepository locationRepository, IMapper mapper)
            {
                _locationRepository = locationRepository;
                _mapper = mapper;
            }

            public async Task<LocationDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var location = await _locationRepository.GetByCodeAsync(request.Code, cancellationToken)
                    ?? throw new NotFoundException($"Location with code '{request.Code}' not found");

                return _mapper.Map<LocationDto>(location);
            }
        }
    }
}