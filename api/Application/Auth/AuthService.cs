using Application.Auth.Dtos;
using Application.Helper;
using Application.Options;
using Application.UserServices.Dtos;
using Core;
using Core.Common;
using Core.Constants;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Application.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        Task<Response> AuthAsync(LoginRequest model);

        /// <summary>
        /// Đổi code SSO lấy mini-crm JWT
        /// </summary>
        Task<Response> SsoCallbackAsync(SsoCallbackRequest model);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ICryptorFactory _cryptorFactory;
        private readonly IJwtFactory _jwtFactory;
        private readonly IAppContextAccessor _accessor;
        private readonly ILogger<AuthService> _logger;
        private readonly AppSetting _setting;
        private readonly SsoSettings _ssoSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(
            AppDbContext context,
            ICryptorFactory cryptorFactory,
            IJwtFactory jwtFactory,
            IOptions<AppSetting> setting,
            IOptions<SsoSettings> ssoSettings,
            IAppContextAccessor accessor,
            ILogger<AuthService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _cryptorFactory = cryptorFactory;
            _jwtFactory = jwtFactory;
            _accessor = accessor;
            _logger = logger;
            _setting = setting.Value;
            _ssoSettings = ssoSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        /// <param name="model">Thông tin đăng nhập</param>
        /// <returns></returns>
        public async Task<Response> AuthAsync(LoginRequest model)
        {
            _logger.LogInformation($"AuthAsync: {JsonConvert.SerializeObject(model)}");
            model.Identity = model.Identity.ToUpper();
            if (!await IsExistsDatabaseAsync(model.Identity))
                return Response.Fail("Định danh đơn vị không tồn tại", ConstantErrorCode.DatabaseNotFound);

            var user = await FindUserAsync(model);

            /// Người dùng không tồn tại
            if (user is null) return Response.Fail("Tài khoản không tồn tại.", ConstantErrorCode.UserNotFound);

            /// Mật khẩu không chính xác
            if (!VerifyPassword(model.Password, user.Password)) return Response.Fail("Mật khẩu không chính xác");

            /// Cập nhật thời gian đăng nhập cuối
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            user.Password = string.Empty;


            _logger.LogInformation($"Người dùng {user.UserName} đã đăng nhập.");


            return Response.Success(new
            {
                Token = _jwtFactory.GenerateJwt(user, model.Identity),
                User = user,
                Database = model.Identity
            });
        }

        public async Task<Response> SsoCallbackAsync(SsoCallbackRequest model)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var tokenPayload = new
            {
                code = model.Code,
                clientId = _ssoSettings.ClientId,
                clientSecret = _ssoSettings.ClientSecret,
                redirectUri = _ssoSettings.RedirectUri
            };

            HttpResponseMessage ssoResponse;
            try
            {
                var jsonBody = JsonConvert.SerializeObject(tokenPayload);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                ssoResponse = await httpClient.PostAsync(
                    $"{_ssoSettings.BaseUrl.TrimEnd('/')}/auth/token", content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể kết nối SSO");
                return Response.Fail("Không thể kết nối SSO.");
            }

            if (!ssoResponse.IsSuccessStatusCode)
            {
                var errBody = await ssoResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("SSO token exchange failed: {Body}", errBody);
                return Response.Fail("Xác thực SSO thất bại.");
            }

            var ssoBody = await ssoResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("SSO token response body: {Body}", ssoBody);
            var ssoJson = JObject.Parse(ssoBody);
            var accessToken = ssoJson["accessToken"]?.ToString();
            if (string.IsNullOrEmpty(accessToken))
                return Response.Fail("SSO không trả về token.");

            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = jwtHandler.ReadJwtToken(accessToken);
            }
            catch
            {
                return Response.Fail("Token SSO không hợp lệ.");
            }

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (!Guid.TryParse(subClaim, out var ssoUserId))
                return Response.Fail("Token SSO không chứa thông tin người dùng.");

            if (string.IsNullOrWhiteSpace(model.Identity))
            {
                var tenantName = ssoJson["tenantName"]?.ToString()
                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "tenantName")?.Value
                    ?? string.Empty;
                model.Identity = tenantName.ToUpper().Replace(" ", "");
            }

            if (string.IsNullOrWhiteSpace(model.Identity))
                return Response.Fail("Không xác định được định danh đơn vị từ SSO.");

            model.Identity = model.Identity.ToUpper();

            if (!await IsExistsDatabaseAsync(model.Identity))
                return Response.Fail("Định danh đơn vị không tồn tại", ConstantErrorCode.DatabaseNotFound);

            var connString = _accessor.GetConnectionString(model.Identity);
            _context.Database.SetConnectionString(connString);

            var appUser = await _context.Users
                .FirstOrDefaultAsync(u => u.SsoId == ssoUserId && !u.IsDeleted);

            if (appUser is null)
                return Response.Fail("Tài khoản chưa được liên kết với SSO. Vui lòng liên hệ quản trị viên.");

            // Ghi đè profile từ SSO
            var ssoName = ssoJson["name"]?.ToString();
            var ssoEmail = ssoJson["email"]?.ToString();
            var ssoPhone = ssoJson["phone"]?.ToString();
            var ssoAvatar = ssoJson["avatar"]?.ToString();
            var ssoDesc = ssoJson["description"]?.ToString();
            _logger.LogInformation(
                "SSO profile: name=[{Name}], email=[{Email}], phone=[{Phone}], avatar=[{Avatar}], desc=[{Desc}]",
                ssoName, ssoEmail, ssoPhone, ssoAvatar, ssoDesc);
            _logger.LogInformation(
                "AppUser before: name=[{Name}], email=[{Email}], phone=[{Phone}]",
                appUser.Name, appUser.Email, appUser.Phone);

            if (!string.IsNullOrWhiteSpace(ssoName)) appUser.Name = ssoName;
            if (!string.IsNullOrWhiteSpace(ssoEmail)) appUser.Email = ssoEmail;
            if (!string.IsNullOrWhiteSpace(ssoPhone)) appUser.Phone = ssoPhone;
            if (!string.IsNullOrWhiteSpace(ssoAvatar)) appUser.Avatar = ssoAvatar;
            if (!string.IsNullOrWhiteSpace(ssoDesc)) appUser.Description = ssoDesc;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "AppUser after: name=[{Name}], email=[{Email}], phone=[{Phone}]",
                appUser.Name, appUser.Email, appUser.Phone);

            appUser.Password = string.Empty;

            _logger.LogInformation("SSO callback: user {UserName} đăng nhập qua SSO", appUser.UserName);

            return Response.Success(new
            {
                Token = _jwtFactory.GenerateJwt(appUser, model.Identity),
                User = appUser,
                Database = model.Identity
            });
        }

        /// <summary>
        /// Kiểm tra có thể kết nối cơ sở dữ liệu không?
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        private async Task<bool> IsExistsDatabaseAsync(string databaseName)
        {
            var connectionString = _accessor.GetConnectionString(databaseName);
            _context.Database.SetConnectionString(connectionString);

            return await _context.Database.CanConnectAsync();
        }

        /// <summary>
        /// Tìm kiếm người dùng theo tên đăng nhập và mật khẩu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<AppUser> FindUserAsync(LoginRequest model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName.Trim().Equals(model.UserName.Trim()));
            if (user is null) return default;

            return user;
        }

        /// <summary>
        /// So sánh mật khẩu
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashPassword"></param>
        /// <returns></returns>
        private bool VerifyPassword(string password, string hashPassword)
        {
            string hased = _cryptorFactory.ToHashPassword(password);

            return hased.Equals(hashPassword);
        }
    }
}
