using Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Application.UserServices.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Entities;

namespace Application.Auth
{
    public interface IJwtFactory
    {
        /// <summary>
        /// Tạo mã xác thực
        /// </summary>
        /// <param name="user"></param>
        /// <param name="identity">Định danh đơn vị</param>
        /// <returns></returns>
        string GenerateJwt(AppUser user, string identity);

        /// <summary>
        /// Xác thực token
        /// </summary>
        /// <param name="token">Mã truy cập</param>
        /// <returns></returns>
        bool IsValidToken(string? token = "");
    }

    public class JwtFactory : IJwtFactory
    {
        private readonly AppSetting _setting;

        public JwtFactory(IOptions<AppSetting> setting)
        {
            _setting = setting.Value;
        }

        /// <summary>
        /// Tạo mã xác thực
        /// </summary>
        /// <param name="user"></param>
        /// <param name="identity">Định danh đơn vị</param>
        /// <returns></returns>
        public string GenerateJwt(AppUser user, string identity)
        {
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim(ClaimTypes.Name, user.UserName),
                 new Claim(ClaimTypes.GivenName, user.Name),
                 new Claim("IsRootAdmin",user.IsRootAdmin.ToString().ToLower()),
                 new Claim("Database", identity)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_setting.Token));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool IsValidToken(string? token = "")
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return false;

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_setting.Token);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Nếu bạn có issuer, đổi thành true và xác định giá trị
                    ValidateAudience = false, // Nếu bạn có audience, đổi thành true và xác định giá trị
                    ValidateLifetime = true, // Kiểm tra xem token có hết hạn không
                    ClockSkew = TimeSpan.Zero // Không có khoảng lệch thời gian
                };

                // Xác thực token và trả về ClaimsPrincipal nếu token hợp lệ
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Kiểm tra xem token có đúng là JWT token không
                if (validatedToken is JwtSecurityToken jwtToken) return true;
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
