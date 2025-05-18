using AutoMapper;
using FluentValidation;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Categories.Commands
{
    public class UpdateCategory
    {
        public record Command(int Id, UpdateCategoryDto CategoryDto) : IRequest<CategoryDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Invalid category ID");

                RuleFor(x => x.CategoryDto.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

                RuleFor(x => x.CategoryDto.Description)
                    .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
            }
        }

        public class Handler : IRequestHandler<Command, CategoryDto>
        {
            private readonly ICategoryRepository _categoryRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
            {
                _categoryRepository = categoryRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<CategoryDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

                if (category == null)
                    throw new NotFoundException($"Category with ID {request.Id} not found");

                category.Update(request.CategoryDto.Name, request.CategoryDto.Description);

                await _categoryRepository.UpdateAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return _mapper.Map<CategoryDto>(category);
            }
        }
    }
}
