using Asp.Versioning;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.InventoryTransaction.Queries;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
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


        // V2 Enhanced endpoint with filtering
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> Search(
            [FromQuery] int? inventoryId,
            [FromQuery] TransactionType? type,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            // In a real implementation, you would add a new query handler for this
            // For now, we'll fetch all and filter in memory
            var transactions = await _mediator.Send(new GetAllTransactions.Query());
            var filteredTransactions = transactions.AsQueryable();

            if (inventoryId.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.InventoryId == inventoryId.Value);

            if (type.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.Type == type.Value);

            if (startDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate <= endDate.Value);

            return Ok(filteredTransactions.ToList());
        }


        // V2 Enhanced endpoint for transaction summary
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<TransactionSummaryDto>> GetTransactionSummary(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            // In a real implementation, you would add a new query handler for this
            var transactions = await _mediator.Send(new GetAllTransactions.Query());

            // Apply date filters
            var filteredTransactions = transactions.AsEnumerable();

            if (startDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate <= endDate.Value);

            var transactionsArray = filteredTransactions.ToArray();

            var summary = new TransactionSummaryDto
            {
                TotalTransactions = transactionsArray.Length,
                StockInTotal = transactionsArray.Where(t => t.Type == TransactionType.StockIn).Sum(t => t.Quantity),
                StockOutTotal = transactionsArray.Where(t => t.Type == TransactionType.StockOut).Sum(t => t.Quantity),
                AdjustmentTotal = transactionsArray.Where(t => t.Type == TransactionType.Adjustment).Sum(t => t.Quantity),
                TransferTotal = transactionsArray.Where(t => t.Type == TransactionType.Transfer).Sum(t => t.Quantity),
                TransactionsByType = Enum.GetValues<TransactionType>()
                    .ToDictionary(
                        type => type.ToString(),
                        type => transactionsArray.Count(t => t.Type == type)
                    )
            };

            return Ok(summary);
        }
    }
}