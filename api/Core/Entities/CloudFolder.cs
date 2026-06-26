using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("CloudFolders")]
    public class CloudFolder : EntityBase
    {
        [StringLength(500)]
        public string Name { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual CloudFolder? Parent { get; set; }

        public long SizeInBytes { get; set; }

        [StringLength(1000)]
        public string Keyword { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual AppUser Owner { get; set; } = null!;

        public virtual ICollection<CloudFolder> Children { get; set; } = new List<CloudFolder>();
        public virtual ICollection<CloudFile> Files { get; set; } = new List<CloudFile>();
    }
}
