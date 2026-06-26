using Application.CloudServices;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/files")]
    public class FilesController : BaseController
    {
        private readonly IFileService _service;

        public FilesController(IFileService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get file metadata by Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>
        /// Download a file by Id.
        /// </summary>
        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download([FromRoute] Guid id)
        {
            var (stream, fileName, contentType) = await _service.DownloadAsync(id);
            if (stream == null)
                return NotFound(Core.Common.Response.Fail("File not found or missing on disk."));

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// Preview/stream a file inline (images, videos, etc.).
        /// Token can be passed via query string ?t= for use in img/video src attributes.
        /// </summary>
        [HttpGet("{id:guid}/preview")]
        public async Task<IActionResult> Preview([FromRoute] Guid id)
        {
            var (stream, fileName, contentType) = await _service.DownloadAsync(id);
            if (stream == null)
                return NotFound(Core.Common.Response.Fail("File not found or missing on disk."));

            return File(stream, contentType, enableRangeProcessing: true);
        }

        /// <summary>
        /// Upload a file (max 10 GB).
        /// </summary>
        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] Guid? folderId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(Core.Common.Response.Fail("Please select a file to upload."));

            return Ok(await _service.UploadAsync(file, folderId));
        }

        /// <summary>
        /// Rename a file.
        /// </summary>
        [HttpPut("{id:guid}/rename")]
        public async Task<IActionResult> Rename([FromRoute] Guid id, [FromBody] FileRenameRequest request)
        {
            return Ok(await _service.RenameAsync(id, request.Name));
        }

        /// <summary>
        /// Move a file to a different folder.
        /// </summary>
        [HttpPut("{id:guid}/move")]
        public async Task<IActionResult> Move([FromRoute] Guid id, [FromBody] FileMoveRequest request)
        {
            return Ok(await _service.MoveAsync(id, request.FolderId));
        }

        /// <summary>
        /// Delete a file (soft delete).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            return Ok(await _service.DeleteAsync(id));
        }

        /// <summary>
        /// Bulk delete files (soft delete).
        /// </summary>
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            return Ok(await _service.BulkDeleteAsync(ids));
        }
    }

    public class FileRenameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class FileMoveRequest
    {
        public Guid? FolderId { get; set; }
    }
}
