using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.RequestMaterials;
using recycle.Application.Interfaces;

namespace recycle.API.Controllers
{
    [ApiController]
    [Route("api/requests/{requestId:guid}/materials")]
    [Authorize]
    public class RequestMaterialsController : ControllerBase
    {
        private readonly IRequestMaterialService _requestMaterialService;

        public RequestMaterialsController(IRequestMaterialService requestMaterialService)
        {
            _requestMaterialService = requestMaterialService;
        }

        /// <summary>
        /// Get all materials for a specific request
        /// </summary>
        /// <param name="requestId">Pickup request ID</param>
        /// <returns>List of request materials</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestMaterialDto>>> GetAll(Guid requestId)
        {
            try
            {
                var materials = await _requestMaterialService.GetRequestMaterialsAsync(requestId);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving request materials", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific request material by ID
        /// </summary>
        /// <param name="requestId">Pickup request ID</param>
        /// <param name="id">Request material ID</param>
        /// <returns>Request material details</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RequestMaterialDto>> GetById(Guid requestId, Guid id)
        {
            try
            {
                var material = await _requestMaterialService.GetRequestMaterialByIdAsync(id);

                if (material == null)
                {
                    return NotFound(new { message = $"Request material with ID {id} not found" });
                }

                // Validate it belongs to the specified request
                if (material.RequestId != requestId)
                {
                    return BadRequest(new { message = "Request material does not belong to the specified request" });
                }

                return Ok(material);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the request material", error = ex.Message });
            }
        }

        /// <summary>
        /// Get total amount for a request
        /// </summary>
        /// <param name="requestId">Pickup request ID</param>
        /// <returns>Total amount</returns>
        [HttpGet("total")]
        public async Task<ActionResult<object>> GetTotal(Guid requestId)
        {
            try
            {
                var total = await _requestMaterialService.GetRequestTotalAmountAsync(requestId);
                return Ok(new { requestId, totalAmount = total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while calculating total amount", error = ex.Message });
            }
        }

        /// <summary>
        /// Add a single material to a request (User only - request owner)
        /// </summary>
        /// <param name="requestId">Pickup request ID</param>
        /// <param name="dto">Material to add</param>
        /// <returns>Created request material</returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<RequestMaterialDto>> Create(
            Guid requestId,
            [FromBody] RequestMaterialDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var material = await _requestMaterialService.AddMaterialToRequestAsync(requestId, dto);
                return CreatedAtAction(
                    nameof(GetById),
                    new { requestId, id = material.Id },
                    material);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding material to request", error = ex.Message });
            }
        }

        /// <summary>
        /// Add multiple materials to a request (User only - request owner)
        /// </summary>
        /// <param name="requestId">Pickup request ID</param>
        /// <param name="dtos">Materials to add</param>
        /// <returns>Created request materials</returns>
        [HttpPost("bulk")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<RequestMaterialDto>>> CreateBulk(
            Guid requestId,
            [FromBody] IEnumerable<RequestMaterialDto> dtos)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var materials = await _requestMaterialService.AddMaterialsToRequestAsync(requestId, dtos);
                return Ok(materials);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding materials to request", error = ex.Message });
            }
        }

        /// <summary>
        /// Update actual weight after pickup (Driver only)
        /// </summary>
        /// <param name="requestId">Pickup request ID</param>
        /// <param name="id">Request material ID</param>
        /// <param name="dto">Actual weight data</param>
        /// <returns>Updated request material</returns>
        [HttpPatch("{id:guid}/actual-weight")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<RequestMaterialDto>> UpdateActualWeight(
            Guid requestId,
            Guid id,
            [FromBody] UpdateActualWeightDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify the material belongs to the request
                var existing = await _requestMaterialService.GetRequestMaterialByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = $"Request material with ID {id} not found" });
                }

                if (existing.RequestId != requestId)
                {
                    return BadRequest(new { message = "Request material does not belong to the specified request" });
                }

                var material = await _requestMaterialService.UpdateActualWeightAsync(id, dto);
                return Ok(material);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating actual weight", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a material from a request (User only - request owner, before pickup)
        /// </summary>
        /// <param name="requestId">Pickup request ID</param>
        /// <param name="id">Request material ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> Delete(Guid requestId, Guid id)
        {
            try
            {
                // Verify the material belongs to the request
                var existing = await _requestMaterialService.GetRequestMaterialByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = $"Request material with ID {id} not found" });
                }

                if (existing.RequestId != requestId)
                {
                    return BadRequest(new { message = "Request material does not belong to the specified request" });
                }

                var result = await _requestMaterialService.RemoveMaterialFromRequestAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Request material with ID {id} not found" });
                }

                return Ok(new { message = "Material removed from request successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing material from request", error = ex.Message });
            }
        }
    }
}
