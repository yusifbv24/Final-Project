using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using ProductService.Application.DTOs;
using ProductService.Application.Events;
using ProductService.Application.Hubs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Products.Commands
{
    public class CreateProduct
    {
        public record Command(CreateProductDto ProductDto) : IRequest<ProductDto>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
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
            private readonly IHubContext<ProductHub> _hubContext;


            public Handler(
                IProductRepository productRepository,
                ICategoryRepository categoryRepository,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IMessagePublisher messagePublisher,
                IHubContext<ProductHub> hubContext)
            {
                _productRepository = productRepository;
                _categoryRepository = categoryRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _messagePublisher = messagePublisher;
                _hubContext = hubContext;
            }

            public async Task<ProductDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validate that category exists
                var categoryExists = await _categoryRepository.ExistsByIdAsync(request.ProductDto.CategoryId, cancellationToken);
                if (!categoryExists)
                    throw new NotFoundException($"Category with ID {request.ProductDto.CategoryId} not found");

                var product = _mapper.Map<Product>(request.ProductDto);

                var result = await _productRepository.AddAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish product created event
                await _messagePublisher.PublishAsync(
                    new ProductCreatedEvent(
                        product.Id,
                        product.Name,
                        product.SKU,
                        product.Price,
                        product.CategoryId,
                        product.CreatedAt),
                    "product.created",
                    cancellationToken);

                // Notify via SignalR
                await _hubContext.Clients.All.SendAsync("ProductCreated", product.Id, product.Name, cancellationToken);

                // Fetch with category details
                var savedProduct = await _productRepository.GetByIdAsync(result.Id, cancellationToken);
                return _mapper.Map<ProductDto>(savedProduct);
            }
        }
    }
}
