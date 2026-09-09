namespace Application.UserServices.Dtos
{
    /// <summary>
    /// 1 user đã merge dữ liệu local (Cloud) + org/chức danh (SSO) — trả nguyên, KHÔNG precompute
    /// search key ở server. FE tự xoá dấu + gộp các field thành key để tìm kiếm phía client.
    /// </summary>
    public class UserDirectoryItemDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }

        /// <summary>Tên phòng ban (Org.Title) — 1 user có thể thuộc nhiều org.</summary>
        public List<string> OrgTitles { get; set; } = new();

        /// <summary>Tên viết tắt phòng ban (Org.ShortName).</summary>
        public List<string> OrgShortNames { get; set; } = new();

        /// <summary>Chức danh (OrgRole.Name).</summary>
        public List<string> OrgRoleNames { get; set; } = new();
    }
}
