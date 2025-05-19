using AutoMapper;
using FluentValidation;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.Events;
using ProductService.Application.Interfaces;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Products.Commands
{
    public class UpdateProduct
    {
        public record Command(int Id, UpdateProductDto ProductDto) : IRequest<ProductDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Invalid product ID");

                RuleFor(x => x.ProductDto.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

                RuleFor(x => x.ProductDto.Description)
                    .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

                RuleFor(x => x.ProductDto.SKU)
                    .NotEmpty().WithMessage("SKU is required")
                    .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

                RuleFor(x => x.ProductDto.Price)
                    .GreaterThanOrEqualTo(0).WithMessage("Price must not be negative");

                RuleFor(x => x.ProductDto.CategoryId)
                    .GreaterThan(0).WithMessage("Valid category ID is required");
            }
        }

        public class Handler : IRequestHandler<Command, ProductDto>
        {
            private readonly IProductRepository _productRepository;
            private readonly ICategoryRepository _categoryRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IMessagePublisher _messagePublisher;

            public Handler(
                IProductRepository productRepository,
                ICategoryRepository categoryRepository,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IMessagePublisher messagePublisher)
            {
                _productRepository = productRepository;
                _categoryRepository = categoryRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _messagePublisher = messagePublisher;
            }

            public async Task<ProductDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

                if (product == null)
                    throw new NotFoundException($"Product with ID {request.Id} not found");

                // Validate that category exists
                var categoryExists = await _categoryRepository.ExistsByIdAsync(request.ProductDto.CategoryId, cancellationToken);
                if (!categoryExists)
                    throw new NotFoundException($"Category with ID {request.ProductDto.CategoryId} not found");

                product.Update(
                    request.ProductDto.Name,
                    request.ProductDto.Description,
                    request.ProductDto.SKU,
                    request.ProductDto.Price,
                    request.ProductDto.CategoryId
                );

                await _productRepository.UpdateAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Publish update product event

                await _messagePublisher.PublishAsync(
                    new ProductUpdatedEvent(
                        product.Id,
                        product.Name,
                        product.SKU,
                        product.Price,
                        product.CategoryId
                    ),
                    "product.updated",
                    cancellationToken
                );

                // Fetch with category details
                var updatedProduct = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
                return _mapper.Map<ProductDto>(updatedProduct);
            }
        }
    }
}
