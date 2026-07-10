using Application.CloudServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/system")]
    public class DatabaseController : BaseController
    {
        private readonly IDatabaseService _service;

        public DatabaseController(IDatabaseService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("migration/run")]
        public async Task<IActionResult> RunMigration([FromQuery] string? identity)
        {
            return Ok(await _service.RunMigrationAsync(identity));
        }

        [AllowAnonymous]
        [HttpPost("database/initialize")]
        public async Task<IActionResult> InitializeDatabase([FromBody] InitializeDatabaseRequest request)
        {
            return Ok(await _service.InitializeDatabaseAsync(request.DatabaseName, request.UserName, request.Password));
        }

        [AllowAnonymous]
        [HttpPost("backfill-keywords")]
        public async Task<IActionResult> BackfillKeywords([FromQuery] string? identity)
        {
            return Ok(await _service.BackfillKeywordsAsync(identity));
        }

        [AllowAnonymous]
        [HttpPost("database/rename")]
        public async Task<IActionResult> RenameDatabase([FromQuery] string oldName, [FromQuery] string newName)
        {
            return Ok(await _service.RenameDatabaseAsync(oldName, newName));
        }

        [AllowAnonymous]
        [HttpPost("sync-user")]
        public async Task<IActionResult> SyncUser([FromBody] SyncUserDto request)
        {
            var result = await _service.SyncUserAsync(request);
            return result.IsSuccess ? Ok(result) : StatusCode(500, result);
        }
    }

    public class InitializeDatabaseRequest
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
