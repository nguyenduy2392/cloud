using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("CloudFiles")]
    public class CloudFile : EntityBase
    {
        [StringLength(500)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string StoredFileName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string FilePath { get; set; } = string.Empty;

        public Guid? FolderId { get; set; }

        [ForeignKey(nameof(FolderId))]
        public virtual CloudFolder? Folder { get; set; }

        public long SizeInBytes { get; set; }

        [StringLength(200)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(50)]
        public string Extension { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Keyword { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual AppUser Owner { get; set; } = null!;
    }
}
