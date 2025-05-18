using AutoMapper;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Categories.Queries
{
    public class GetCategoryById
    {
        public record Query(int Id) : IRequest<CategoryDto>;

        public class Handler : IRequestHandler<Query, CategoryDto>
        {
            private readonly ICategoryRepository _categoryRepository;
            private readonly IMapper _mapper;

            public Handler(ICategoryRepository categoryRepository, IMapper mapper)
            {
                _categoryRepository = categoryRepository;
                _mapper = mapper;
            }

            public async Task<CategoryDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

                if (category == null)
                    throw new NotFoundException($"Category with ID {request.Id} not found");

                return _mapper.Map<CategoryDto>(category);
            }
        }
    }
}
