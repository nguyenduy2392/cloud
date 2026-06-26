using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Auth;
using Application.Auth.Dtos;

namespace Api.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Auth([FromBody] LoginRequest model)
        {
            var response = await _service.AuthAsync(model);

            if (response is null) return BadRequest(Core.Common.Response.Fail());

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("sso-callback")]
        public async Task<IActionResult> SsoCallback([FromBody] SsoCallbackRequest model)
        {
            var response = await _service.SsoCallbackAsync(model);
            return Ok(response);
        }
    }
}
