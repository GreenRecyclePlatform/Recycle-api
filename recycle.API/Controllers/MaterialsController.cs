using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Materials;
using recycle.Application.Interfaces;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // ✅ Admin-only by default
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialsController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        /// <summary>
        /// Get all materials - PUBLIC ACCESS for price list
        /// </summary>
        /// <param name="includeInactive">Include inactive materials (admin only)</param>
        /// <returns>List of materials</returns>
        [HttpGet]
        [AllowAnonymous] // ✅ Anyone can view materials
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                // ✅ Only admins can see inactive materials
                if (includeInactive && !User.IsInRole("Admin"))
                {
                    includeInactive = false;
                }

                var materials = await _materialService.GetAllMaterialsAsync(includeInactive);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving materials", error = ex.Message });
            }
        }

        /// <summary>
        /// Get material by ID - PUBLIC ACCESS
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <returns>Material details</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous] // ✅ Anyone can view material details
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
        /// Search materials by name or description - PUBLIC ACCESS
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="onlyActive">Only search active materials</param>
        /// <returns>List of matching materials</returns>
        [HttpGet("search")]
        [AllowAnonymous] // ✅ Anyone can search materials
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        /// Create a new material - ADMIN ONLY
        /// </summary>
        /// <param name="dto">Material data</param>
        /// <returns>Created material</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// Update an existing material - ADMIN ONLY
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <param name="dto">Material data</param>
        /// <returns>Updated material</returns>
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// Update material image only - ADMIN ONLY
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <param name="imageDto">Image file</param>
        /// <returns>Updated material</returns>
        [HttpPut("{id:guid}/image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// Delete a material - ADMIN ONLY
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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