using AutoMapper;
using MediatR;
using SupplierService.Application.DTOs;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Repositories;

namespace SupplierService.Application.Features.Suppliers.Commands
{
    public static class CreateSupplier
    {
        public record Command(CreateSupplierDto SupplierDto) : IRequest<SupplierDto>;

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
                // Check if supplier with the same email already exists
                if (await _supplierRepository.ExistsByEmailAsync(request.SupplierDto.Email, cancellationToken))
                {
                    throw new InvalidOperationException($"Supplier with email {request.SupplierDto.Email} already exists");
                }

                // Create new supplier entity
                var supplier = new Supplier(
                    request.SupplierDto.Name,
                    request.SupplierDto.ContactName,
                    request.SupplierDto.Email,
                    request.SupplierDto.Phone,
                    request.SupplierDto.Address,
                    request.SupplierDto.Website,
                    request.SupplierDto.Notes
                );

                // Add to repository
                await _supplierRepository.AddAsync(supplier, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event
                await _messagePublisher.PublishAsync(
                    new SupplierCreatedEvent
                    {
                        SupplierId = supplier.Id,
                        Name = supplier.Name,
                        Email = supplier.Email
                    },
                    "suppliers.created",
                    cancellationToken);

                // Return mapped DTO
                return _mapper.Map<SupplierDto>(supplier);
            }
        }

        public record SupplierCreatedEvent
        {
            public int SupplierId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
