using Application.Auth;
using Application.Helper;
using Application.UserServices.Dtos;
using Core;
using Core.Common;
using Core.Constants;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.UserServices
{
    public class UserService : IUserService
    {
        private readonly IRepository _repository;
        private readonly ICryptorFactory _cryptorFactory;
        private readonly ILogger<UserService> _logger;
        private readonly IAppContextAccessor _accessor;

        public UserService(
            IRepository repository,
            ICryptorFactory cryptorFactory,
            ILogger<UserService> logger,
            IAppContextAccessor accessor)
        {
            _repository = repository;
            _cryptorFactory = cryptorFactory;
            _logger = logger;
            _accessor = accessor;
        }

        public async Task<Response> GetAllAsync(int page = 1, int pageSize = 20, string? keyword = null)
        {
            try
            {
                var query = _repository.GetQueryable<AppUser>();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(x =>
                        x.UserName.ToLower().Contains(kw) ||
                        x.Name.ToLower().Contains(kw) ||
                        (x.Email != null && x.Email.ToLower().Contains(kw)) ||
                        (x.Phone != null && x.Phone.Contains(kw)));
                }

                var paged = await _repository.FindPagedAsync<AppUser, UserModel>(
                    query,
                    u => new UserModel(u),
                    page,
                    pageSize);

                return Response.Success(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng.");
                return Response.Fail("Không tải được danh sách người dùng.");
            }
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id không hợp lệ.");

            try
            {
                var user = await _repository.FindAsync<AppUser>(id);
                if (user is null) return Response.Fail("Người dùng không tồn tại.");
                return Response.Success(new UserModel(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy người dùng [{Id}].", id);
                return Response.Fail("Không tải được thông tin người dùng.");
            }
        }

        public async Task<Response> GetByUserNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return Response.Fail("Tên đăng nhập không hợp lệ.");

            try
            {
                var user = await _repository.FindAsync<AppUser>(x => x.UserName.ToLower().Equals(userName.ToLower()));
                if (user is null) return Response.Fail("Người dùng không tồn tại.");
                return Response.Success(new UserModel(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy người dùng [{UserName}].", userName);
                return Response.Fail("Không tải được thông tin người dùng.");
            }
        }

        public async Task<Response> CreateAsync(CreateUserModel model)
        {
            if (model == null) return Response.Fail("Dữ liệu không hợp lệ.");

            if (string.IsNullOrWhiteSpace(model.UserName))
                return Response.Fail("Tên đăng nhập không được để trống.");

            if (model.UserName.Trim().Length < 3)
                return Response.Fail("Tên đăng nhập phải có ít nhất 3 ký tự.");

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
                return Response.Fail("Mật khẩu phải có ít nhất 6 ký tự.");

            if (string.IsNullOrWhiteSpace(model.Name))
                return Response.Fail("Họ tên không được để trống.");

            try
            {
                var existsUserName = await _repository.IsExistsAsync<AppUser>(x => x.UserName.ToLower() == model.UserName.ToLower());
                if (existsUserName)
                    return Response.Fail($"Tên đăng nhập \"{model.UserName}\" đã tồn tại.");

                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    var existsEmail = await _repository.IsExistsAsync<AppUser>(x => x.Email != null && x.Email.ToLower() == model.Email.ToLower());
                    if (existsEmail)
                        return Response.Fail($"Email \"{model.Email}\" đã được sử dụng.");
                }

                var user = new AppUser
                {
                    UserName = model.UserName.Trim(),
                    Password = _cryptorFactory.ToHashPassword(model.Password),
                    Name = model.Name.Trim(),
                    Email = model.Email?.Trim(),
                    Phone = model.Phone?.Trim(),
                    Address = model.Address?.Trim(),
                    Birthday = model.Birthday,
                    Gender = model.Gender,
                    Description = model.Description?.Trim(),
                    Avatar = model.Avatar,
                    IsRootAdmin = model.IsRootAdmin,
                    IsEmployee = model.IsEmployee,
                };

                var result = await _repository.AddAsync(user);
                _logger.LogInformation("Tạo user [{UserName}] bởi [{By}].", user.UserName, _accessor.GetCurrentUserId());

                return Response.Success(new { result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng [{UserName}].", model.UserName);
                return Response.Fail($"Tạo người dùng thất bại: {ex.Message}");
            }
        }

        public async Task<Response> UpdateAsync(UpdateUserModel model)
        {
            if (model == null) return Response.Fail("Dữ liệu không hợp lệ.");
            if (model.Id == Guid.Empty) return Response.Fail("Id không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.Name)) return Response.Fail("Họ tên không được để trống.");

            try
            {
                var user = await _repository.FindAsync<AppUser>(model.Id);
                if (user is null) return Response.Fail("Người dùng không tồn tại.");

                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    var existsEmail = await _repository.IsExistsAsync<AppUser>(x => x.Id != model.Id && x.Email != null && x.Email.ToLower() == model.Email.ToLower());
                    if (existsEmail)
                        return Response.Fail($"Email \"{model.Email}\" đã được sử dụng.");
                }

                user.Name = model.Name.Trim();
                user.Email = model.Email?.Trim();
                user.Phone = model.Phone?.Trim();
                user.Address = model.Address?.Trim();
                user.Birthday = model.Birthday;
                user.Gender = model.Gender;
                user.Description = model.Description?.Trim();
                user.Avatar = model.Avatar;
                user.IsRootAdmin = model.IsRootAdmin;
                user.IsEmployee = model.IsEmployee;

                await _repository.UpdateAsync(user, saveChanges: true);
                _logger.LogInformation("Cập nhật user [{Id}] bởi [{By}].", user.Id, _accessor.GetCurrentUserId());

                return Response.Success(new { user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng [{Id}].", model.Id);
                return Response.Fail($"Cập nhật người dùng thất bại: {ex.Message}");
            }
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id không hợp lệ.");

            try
            {
                var user = await _repository.FindAsync<AppUser>(id);
                if (user is null) return Response.Fail("Người dùng không tồn tại.");

                var currentUserId = _accessor.GetCurrentUserId();
                if (user.Id == currentUserId)
                    return Response.Fail("Không thể tự xóa tài khoản của mình.");

                var ok = await _repository.DeleteSoftAsync(user);
                if (!ok) return Response.Fail("Không xóa được người dùng.");

                _logger.LogInformation("Xóa mềm user [{Id}] bởi [{By}].", id, currentUserId);
                return Response.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa người dùng [{Id}].", id);
                return Response.Fail($"Xóa người dùng thất bại: {ex.Message}");
            }
        }

        public async Task<Response> DeleteManyAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0) return Response.Fail("Danh sách ID trống.");

            ids = ids.Where(x => x != Guid.Empty).ToList();
            if (ids.Count == 0) return Response.Fail("Danh sách ID không hợp lệ.");

            try
            {
                var currentUserId = _accessor.GetCurrentUserId();
                ids.RemoveAll(id => id == currentUserId);

                if (ids.Count == 0)
                    return Response.Fail("Không thể xóa tài khoản của mình.");

                await _repository.DeleteRangeSoftAsync<AppUser>(x => ids.Contains(x.Id));
                _logger.LogInformation("Xóa mềm [{Count}] users bởi [{By}].", ids.Count, currentUserId);

                return Response.Success(new { deleted = ids.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhiều người dùng.");
                return Response.Fail($"Xóa người dùng thất bại: {ex.Message}");
            }
        }

        public async Task<Response> ChangePasswordAsync(ChangePasswordModel model)
        {
            if (model == null) return Response.Fail("Dữ liệu không hợp lệ.");
            if (model.UserId == Guid.Empty) return Response.Fail("UserId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.CurrentPassword)) return Response.Fail("Mật khẩu hiện tại không được để trống.");
            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
                return Response.Fail("Mật khẩu mới phải có ít nhất 6 ký tự.");

            try
            {
                var user = await _repository.FindAsync<AppUser>(model.UserId);
                if (user is null) return Response.Fail("Người dùng không tồn tại.");

                var hashedCurrent = _cryptorFactory.ToHashPassword(model.CurrentPassword);
                if (user.Password != hashedCurrent)
                    return Response.Fail("Mật khẩu hiện tại không đúng.");

                user.Password = _cryptorFactory.ToHashPassword(model.NewPassword);
                await _repository.UpdateAsync(user, saveChanges: true);
                _logger.LogInformation("User [{Id}] đổi mật khẩu.", model.UserId);

                return Response.Success(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu [{UserId}].", model.UserId);
                return Response.Fail($"Đổi mật khẩu thất bại: {ex.Message}");
            }
        }

        public async Task<Response> ResetPasswordAsync(ResetPasswordModel model)
        {
            if (model == null) return Response.Fail("Dữ liệu không hợp lệ.");
            if (model.UserId == Guid.Empty) return Response.Fail("UserId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
                return Response.Fail("Mật khẩu mới phải có ít nhất 6 ký tự.");

            try
            {
                var user = await _repository.FindAsync<AppUser>(model.UserId);
                if (user is null) return Response.Fail("Người dùng không tồn tại.");

                user.Password = _cryptorFactory.ToHashPassword(model.NewPassword);
                await _repository.UpdateAsync(user, saveChanges: true);
                _logger.LogInformation("Admin reset mật khẩu user [{UserId}].", model.UserId);

                return Response.Success(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset mật khẩu [{UserId}].", model.UserId);
                return Response.Fail($"Reset mật khẩu thất bại: {ex.Message}");
            }
        }

        public async Task<Response> UpdateAvatarAsync(Guid userId, string? avatarFileName)
        {
            if (userId == Guid.Empty) return Response.Fail("UserId không hợp lệ.");

            try
            {
                var user = await _repository.FindAsync<AppUser>(userId);
                if (user is null) return Response.Fail("Người dùng không tồn tại.");

                user.Avatar = avatarFileName;
                await _repository.UpdateAsync(user, saveChanges: true);
                _logger.LogInformation("Cập nhật avatar user [{UserId}] -> [{Avatar}].", userId, avatarFileName ?? "(null)");

                return Response.Success(new { avatar = avatarFileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật avatar [{UserId}].", userId);
                return Response.Fail($"Cập nhật avatar thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy toàn bộ người dùng đang hoạt động (không phân trang).
        /// </summary>
        public async Task<Response> GetAllUsersAsync()
        {
            var users = await _repository.GetQueryable<AppUser>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new UserModel(x)).ToListAsync();

            return Response.Success(users);
        }

    }
}
