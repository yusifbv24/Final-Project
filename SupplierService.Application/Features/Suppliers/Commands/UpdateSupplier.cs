using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Exceptions;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.Suppliers.Commands
{
    public static class UpdateSupplier
    {
        public record Command(int Id, UpdateSupplierDto SupplierDto) : IRequest<SupplierDto>;

        public class Handler : IRequestHandler<Command, SupplierDto>
        {
            private readonly ISupplierRepository _supplierRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IMessagePublisher _messagePublisher;

            public Handler(
                ISupplierRepository supplierRepository,
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IMessagePublisher messagePublisher)
            {
                _supplierRepository = supplierRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _messagePublisher = messagePublisher;
            }

            public async Task<SupplierDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Get supplier by id
                var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException($"Supplier with ID {request.Id} not found");

                // Check if email is already taken by another supplier
                var existingSupplier = await _supplierRepository.GetByEmailAsync(request.SupplierDto.Email, cancellationToken);
                if (existingSupplier != null && existingSupplier.Id != request.Id)
                {
                    throw new InvalidOperationException($"Email {request.SupplierDto.Email} is already in use by another supplier");
                }

                // Update supplier
                supplier.Update(
                    request.SupplierDto.Name,
                    request.SupplierDto.ContactName,
                    request.SupplierDto.Email,
                    request.SupplierDto.Phone,
                    request.SupplierDto.Address,
                    request.SupplierDto.Website,
                    request.SupplierDto.Notes
                );

                // Update repository
                await _supplierRepository.UpdateAsync(supplier, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event
                await _messagePublisher.PublishAsync(
                    new SupplierUpdatedEvent
                    {
                        SupplierId = supplier.Id,
                        Name = supplier.Name,
                        Email = supplier.Email
                    },
                    "suppliers.updated",
                    cancellationToken);

                // Return mapped DTO
                return _mapper.Map<SupplierDto>(supplier);
            }
        }

        public record SupplierUpdatedEvent
        {
            public int SupplierId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
