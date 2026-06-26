using Application.Options;
using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Application.Auth
{
    
    public class AppContextAccessor : IAppContextAccessor
    {
        private readonly IHttpContextAccessor _context;
        private readonly ServerSetting _setting;
        private string? _databaseOverride;

        public AppContextAccessor(IHttpContextAccessor context, IOptions<ServerSetting> setting)
        {
            _context = context;
            _setting = setting.Value;
        }

        /// <summary>
        /// Tạo chuỗi kết nối
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public string GetConnectionString(string? databaseName = "")
        {
            if (string.IsNullOrEmpty(databaseName)) databaseName = GetDatabaseName();

            return $"Server={_setting.Address}; Database=Cloud_{databaseName.ToLower()};User Id={_setting.Id};Password={_setting.Password};MultipleActiveResultSets=true;TrustServerCertificate=True";

        }

        /// <summary>
        /// Id người dùng đang đăng nhập
        /// </summary>
        /// <returns></returns>
        public Guid? GetCurrentUserId()
        {
            var claim = _context.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier));
            if (claim == null)
            {
                return Guid.Empty;
            }

            return Guid.Parse(claim.Value);
        }

        /// <summary>
        /// Lấy cơ sở dữ liệu
        /// </summary>
        /// <returns></returns>
        public string GetDatabaseName()
        {
            if (!string.IsNullOrEmpty(_databaseOverride)) return _databaseOverride;

            var claim = _context.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type.Equals("Database"));
            if (claim != null && !string.IsNullOrEmpty(claim.Value)) return claim.Value;

            // Fallback cho public endpoint (không có JWT): đọc query param ?db=
            var dbParam = _context.HttpContext?.Request?.Query["db"].FirstOrDefault();
            if (!string.IsNullOrEmpty(dbParam)) return dbParam;

            return string.Empty;
        }

        /// <summary>
        /// Ghi đè định danh đơn vị cho scope hiện tại (dùng cho background job không có HttpContext).
        /// </summary>
        public void SetDatabaseOverride(string tenant)
        {
            _databaseOverride = tenant;
        }

        /// <summary>
        /// Thư mục lưu trữ
        /// </summary>
        /// <returns></returns>
        public string GetFolderName()
        {
            var claim = _context.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type.Equals("RootFolder"));
            if (claim == null) return string.Empty;

            return claim.Value;
        }

        /// <summary>
        /// Địa chỉ IP đăng nhập
        /// </summary>
        /// <returns></returns>
        public string GetIpAddress() => _context?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

        /// <summary>
        /// Tên người dùng đang đăng nhập
        /// </summary>
        /// <returns></returns>
        public string GetUserFullName()
        {
            var claim = _context.HttpContext?.User?
                .Claims?.FirstOrDefault(c => c.Type.Equals("FullName"));
            if (claim == null)
            {
                return "Quản trị";
            }

            return claim.Value;
        }

        /// <summary>
        /// Kiểm tra người dùng có phải admin không?
        /// </summary>
        /// <returns></returns>
        public bool IsRootAdmin()
        {
            var claims = _context.HttpContext?.User?.Claims;
            if (claims == null)
                return false;

            var claim = claims.FirstOrDefault(c =>
                c.Type.Equals("IsRootAdmin", StringComparison.OrdinalIgnoreCase)
                || c.Type.Equals("IsAdmin", StringComparison.OrdinalIgnoreCase));
            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
                return false;

            return bool.TryParse(claim.Value, out var isAdmin) && isAdmin;
        }
    }
}
