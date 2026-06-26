using Core.Common;
using Core.Entities;

namespace Application.UserServices.Dtos
{
    public class UserFilter : Filter
    {
        public List<Guid> Ids { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// Represents a view model for a user.
    /// </summary>
    public class UserModel
    {
        public Guid? Id { get; set; }

        public string UserName { get; set; }

        public string? Email { get; set; }

        public string? Avatar { get; set; }

        public DateTime? Birthday { get; set; }

        public int? Gender { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool IsRootAdmin { get; set; }

        public bool IsEmployee { get; set; }

        public string? Description { get; set; }

        public UserModel()
        {
            
        }

        public UserModel(AppUser entity)
        {
            Id = entity.Id;
            UserName = entity.UserName;
            Email = entity.Email;
            Avatar = entity.Avatar;
            Birthday = entity.Birthday;
            Gender = entity.Gender;
            Name = entity.Name;
            Password = string.Empty;
            Address = entity.Address;
            Phone = entity.Phone;
            LastLogin = entity.LastLogin;
            IsRootAdmin = entity.IsRootAdmin;
            IsEmployee = entity.IsEmployee;
            Description = entity.Description;
        }
    }


}



