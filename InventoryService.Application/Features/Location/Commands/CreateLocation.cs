using AutoMapper;
using FluentValidation;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Location.Commands
{
    public class CreateLocation
    {
        public record Command(CreateLocationDto LocationDto) : IRequest<LocationDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.LocationDto.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

                RuleFor(x => x.LocationDto.Code)
                    .NotEmpty().WithMessage("Code is required")
                    .MaximumLength(20).WithMessage("Code must not exceed 20 characters");

                RuleFor(x => x.LocationDto.Description)
                    .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
            }
        }

        public class Handler : IRequestHandler<Command, LocationDto>
        {
            private readonly ILocationRepository _locationRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(
                ILocationRepository locationRepository,
                IUnitOfWork unitOfWork,
                IMapper mapper)
            {
                _locationRepository = locationRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<LocationDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Check if location code is already used
                var codeExists = await _locationRepository.ExistsByCodeAsync(
                    request.LocationDto.Code, cancellationToken);

                if (codeExists)
                    throw new InvalidOperationException($"Location with code '{request.LocationDto.Code}' already exists");

                var location = _mapper.Map<Domain.Entities.Location>(request.LocationDto);
                var result = await _locationRepository.AddAsync(location, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return _mapper.Map<LocationDto>(result);
            }
        }
    }
}
