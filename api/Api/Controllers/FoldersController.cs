using Application.CloudServices;
using Application.CloudServices.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/folders")]
    public class FoldersController : BaseController
    {
        private readonly IFolderService _service;

        public FoldersController(IFolderService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get folder contents (subfolders + files) at a given parent level.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetContents(
            [FromQuery] Guid? parentId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? keyword = null)
        {
            return Ok(await _service.GetContentsAsync(parentId, page, pageSize, keyword));
        }

        /// <summary>
        /// Get full folder tree for current user.
        /// </summary>
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            return Ok(await _service.GetTreeAsync());
        }

        /// <summary>
        /// Get files and folders shared with the current user.
        /// </summary>
        [HttpGet("shared-with-me")]
        public async Task<IActionResult> GetSharedWithMe([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            return Ok(await _service.GetSharedWithMeAsync(page, pageSize));
        }

        /// <summary>
        /// Get folder detail by Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>
        /// Create a new folder.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFolderDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _service.CreateAsync(dto));
        }

        /// <summary>
        /// Rename a folder.
        /// </summary>
        [HttpPut("{id:guid}/rename")]
        public async Task<IActionResult> Rename([FromRoute] Guid id, [FromBody] RenameRequest request)
        {
            return Ok(await _service.RenameAsync(id, request.Name));
        }

        /// <summary>
        /// Move a folder to a new parent.
        /// </summary>
        [HttpPut("{id:guid}/move")]
        public async Task<IActionResult> Move([FromRoute] Guid id, [FromBody] MoveRequest request)
        {
            return Ok(await _service.MoveAsync(id, request.ParentId));
        }

        /// <summary>
        /// Delete a folder (soft delete, including all children).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            return Ok(await _service.DeleteAsync(id));
        }
    }

    public class RenameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class MoveRequest
    {
        public Guid? ParentId { get; set; }
    }
}
