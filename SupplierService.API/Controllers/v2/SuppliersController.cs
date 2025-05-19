using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupplierService.Application.DTOs;
using SupplierService.Application.Features.PurchaseOrders.Queries;
using SupplierService.Application.Features.Suppliers.Queries;
using SupplierService.Domain.Exceptions;

namespace SupplierService.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuppliersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll()
        {
            var suppliers = await _mediator.Send(new GetAllSuppliers.Query());
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierDto>> GetById(int id)
        {
            try
            {
                var supplier = await _mediator.Send(new GetSupplierById.Query(id));
                return Ok(supplier);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // V2 Enhanced endpoint - Search with filtering
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> Search(
            [FromQuery] string? name,
            [FromQuery] string? email,
            [FromQuery] bool? isActive)
        {
            // In a real implementation, you would add a dedicated query handler for this
            var suppliers = await _mediator.Send(new GetAllSuppliers.Query());
            var filteredSuppliers = suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                filteredSuppliers = filteredSuppliers.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(email))
                filteredSuppliers = filteredSuppliers.Where(s => s.Email.Contains(email, StringComparison.OrdinalIgnoreCase));

            if (isActive.HasValue)
                filteredSuppliers = filteredSuppliers.Where(s => s.IsActive == isActive.Value);

            return Ok(filteredSuppliers.ToList());
        }

        // V2 Enhanced endpoint - Get supplier with purchase order summary
        [HttpGet("{id}/with-orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierWithOrdersDto>> GetWithOrders(int id)
        {
            try
            {
                // In a real implementation, you would add a dedicated query handler for this
                var supplier = await _mediator.Send(new GetSupplierById.Query(id));
                var purchaseOrders = await _mediator.Send(new GetPurchaseOrdersBySupplier.Query(id));

                var result = new SupplierWithOrdersDto
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    ContactName = supplier.ContactName,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address,
                    Website = supplier.Website,
                    Notes = supplier.Notes,
                    IsActive = supplier.IsActive,
                    CreatedAt = supplier.CreatedAt,
                    UpdatedAt = supplier.UpdatedAt,
                    TotalOrders = purchaseOrders.Count(),
                    TotalOrderAmount = purchaseOrders.Sum(o => o.TotalAmount),
                    OrdersByStatus = purchaseOrders
                        .GroupBy(o => o.Status)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    RecentOrders = purchaseOrders
                        .OrderByDescending(o => o.OrderDate)
                        .Take(5)
                        .ToList()
                };

                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }

    public record SupplierWithOrdersDto : SupplierDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public IEnumerable<PurchaseOrderDto> RecentOrders { get; set; } = new List<PurchaseOrderDto>();
    }
}
