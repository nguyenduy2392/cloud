namespace Application.CloudServices.Dtos
{
    public class FolderListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public long SizeInBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int SubFolderCount { get; set; }
        public int FileCount { get; set; }
    }

    public class CreateFolderDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
    }

    public class FolderTreeItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public List<FolderTreeItemDto> Children { get; set; } = new();
    }
}
