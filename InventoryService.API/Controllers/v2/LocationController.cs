using Asp.Versioning;
using InventoryService.Application.DTOs;
using InventoryService.Application.Features.Inventory.Queries;
using InventoryService.Application.Features.Location.Queries;
using InventoryService.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LocationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetAll()
        {
            var locations = await _mediator.Send(new GetAllLocations.Query());
            return Ok(locations);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LocationDto>> GetById(int id)
        {
            try
            {
                var location = await _mediator.Send(new GetLocationById.Query(id));
                return Ok(location);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        // V2 Enhanced endpoint with inventory summary for each location
        [HttpGet("with-inventory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LocationWithInventoryDto>>> GetLocationsWithInventory()
        {
            // In a real implementation, you would add a new query handler for this
            var locations = await _mediator.Send(new GetAllLocations.Query());
            var allInventory = await _mediator.Send(new GetAllInventory.Query());

            var result = locations.Select(location => {
                var locationInventory = allInventory
                    .Where(i => i.LocationId == location.Id)
                    .ToList();

                return new LocationWithInventoryDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Code = location.Code,
                    Description = location.Description,
                    IsActive = location.IsActive,
                    TotalItems = locationInventory.Count,
                    TotalUniqueProducts = locationInventory.Select(i => i.ProductId).Distinct().Count(),
                    TotalQuantity = locationInventory.Sum(i => i.Quantity)
                };
            }).ToList();

            return Ok(result);
        }
    }
}