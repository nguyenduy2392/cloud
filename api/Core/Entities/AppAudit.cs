using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class AppAudit : EntityBase
    {
        /// <summary>
        /// Id của entity liên quan (ví dụ: CustomerId khi thêm tương tác)
        /// </summary>
        public Guid ReferenceId { get; set; }

        /// <summary>
        /// Tên entity (ví dụ: Customer, CustomerInteraction)
        /// </summary>
        public string Entity { get; set; } = string.Empty;

        /// <summary>
        /// Ngày hành động
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Mô tả hành động
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ IP
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Id của user đang đăng nhập
        /// </summary>
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; } = null!;
    }
}
