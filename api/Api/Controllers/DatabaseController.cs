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
    }

    public class InitializeDatabaseRequest
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
