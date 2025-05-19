using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupplierService.Application.DTOs;
using SupplierService.Application.Features.PurchaseOrders.Commands;
using SupplierService.Application.Features.PurchaseOrders.Queries;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Exceptions;

namespace SupplierService.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
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


        [HttpGet("by-order-number/{orderNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseOrderDto>> GetByOrderNumber(string orderNumber)
        {
            try
            {
                var purchaseOrder = await _mediator.Send(new GetPurchaseOrderByOrderNumber.Query(orderNumber));
                return Ok(purchaseOrder);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpGet("by-supplier/{supplierId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetBySupplier(int supplierId)
        {
            try
            {
                var purchaseOrders = await _mediator.Send(new GetPurchaseOrdersBySupplier.Query(supplierId));
                return Ok(purchaseOrders);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpGet("by-status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetByStatus(PurchaseOrderStatus status)
        {
            var purchaseOrders = await _mediator.Send(new GetPurchaseOrdersByStatus.Query(status));
            return Ok(purchaseOrders);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseOrderDto>> Create([FromBody] CreatePurchaseOrderDto purchaseOrderDto)
        {
            try
            {
                var purchaseOrder = await _mediator.Send(new CreatePurchaseOrder.Command(purchaseOrderDto));
                return CreatedAtAction(nameof(GetById), new { id = purchaseOrder.Id, version = "1.0" }, purchaseOrder);
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


        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseOrderDto>> UpdateStatus(int id, [FromBody] UpdatePurchaseOrderStatusDto statusDto)
        {
            try
            {
                var purchaseOrder = await _mediator.Send(new UpdatePurchaseOrderStatus.Command(id, statusDto));
                return Ok(purchaseOrder);
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


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseOrderDto>> Update(int id, [FromBody] UpdatePurchaseOrderDto purchaseOrderDto)
        {
            try
            {
                var purchaseOrder = await _mediator.Send(new UpdatePurchaseOrder.Command(id, purchaseOrderDto));
                return Ok(purchaseOrder);
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


        [HttpPost("{id}/items/{itemId}/receive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseOrderDto>> ReceiveItem(int id, int itemId, [FromBody] ReceivePurchaseOrderItemDto receiveDto)
        {
            try
            {
                var purchaseOrder = await _mediator.Send(new ReceivePurchaseOrderItem.Command(id, itemId, receiveDto.ReceivedQuantity));
                return Ok(purchaseOrder);
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


        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseOrderDto>> Cancel(int id)
        {
            try
            {
                var purchaseOrder = await _mediator.Send(new CancelPurchaseOrder.Command(id));
                return Ok(purchaseOrder);
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
