using MediatR;
using Microsoft.AspNetCore.SignalR;
using ProductService.Application.Hubs;
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
            private readonly IHubContext<ProductHub> _hubContext;

            public Handler(
                IProductRepository productRepository, 
                IUnitOfWork unitOfWork,
                IHubContext<ProductHub> hubContext)
            {
                _productRepository = productRepository;
                _unitOfWork = unitOfWork;
                _hubContext = hubContext;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

                if (product == null)
                    throw new NotFoundException($"Product with ID {request.Id} not found");

                await _productRepository.DeleteAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _hubContext.Clients.All.SendAsync("ProductDeleted", request.Id, cancellationToken);

                return true;
            }
        }
    }
}
