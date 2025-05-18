using AutoMapper;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Products.Queries
{
    public class GetProductById
    {
        public record Query(int Id) : IRequest<ProductDto>;

        public class Handler : IRequestHandler<Query, ProductDto>
        {
            private readonly IProductRepository _productRepository;
            private readonly IMapper _mapper;

            public Handler(IProductRepository productRepository, IMapper mapper)
            {
                _productRepository = productRepository;
                _mapper = mapper;
            }

            public async Task<ProductDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

                if (product == null)
                    throw new NotFoundException($"Product with ID {request.Id} not found");

                return _mapper.Map<ProductDto>(product);
            }
        }
    }
}
