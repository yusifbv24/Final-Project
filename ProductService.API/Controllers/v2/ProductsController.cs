using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products.Commands;
using ProductService.Application.Features.Products.Queries;
using ProductService.Domain.Exceptions;

namespace ProductService.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _mediator.Send(new GetAllProducts.Query());
            return Ok(products);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            try
            {
                var product = await _mediator.Send(new GetProductById.Query(id));
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("by-category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(int categoryId)
        {
            try
            {
                var products = await _mediator.Send(new GetProductsByCategoryId.Query(categoryId));
                return Ok(products);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto productDto)
        {
            var product = await _mediator.Send(new CreateProduct.Command(productDto));
            return CreatedAtAction(nameof(GetById), new { id = product.Id, version = "2.0" }, product);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto productDto)
        {
            try
            {
                var product = await _mediator.Send(new UpdateProduct.Command(id, productDto));
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mediator.Send(new DeleteProduct.Command(id));
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // V2 specific endpoints
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Search(
            [FromQuery] string? keyword, 
            [FromQuery] decimal? minPrice, 
            [FromQuery] decimal? maxPrice)
        {
            // In a real implementation, you would add a new query handler for this
            var products = await _mediator.Send(new GetAllProducts.Query());

            // Filter products based on search criteria
            var filteredProducts = products.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                filteredProducts = filteredProducts.Where(p =>
                    p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (minPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value);
            }

            return Ok(filteredProducts.ToList());
        }
    }
}
