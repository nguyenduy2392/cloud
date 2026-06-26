using Application.UserServices.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Application.Auth.Dtos
{
    public class LoginRequest
    {
        /// <summary>
        /// Định danh đơn vị
        /// </summary>
        [Required]
        public string Identity { get; set; }

        /// <summary>
        /// Tên người dùng
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Mật khẩu
        /// </summary>
        [Required]
        public string Password { get; set; }
    }

    public class SsoCallbackRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;
        public string? Identity { get; set; }
    }
}
