using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("CloudUserStorages")]
    public class CloudUserStorage : EntityBase
    {
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; } = null!;

        public long UsedBytes { get; set; }

        public long MaxBytes { get; set; } = 1_073_741_824; // 1 GB
    }
}
