using Application.CloudServices;
using Application.CloudServices.Dtos;
using Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/resource-permissions")]
    public class ResourcePermissionsController : BaseController
    {
        private readonly IResourcePermissionService _service;

        public ResourcePermissionsController(IResourcePermissionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all permissions for a resource.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPermissions(
            [FromQuery] EnumResourceType resourceType,
            [FromQuery] Guid resourceId)
        {
            return Ok(await _service.GetPermissionsAsync(resourceType, resourceId));
        }

        /// <summary>
        /// Set (create or update) a permission for a resource.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetPermission([FromBody] SetResourcePermissionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _service.SetPermissionAsync(dto));
        }

        /// <summary>
        /// Remove a permission (hard delete).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemovePermission([FromRoute] Guid id)
        {
            return Ok(await _service.RemovePermissionAsync(id));
        }

        /// <summary>
        /// Get all resources shared with the current user.
        /// </summary>
        [HttpGet("shared-with-me")]
        public async Task<IActionResult> SharedWithMe(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 40,
            [FromQuery] string? keyword = null)
        {
            return Ok(await _service.GetSharedWithMeAsync(page, pageSize, keyword));
        }
    }
}
