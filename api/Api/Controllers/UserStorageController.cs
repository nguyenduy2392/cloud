using Application.CloudServices;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/user-storage")]
    public class UserStorageController : BaseController
    {
        private readonly IUserStorageService _service;

        public UserStorageController(IUserStorageService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get current user's storage info.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyStorage()
        {
            return Ok(await _service.GetMyStorageAsync());
        }

        /// <summary>
        /// Get a specific user's storage info (admin).
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserStorage([FromRoute] Guid userId)
        {
            return Ok(await _service.GetUserStorageAsync(userId));
        }

        /// <summary>
        /// Set storage quota for a user (admin).
        /// </summary>
        [HttpPut("{userId:guid}/quota")]
        public async Task<IActionResult> SetQuota([FromRoute] Guid userId, [FromBody] SetQuotaRequest request)
        {
            return Ok(await _service.SetQuotaAsync(userId, request.MaxBytes));
        }
    }

    public class SetQuotaRequest
    {
        public long MaxBytes { get; set; }
    }
}
