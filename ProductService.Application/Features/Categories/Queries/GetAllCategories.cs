﻿using AutoMapper;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Categories.Queries
{
    public class GetAllCategories
    {
        public record Query : IRequest<IEnumerable<CategoryDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<CategoryDto>>
        {
            private readonly ICategoryRepository _categoryRepository;
            private readonly IMapper _mapper;

            public Handler(ICategoryRepository categoryRepository, IMapper mapper)
            {
                _categoryRepository = categoryRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<CategoryDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var categories = await _categoryRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<CategoryDto>>(categories);
            }
        }
    }
}
