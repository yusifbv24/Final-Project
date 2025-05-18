using MediatR;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Features.Categories.Commands
{
    public class DeleteCategory
    {
        public record Command(int Id) : IRequest<bool>;

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly ICategoryRepository _categoryRepository;
            private readonly IUnitOfWork _unitOfWork;

            public Handler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
            {
                _categoryRepository = categoryRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

                if (category == null)
                    throw new NotFoundException($"Category with ID {request.Id} not found");

                await _categoryRepository.DeleteAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }
}
