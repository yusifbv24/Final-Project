using Asp.Versioning;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.Inventory.Queries;
using InventoryService.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAll()
        {
            var inventories = await _mediator.Send(new GetAllInventory.Query());
            return Ok(inventories);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryDto>> GetById(int id)
        {
            try
            {
                var inventory = await _mediator.Send(new GetInventoryById.Query(id));
                return Ok(inventory);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        // V2 Enhanced endpoint with filtering
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> Search(
            [FromQuery] int? productId,
            [FromQuery] int? locationId,
            [FromQuery] int? minQuantity,
            [FromQuery] int? maxQuantity,
            [FromQuery] bool? isActive)
        {
            // In a real implementation, you would add a new query handler for this
            // For now, we'll fetch all and filter in memory
            var inventories = await _mediator.Send(new GetAllInventory.Query());
            var filteredInventories = inventories.AsQueryable();

            if (productId.HasValue)
                filteredInventories = filteredInventories.Where(i => i.ProductId == productId.Value);

            if (locationId.HasValue)
                filteredInventories = filteredInventories.Where(i => i.LocationId == locationId.Value);

            if (minQuantity.HasValue)
                filteredInventories = filteredInventories.Where(i => i.Quantity >= minQuantity.Value);

            if (maxQuantity.HasValue)
                filteredInventories = filteredInventories.Where(i => i.Quantity <= maxQuantity.Value);

            if (isActive.HasValue)
                filteredInventories = filteredInventories.Where(i => i.IsActive == isActive.Value);

            return Ok(filteredInventories.ToList());
        }

        // V2 Enhanced endpoint for low stock summary
        [HttpGet("low-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LowStockItemDto>>> GetLowStockItems([FromQuery] int threshold = 10)
        {
            // In a real implementation, you would add a new query handler for this
            var inventories = await _mediator.Send(new GetAllInventory.Query());

            var lowStockItems = inventories
                .Where(i => i.Quantity <= threshold)
                .Select(i => new LowStockItemDto
                {
                    InventoryId = i.Id,
                    ProductId = i.ProductId,
                    LocationId = i.LocationId,
                    LocationName = i.LocationName,
                    CurrentQuantity = i.Quantity,
                    Threshold = threshold
                })
                .ToList();

            return Ok(lowStockItems);
        }
    }
}