using AutoMapper;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Products.Queries
{
    public class GetAllProducts
    {
        public record Query : IRequest<IEnumerable<ProductDto>>;

        public class Handler : IRequestHandler<Query, IEnumerable<ProductDto>>
        {
            private readonly IProductRepository _productRepository;
            private readonly IMapper _mapper;

            public Handler(IProductRepository productRepository, IMapper mapper)
            {
                _productRepository = productRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<ProductDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var products = await _productRepository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
        }
    }
}
