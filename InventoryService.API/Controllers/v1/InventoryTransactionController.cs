using Asp.Versioning;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.InventoryTransaction.Commands;
using InventoryService.Application.Features.InventoryTransaction.Queries;
using InventoryService.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoryTransactionController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetAll()
        {
            var transactions = await _mediator.Send(new GetAllTransactions.Query());
            return Ok(transactions);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryTransactionDto>> GetById(int id)
        {
            try
            {
                var transaction = await _mediator.Send(new GetTransactionById.Query(id));
                return Ok(transaction);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpGet("by-inventory/{inventoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetByInventoryId(int inventoryId)
        {
            try
            {
                var transactions = await _mediator.Send(new GetTransactionsByInventoryId.Query(inventoryId));
                return Ok(transactions);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryTransactionDto>> Create([FromBody] CreateInventoryTransactionDto transactionDto)
        {
            try
            {
                var transaction = await _mediator.Send(new CreateInventoryTransaction.Command(transactionDto));
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id, version = "1.0" }, transaction);
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
