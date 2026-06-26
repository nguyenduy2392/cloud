using Application.UserServices;
using Application.UserServices.Dtos;
using Core;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Api.Controllers
{
    /// <summary>
    /// API quản lý người dùng hệ thống.
    /// </summary>
    [Route("api/users")]
    public class UsersController : BaseController
    {
        private readonly IUserService _service;
        private readonly string _avatarFolder;
        private readonly string _tenant;

        public UsersController(IUserService service, IAppContextAccessor accessor)
        {
            _service = service;
            _tenant = accessor.GetDatabaseName()?.ToLower() ?? "default";
            _avatarFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data", _tenant, "Avatars");
            if (!Directory.Exists(_avatarFolder))
                Directory.CreateDirectory(_avatarFolder);
        }

        /// <summary>
        /// Danh sách người dùng (có phân trang + tìm kiếm).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null)
        {
            return Ok(await _service.GetAllAsync(page, pageSize, keyword));
        }

        /// <summary>
        /// Chi tiết người dùng theo Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Tạo mới người dùng.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật người dùng.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            model.Id = id;
            var result = await _service.UpdateAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Xóa một người dùng (xóa mềm).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Xóa nhiều người dùng (xóa mềm).
        /// </summary>
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            var result = await _service.DeleteManyAsync(ids);
            return Ok(result);
        }

        /// <summary>
        /// Đổi mật khẩu người dùng (yêu cầu mật khẩu hiện tại).
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.ChangePasswordAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Reset mật khẩu người dùng (admin đặt lại mà không cần mật khẩu cũ).
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.ResetPasswordAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Upload ảnh đại diện cho người dùng.
        /// </summary>
        [HttpPost("{id:guid}/avatar")]
        public async Task<IActionResult> UploadAvatar([FromRoute] Guid id, IFormFile file)
        {
            if (id == Guid.Empty)
                return BadRequest(Core.Common.Response.Fail("Id không hợp lệ."));

            if (file == null || file.Length == 0)
                return BadRequest(Core.Common.Response.Fail("Vui lòng chọn file ảnh."));

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(Core.Common.Response.Fail("Chỉ chấp nhận file ảnh: jpg, jpeg, png, gif, webp."));

            // Validate file size (max 2MB)
            const long maxSize = 2 * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest(Core.Common.Response.Fail("Kích thước file không được vượt quá 2MB."));

            try
            {
                // Verify user exists
                var userResp = await _service.GetByIdAsync(id);
                if (!userResp.IsSuccess)
                    return BadRequest(Core.Common.Response.Fail("Người dùng không tồn tại."));

                // Delete old avatar if exists
                if (userResp.Data is UserModel user && !string.IsNullOrEmpty(user.Avatar))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", user.Avatar.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Generate unique filename
                var fileName = $"{id}{ext}";
                var filePath = Path.Combine(_avatarFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update avatar path in user
                var updateResp = await _service.UpdateAvatarAsync(id, $"{_tenant}/Avatars/{fileName}");
                return Ok(updateResp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Core.Common.Response.Fail($"Lỗi khi upload avatar: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xóa ảnh đại diện của người dùng.
        /// </summary>
        [HttpDelete("{id:guid}/avatar")]
        public async Task<IActionResult> DeleteAvatar([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(Core.Common.Response.Fail("Id không hợp lệ."));

            try
            {
                // Get current avatar
                var userResp = await _service.GetByIdAsync(id);
                if (!userResp.IsSuccess)
                    return BadRequest(Core.Common.Response.Fail("Người dùng không tồn tại."));

                if (userResp.Data is UserModel user && !string.IsNullOrEmpty(user.Avatar))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", user.Avatar.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                var result = await _service.UpdateAvatarAsync(id, null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Core.Common.Response.Fail($"Lỗi khi xóa avatar: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy toàn bộ người dùng đang hoạt động (không phân trang).
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _service.GetAllUsersAsync());
        }

    }
}
