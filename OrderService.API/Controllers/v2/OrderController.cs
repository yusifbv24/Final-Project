using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OrderService.Application.DTOs;
using OrderService.Application.Features.Orders.Commands;
using OrderService.Application.Features.Orders.Queries;
using OrderService.Application.Hubs;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<OrderHub> _orderHubContext;

        public OrderController(IMediator mediator, IHubContext<OrderHub> orderHubContext)
        {
            _mediator = mediator;
            _orderHubContext = orderHubContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            var orders = await _mediator.Send(new GetAllOrders.Query());
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            try
            {
                var order = await _mediator.Send(new GetOrderById.Query(id));
                return Ok(order);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("by-status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStatus(OrderStatus status)
        {
            var orders = await _mediator.Send(new GetOrdersByStatus.Query(status));
            return Ok(orders);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto orderDto)
        {
            try
            {
                var order = await _mediator.Send(new CreateOrder.Command(orderDto));

                // Notify clients via SignalR
                await _orderHubContext.Clients.All.SendAsync("OrderCreated", order.Id, order.CustomerName);

                return CreatedAtAction(nameof(GetById), new { id = order.Id, version = "2.0" }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                var order = await _mediator.Send(new UpdateOrderStatus.Command(id, statusDto));

                // Notify clients via SignalR
                await _orderHubContext.Clients.All.SendAsync("OrderStatusChanged", order.Id, order.Status.ToString());

                return Ok(order);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // V2 Enhanced endpoint for order search
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> Search(
            [FromQuery] string? customerName,
            [FromQuery] string? customerEmail,
            [FromQuery] OrderStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            // In a real implementation, you would add a new query handler for this
            // For now, we'll fetch all and filter in memory
            var orders = await _mediator.Send(new GetAllOrders.Query());
            var filteredOrders = orders.AsQueryable();

            if (!string.IsNullOrEmpty(customerName))
                filteredOrders = filteredOrders.Where(o => o.CustomerName.Contains(customerName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(customerEmail))
                filteredOrders = filteredOrders.Where(o => o.CustomerEmail.Contains(customerEmail, StringComparison.OrdinalIgnoreCase));

            if (status.HasValue)
                filteredOrders = filteredOrders.Where(o => o.Status == status.Value);

            if (fromDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate <= toDate.Value);

            return Ok(filteredOrders.ToList());
        }

        // V2 Enhanced endpoint with order summary statistics
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderSummaryDto>> GetSummary()
        {
            // In a real implementation, you would add a new query handler for this
            var orders = await _mediator.Send(new GetAllOrders.Query());

            var summary = new OrderSummaryDto
            {
                TotalOrders = orders.Count(),
                TotalSales = orders.Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                OrdersByStatus = Enum.GetValues<OrderStatus>()
                    .ToDictionary(
                        status => status.ToString(),
                        status => orders.Count(o => o.Status == status)
                    )
            };

            return Ok(summary);
        }
        public class OrderSummaryDto
        {
            public int TotalOrders { get; set; }
            public decimal TotalSales { get; set; }
            public decimal AverageOrderValue { get; set; }
            public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        }
    }
}
