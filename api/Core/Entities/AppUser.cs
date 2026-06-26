using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    /// <summary>
    /// Lớp biểu diễn thông tin người dùng trong hệ thống.
    /// </summary>
    [Table("AppUsers")]
    public class AppUser : EntityBase
    {
        /// <summary>
        /// Tên đăng nhập của người dùng.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email của người dùng.
        /// </summary>
        public string? Email { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn đến ảnh đại diện của người dùng.
        /// </summary>
        public string? Avatar { get; set; } = string.Empty;

        /// <summary>
        /// Ngày sinh của người dùng.
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// Giới tính của người dùng (0: Không xác định, 1: Nam, 2: Nữ).
        /// </summary>
        public int? Gender { get; set; }

        /// <summary>
        /// Tên đầy đủ của người dùng.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu của người dùng.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ của người dùng.
        /// </summary>
        public string? Address { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại của người dùng.
        /// </summary>
        public string? Phone { get; set; } = string.Empty;

        /// <summary>
        /// Thông tin chi tiết khác về người dùng.
        /// </summary>
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Lần đăng nhập cuối cùng của người dùng.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Admin chính
        /// </summary>
        public bool IsRootAdmin { get; set; } = false;

        /// <summary>
        /// Có phải nhân viên không?
        /// </summary>
        public bool IsEmployee { get; set; } = true;

        public Guid? SsoId { get; set; }
    }
}
