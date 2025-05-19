
using AutoMapper;
using FluentValidation;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Exceptions;
using InventoryService.Domain.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Location.Commands
{
    public class UpdateLocation
    {
        public record Command(int Id, UpdateLocationDto LocationDto) : IRequest<LocationDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Invalid location ID");

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
                var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Location with ID {request.Id} not found");

                // Check if the code is changed and if the new code is already used
                if (location.Code != request.LocationDto.Code)
                {
                    var codeExists = await _locationRepository.ExistsByCodeAsync(
                        request.LocationDto.Code, cancellationToken);

                    if (codeExists)
                        throw new InvalidOperationException($"Location with code '{request.LocationDto.Code}' already exists");
                }

                location.Update(
                    request.LocationDto.Name,
                    request.LocationDto.Code,
                    request.LocationDto.Description
                );

                await _locationRepository.UpdateAsync(location, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return _mapper.Map<LocationDto>(location);
            }
        }
    }
}
