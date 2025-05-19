using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupplierService.Application.DTOs;
using SupplierService.Application.Features.PurchaseOrders.Queries;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Exceptions;

namespace SupplierService.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetAll()
        {
            var purchaseOrders = await _mediator.Send(new GetAllPurchaseOrders.Query());
            return Ok(purchaseOrders);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseOrderDto>> GetById(int id)
        {
            try
            {
                var purchaseOrder = await _mediator.Send(new GetPurchaseOrderById.Query(id));
                return Ok(purchaseOrder);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // V2 Enhanced endpoint - Advanced search with dates and amounts
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> Search(
            [FromQuery] int? supplierId,
            [FromQuery] PurchaseOrderStatus? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] decimal? minAmount,
            [FromQuery] decimal? maxAmount)
        {
            // In a real implementation, you would add a dedicated query handler for this
            var purchaseOrders = await _mediator.Send(new GetAllPurchaseOrders.Query());
            var filteredOrders = purchaseOrders.AsQueryable();

            if (supplierId.HasValue)
                filteredOrders = filteredOrders.Where(o => o.SupplierId == supplierId.Value);

            if (status.HasValue)
                filteredOrders = filteredOrders.Where(o => o.Status == status.Value);

            if (startDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate <= endDate.Value);

            if (minAmount.HasValue)
                filteredOrders = filteredOrders.Where(o => o.TotalAmount >= minAmount.Value);

            if (maxAmount.HasValue)
                filteredOrders = filteredOrders.Where(o => o.TotalAmount <= maxAmount.Value);

            return Ok(filteredOrders.ToList());
        }

        // V2 Enhanced endpoint - Summary of purchase orders
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PurchaseOrderSummaryDto>> GetSummary(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            // In a real implementation, you would add a dedicated query handler for this
            var purchaseOrders = await _mediator.Send(new GetAllPurchaseOrders.Query());

            var filteredOrders = purchaseOrders.AsEnumerable();

            if (startDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate <= endDate.Value);

            var ordersArray = filteredOrders.ToArray();

            var summary = new PurchaseOrderSummaryDto
            {
                TotalOrders = ordersArray.Length,
                TotalAmount = ordersArray.Sum(o => o.TotalAmount),
                AverageOrderAmount = ordersArray.Any() ? ordersArray.Average(o => o.TotalAmount) : 0,
                OrdersBySupplier = ordersArray
                    .GroupBy(o => o.SupplierId)
                    .ToDictionary(g => g.Key, g => g.Count()),
                OrdersByStatus = Enum.GetValues<PurchaseOrderStatus>()
                    .ToDictionary(
                        status => status.ToString(),
                        status => ordersArray.Count(o => o.Status == status)
                    ),
                OrdersByMonth = ordersArray
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:D2}",
                        g => g.Count()
                    )
            };

            return Ok(summary);
        }
    }

    public record PurchaseOrderSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageOrderAmount { get; set; }
        public Dictionary<int, int> OrdersBySupplier { get; set; } = new();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public Dictionary<string, int> OrdersByMonth { get; set; } = new();
    }
}
