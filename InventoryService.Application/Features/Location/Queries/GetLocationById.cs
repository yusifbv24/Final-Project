using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Location.Queries
{
    public class GetLocationById
    {
        public record Query(int Id) : IRequest<LocationDto>;

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
                var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Location with ID {request.Id} not found");

                return _mapper.Map<LocationDto>(location);
            }
        }
    }
}