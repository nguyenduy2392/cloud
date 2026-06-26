using Core.Enums;

namespace Application.CloudServices.Dtos
{
    public class ResourcePermissionDto
    {
        public Guid Id { get; set; }
        public EnumResourceType ResourceType { get; set; }
        public Guid ResourceId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class SetResourcePermissionDto
    {
        public EnumResourceType ResourceType { get; set; }
        public Guid ResourceId { get; set; }
        public Guid UserId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
