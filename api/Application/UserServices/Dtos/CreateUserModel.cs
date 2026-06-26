using System.ComponentModel.DataAnnotations;

namespace Application.UserServices.Dtos
{
    public class CreateUserModel
    {
        [Required]
        [StringLength(256, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(512)]
        public string? Address { get; set; }

        public DateTime? Birthday { get; set; }

        [Range(0, 2)]
        public int? Gender { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public string? Avatar { get; set; }

        public bool IsRootAdmin { get; set; } = false;

        public bool IsEmployee { get; set; } = true;
    }

    public class UpdateUserModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(512)]
        public string? Address { get; set; }

        public DateTime? Birthday { get; set; }

        [Range(0, 2)]
        public int? Gender { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public string? Avatar { get; set; }

        public bool IsRootAdmin { get; set; }

        public bool IsEmployee { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
