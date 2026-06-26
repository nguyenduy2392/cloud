using Application.UserServices.Dtos;
using Core.Common;

namespace Application.UserServices
{
    public interface IUserService
    {
        /// <summary>
        /// Lấy danh sách người dùng (có phân trang + tìm kiếm)
        /// </summary>
        Task<Response> GetAllAsync(int page = 1, int pageSize = 20, string? keyword = null);

        /// <summary>
        /// Lấy toàn bộ người dùng (không phân trang, cho dropdown/chọn người phụ trách).
        /// </summary>
        Task<Response> GetAllUsersAsync();

        /// <summary>
        /// Lấy chi tiết người dùng theo Id
        /// </summary>
        Task<Response> GetByIdAsync(Guid id);

        /// <summary>
        /// Chi tiết người dùng theo tên đăng nhập
        /// </summary>
        Task<Response> GetByUserNameAsync(string userName);

        /// <summary>
        /// Tạo mới người dùng
        /// </summary>
        Task<Response> CreateAsync(CreateUserModel model);

        /// <summary>
        /// Cập nhật người dùng
        /// </summary>
        Task<Response> UpdateAsync(UpdateUserModel model);

        /// <summary>
        /// Xóa người dùng (xóa mềm)
        /// </summary>
        Task<Response> DeleteAsync(Guid id);

        /// <summary>
        /// Xóa nhiều người dùng (xóa mềm)
        /// </summary>
        Task<Response> DeleteManyAsync(List<Guid> ids);

        /// <summary>
        /// Đổi mật khẩu người dùng (yêu cầu mật khẩu hiện tại)
        /// </summary>
        Task<Response> ChangePasswordAsync(ChangePasswordModel model);

        /// <summary>
        /// Reset mật khẩu người dùng (admin đặt lại, không cần mật khẩu cũ)
        /// </summary>
        Task<Response> ResetPasswordAsync(ResetPasswordModel model);

        /// <summary>
        /// Cập nhật ảnh đại diện người dùng
        /// </summary>
        Task<Response> UpdateAvatarAsync(Guid userId, string? avatarFileName);
    }
}
