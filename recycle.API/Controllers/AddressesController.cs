using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs;
using recycle.Application.Services;
using recycle.Domain.Entities;
using System.Net;
using System.Security.Claims;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly AddressService _addressService;
        public AddressesController(AddressService addressService)
        {
            _addressService = addressService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAddresses()
        {

            var Addresses = await _addressService.GetAddresses(GetUserId());
            
            return Ok(Addresses);
        }

       
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Address>> GetAddress(Guid id)
        {
            
            var address = await _addressService.GetAddress(id, GetUserId());
            if (address == null)
            {
                return NotFound("Address Not Found");
            }

            return Ok(address);
        }

     
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> CreateAddress([FromBody] AddressDto addressDto,
            IValidator<AddressDto> validator)
        {
            var validationResult = await validator.ValidateAsync(addressDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not authorized");
            }

            var entity = await _addressService.CreateAddress(addressDto, GetUserId());

           
            return CreatedAtAction(nameof(GetAddress), new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AddressDto>> UpdateAddress(Guid id,
            [FromBody] AddressDto addressdto,
            IValidator<AddressDto> validator)
        {
            var validationResult = await validator.ValidateAsync(addressdto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var address = await _addressService.UpdateAddress(id, addressdto);

            return Ok(address);
        }

       
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteAddress(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("User not authorized");
            }

            var address = await _addressService.GetAddress(id, GetUserId());
            if (address == null)
            {
                return NotFound("Address Not Found");
            }

            var result = await _addressService.DeleteAddress(address);
            if (!result)
            {
                return BadRequest("Error deleting address");
            }
            return NoContent();
        }
    }
}
