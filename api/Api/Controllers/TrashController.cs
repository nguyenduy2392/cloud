using Application.CloudServices;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/trash")]
    public class TrashController : BaseController
    {
        private readonly ITrashService _service;

        public TrashController(ITrashService service)
        {
            _service = service;
        }

        /// <summary>Get all items in the current user's trash.</summary>
        [HttpGet]
        public async Task<IActionResult> GetTrash()
        {
            return Ok(await _service.GetTrashAsync());
        }

        /// <summary>Restore an item from trash by Id (file or folder).</summary>
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore([FromRoute] Guid id)
        {
            return Ok(await _service.RestoreAsync(id));
        }

        /// <summary>Permanently delete a single item from trash.</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> PermanentDelete([FromRoute] Guid id)
        {
            return Ok(await _service.PermanentDeleteAsync(id));
        }

        /// <summary>Empty the entire trash for the current user.</summary>
        [HttpDelete]
        public async Task<IActionResult> EmptyTrash()
        {
            return Ok(await _service.EmptyTrashAsync());
        }
    }
}
