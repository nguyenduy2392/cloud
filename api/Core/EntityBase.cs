using System.ComponentModel.DataAnnotations;

namespace Core
{
    /// <summary>
    /// Thực thể chung
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// Định danh thực thể
        /// </summary>
        [Key]
        public Guid Id { get; set; }


        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tài khoản tạo
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Thời gian cập nhật cuối
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Tài khoản cập nhật cuối
        /// </summary>
        public Guid? ModifiedBy  { get; set; }

        /// <summary>
        /// Đã bị xóa
        /// </summary>
        public bool IsDeleted { get; set; } = false;


    }
}
