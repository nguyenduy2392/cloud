namespace Core
{
    public interface IAppContextAccessor
    {
        /// <summary>
        /// Id người dùng đang đăng nhập
        /// </summary>
        /// <returns></returns>
        Guid? GetCurrentUserId();

        /// <summary>
        /// Địa chỉ Ip đang sử dụng
        /// </summary>
        /// <returns></returns>
        string GetIpAddress();

        /// <summary>
        /// Tên người dùng đang đăng nhập
        /// </summary>
        /// <returns></returns>
        string GetUserFullName();

        /// <summary>
        /// Người dùng hiện tại có phải admin không?
        /// </summary>
        /// <returns></returns>
        bool IsRootAdmin();

        /// <summary>
        /// Lấy chuỗi kết nối
        /// </summary>
        /// <returns></returns>
        string GetConnectionString(string? databaseName = "");

        /// <summary>
        /// Lấy cơ sở dữ liệu qua token
        /// </summary>
        /// <returns></returns>
        string GetDatabaseName();

        /// <summary>
        /// Thư mục lưu trữ
        /// </summary>
        /// <returns></returns>
        string GetFolderName();

        /// <summary>
        /// Ghi đè định danh đơn vị cho scope hiện tại (dùng cho background job không có HttpContext)
        /// </summary>
        void SetDatabaseOverride(string tenant);
    }
}
