using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Materials;
using recycle.Application.Interfaces;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]  // ← Uncomment this when you want to require authentication for all endpoints
    [AllowAnonymous]  // ← Remove this when you uncomment [Authorize] above
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialsController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        /// <summary>
        /// Get all materials
        /// </summary>
        /// <param name="includeInactive">Include inactive materials</param>
        /// <returns>List of materials</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[AllowAnonymous]  // ← Uncomment if you want anyone to access this even when controller requires auth
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                // Uncomment below when authorization is enabled
                // Only admins can see inactive materials
                //if (includeInactive && !User.IsInRole("Admin"))
                //{
                //    includeInactive = false;
                //}

                var materials = await _materialService.GetAllMaterialsAsync(includeInactive);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving materials", error = ex.Message });
            }
        }

        /// <summary>
        /// Get material by ID
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <returns>Material details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MaterialDto>> GetById(Guid id)
        {
            try
            {
                var material = await _materialService.GetMaterialByIdAsync(id);

                if (material == null)
                {
                    return NotFound(new { message = $"Material with ID {id} not found" });
                }

                return Ok(material);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the material", error = ex.Message });
            }
        }

        /// <summary>
        /// Search materials by name or description
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="onlyActive">Only search active materials</param>
        /// <returns>List of matching materials</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[AllowAnonymous]  // ← Uncomment if you want anyone to access this even when controller requires auth
        public async Task<ActionResult<IEnumerable<MaterialDto>>> Search(
            [FromQuery] string searchTerm,
            [FromQuery] bool onlyActive = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new { message = "Search term is required" });
                }

                var materials = await _materialService.SearchMaterialsAsync(searchTerm, onlyActive);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching materials", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new material (Admin only when auth is enabled)
        /// </summary>
        /// <param name="dto">Material data</param>
        /// <returns>Created material</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Authorize(Roles = "Admin")]  // ← Uncomment to restrict this to Admin role only
        public async Task<ActionResult<MaterialDto>> Create([FromForm] CreateMaterialDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var material = await _materialService.CreateMaterialAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the material", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing material (Admin only when auth is enabled)
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <param name="dto">Material data</param>
        /// <returns>Updated material</returns>
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[Authorize(Roles = "Admin")]  // ← Uncomment to restrict this to Admin role only
        public async Task<ActionResult<MaterialDto>> Update(Guid id, [FromForm] UpdateMaterialDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var material = await _materialService.UpdateMaterialAsync(id, dto);
                return Ok(material);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the material", error = ex.Message });
            }
        }

        /// <summary>
        /// Update material image only
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <param name="imageDto">Image file</param>
        /// <returns>Updated material</returns>
        [HttpPut("{id:guid}/image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[Authorize(Roles = "Admin")]  // ← Uncomment to restrict this to Admin role only
        public async Task<ActionResult> UpdateMaterialImage(Guid id, [FromForm] UpdateMaterialImageDto imageDto)
        {
            try
            {
                var material = await _materialService.UpdateMaterialImageAsync(id, imageDto);
                return Ok(material);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the material image", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a material (Admin only when auth is enabled)
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[Authorize(Roles = "Admin")]  // ← Uncomment to restrict this to Admin role only
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _materialService.DeleteMaterialAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Material with ID {id} not found" });
                }

                return Ok(new { message = "Material deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the material", error = ex.Message });
            }
        }
    }
}