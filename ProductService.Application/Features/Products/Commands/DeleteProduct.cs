using MediatR;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Products.Commands
{
    public class DeleteProduct
    {
        public record Command(int Id) : IRequest<bool>;

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly IProductRepository _productRepository;
            private readonly IUnitOfWork _unitOfWork;

            public Handler(IProductRepository productRepository, IUnitOfWork unitOfWork)
            {
                _productRepository = productRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

                if (product == null)
                    throw new NotFoundException($"Product with ID {request.Id} not found");

                await _productRepository.DeleteAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }
}
