using AutoMapper;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Products.Queries
{
    public class GetProductsByCategoryId
    {
        public record Query(int CategoryId) : IRequest<IEnumerable<ProductDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<ProductDto>>
        {
            private readonly IProductRepository _productRepository;
            private readonly ICategoryRepository _categoryRepository;
            private readonly IMapper _mapper;

            public Handler(
                IProductRepository productRepository,
                ICategoryRepository categoryRepository,
                IMapper mapper)
            {
                _productRepository = productRepository;
                _categoryRepository = categoryRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<ProductDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Verify category exists
                var categoryExists = await _categoryRepository.ExistsByIdAsync(request.CategoryId, cancellationToken);
                if (!categoryExists)
                    throw new NotFoundException($"Category with ID {request.CategoryId} not found");

                var products = await _productRepository.GetByCategoryIdAsync(request.CategoryId, cancellationToken);
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
        }
    }
}
