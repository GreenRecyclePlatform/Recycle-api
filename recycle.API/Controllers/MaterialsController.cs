using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Materials;
using recycle.Application.Interfaces;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        /// <param name="includeInactive">Include inactive materials (Admin only)</param>
        /// <returns>List of materials</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                // Only admins can see inactive materials
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
        /// Get material by ID
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <returns>Material details</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        /// Create a new material (Admin only)
        /// </summary>
        /// <param name="dto">Material data</param>
        /// <returns>Created material</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MaterialDto>> Create([FromBody] CreateMaterialDto dto)
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
        /// Update an existing material (Admin only)
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <param name="dto">Material data</param>
        /// <returns>Updated material</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MaterialDto>> Update(Guid id, [FromBody] UpdateMaterialDto dto)
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
        /// Delete a material (Admin only)
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
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
