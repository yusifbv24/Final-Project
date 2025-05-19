using Asp.Versioning;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.Inventory.Commands;
using InventoryService.Application.Features.Inventory.Queries;
using InventoryService.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
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


        [HttpGet("by-product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetByProductId(int productId)
        {
            try
            {
                var inventories = await _mediator.Send(new GetInventoryByProductId.Query(productId));
                return Ok(inventories);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpGet("by-location/{locationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetByLocationId(int locationId)
        {
            try
            {
                var inventories = await _mediator.Send(new GetInventoryByLocationId.Query(locationId));
                return Ok(inventories);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryDto>> Create([FromBody] CreateInventoryDto inventoryDto)
        {
            try
            {
                var inventory = await _mediator.Send(new CreateInventory.Command(inventoryDto));
                return CreatedAtAction(nameof(GetById), new { id = inventory.Id, version = "1.0" }, inventory);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is NotFoundException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("{id}/quantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryDto>> UpdateQuantity(int id, [FromBody] UpdateInventoryDto inventoryDto)
        {
            try
            {
                var inventory = await _mediator.Send(new UpdateInventoryQuantity.Command(id, inventoryDto));
                return Ok(inventory);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("{id}/add-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryDto>> AddStock(
            int id,
            [FromBody] AddStockRequest request)
        {
            try
            {
                var inventory = await _mediator.Send(new AddInventoryStock.Command(
                    id,
                    request.Quantity,
                    request.Reference,
                    request.Notes));

                return Ok(inventory);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("{id}/remove-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryDto>> RemoveStock(
            int id,
            [FromBody] RemoveStockRequest request)
        {
            try
            {
                var inventory = await _mediator.Send(new RemoveInventoryStock.Command(
                    id,
                    request.Quantity,
                    request.Reference,
                    request.Notes));

                return Ok(inventory);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
