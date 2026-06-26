using Application.CloudServices.Dtos;
using Core;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.CloudServices
{
    public interface IResourcePermissionService
    {
        Task<Response> GetPermissionsAsync(EnumResourceType type, Guid resourceId);
        Task<Response> SetPermissionAsync(SetResourcePermissionDto dto);
        Task<Response> RemovePermissionAsync(Guid id);
        Task<Response> GetSharedWithMeAsync(int page = 1, int pageSize = 40, string? keyword = null);
    }

    public class ResourcePermissionService : IResourcePermissionService
    {
        private readonly IRepository _repository;
        private readonly IAppContextAccessor _accessor;
        private readonly ILogger<ResourcePermissionService> _logger;

        public ResourcePermissionService(
            IRepository repository,
            IAppContextAccessor accessor,
            ILogger<ResourcePermissionService> logger)
        {
            _repository = repository;
            _accessor = accessor;
            _logger = logger;
        }

        public async Task<Response> GetPermissionsAsync(EnumResourceType type, Guid resourceId)
        {
            if (resourceId == Guid.Empty) return Response.Fail("ResourceId is invalid.");

            try
            {
                var permissions = await _repository.GetQueryable<CloudResourcePermission>()
                    .AsNoTracking()
                    .Where(p => p.ResourceType == type && p.ResourceId == resourceId && !p.IsDeleted)
                    .Select(p => new ResourcePermissionDto
                    {
                        Id = p.Id,
                        ResourceType = p.ResourceType,
                        ResourceId = p.ResourceId,
                        UserId = p.UserId,
                        UserName = p.User != null ? p.User.UserName : string.Empty,
                        Name = p.User != null ? p.User.Name : string.Empty,
                        CanView = p.CanView,
                        CanAdd = p.CanAdd,
                        CanEdit = p.CanEdit,
                        CanDelete = p.CanDelete
                    })
                    .ToListAsync();

                return Response.Success(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for [{Type}] [{ResourceId}].", type, resourceId);
                return Response.Fail("Failed to load permissions.");
            }
        }

        public async Task<Response> SetPermissionAsync(SetResourcePermissionDto dto)
        {
            if (dto == null) return Response.Fail("Invalid data.");
            if (dto.ResourceId == Guid.Empty) return Response.Fail("ResourceId is invalid.");
            if (dto.UserId == Guid.Empty) return Response.Fail("UserId is invalid.");

            try
            {
                // Check if permission already exists for same user + resource
                var existing = await _repository.FindAsync<CloudResourcePermission>(
                    p => p.ResourceType == dto.ResourceType
                         && p.ResourceId == dto.ResourceId
                         && p.UserId == dto.UserId
                         && !p.IsDeleted);

                if (existing != null)
                {
                    // Update existing
                    existing.CanView = dto.CanView;
                    existing.CanAdd = dto.CanAdd;
                    existing.CanEdit = dto.CanEdit;
                    existing.CanDelete = dto.CanDelete;
                    await _repository.UpdateAsync(existing, saveChanges: true);

                    _logger.LogInformation("Updated permission [{Id}] for user [{UserId}] on [{Type}] [{ResourceId}].",
                        existing.Id, dto.UserId, dto.ResourceType, dto.ResourceId);

                    return Response.Success(new { existing.Id });
                }
                else
                {
                    // Create new
                    var permission = new CloudResourcePermission
                    {
                        ResourceType = dto.ResourceType,
                        ResourceId = dto.ResourceId,
                        UserId = dto.UserId,
                        CanView = dto.CanView,
                        CanAdd = dto.CanAdd,
                        CanEdit = dto.CanEdit,
                        CanDelete = dto.CanDelete
                    };

                    var result = await _repository.AddAsync(permission);

                    _logger.LogInformation("Created permission [{Id}] for user [{UserId}] on [{Type}] [{ResourceId}].",
                        result.Id, dto.UserId, dto.ResourceType, dto.ResourceId);

                    return Response.Success(new { result.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting permission for [{Type}] [{ResourceId}].", dto.ResourceType, dto.ResourceId);
                return Response.Fail($"Failed to set permission: {ex.Message}");
            }
        }

        public async Task<Response> RemovePermissionAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var permission = await _repository.FindAsync<CloudResourcePermission>(id);
                if (permission == null) return Response.Fail("Permission not found.");

                await _repository.DeleteAsync(permission);

                _logger.LogInformation("Removed permission [{Id}] by [{By}].", id, _accessor.GetCurrentUserId());
                return Response.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission [{Id}].", id);
                return Response.Fail($"Failed to remove permission: {ex.Message}");
            }
        }

        public async Task<Response> GetSharedWithMeAsync(int page = 1, int pageSize = 40, string? keyword = null)
        {
            try
            {
                var currentUserId = _accessor.GetCurrentUserId();
                var kw = keyword?.Trim().ToLower() ?? "";
                var hasKw = !string.IsNullOrWhiteSpace(keyword);

                var perms = _repository.GetQueryable<CloudResourcePermission>()
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserId && p.CanView && !p.IsDeleted);

                var folderIds = perms
                    .Where(p => p.ResourceType == EnumResourceType.Folder)
                    .Select(p => p.ResourceId);

                var fileIds = perms
                    .Where(p => p.ResourceType == EnumResourceType.File)
                    .Select(p => p.ResourceId);

                var folderQuery = _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => !f.IsDeleted && folderIds.Contains(f.Id));

                if (hasKw)
                    folderQuery = folderQuery.Where(f => f.Keyword.Contains(kw));

                var totalFolders = await folderQuery.CountAsync();
                var folders = await folderQuery
                    .OrderBy(f => f.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FolderListItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        ParentId = f.ParentId,
                        SizeInBytes = f.SizeInBytes,
                        CreatedAt = f.CreatedAt,
                        CreatedByName = f.Owner != null ? f.Owner.Name : string.Empty,
                    })
                    .ToListAsync();

                var fileQuery = _repository.GetQueryable<CloudFile>()
                    .AsNoTracking()
                    .Where(f => !f.IsDeleted && fileIds.Contains(f.Id));

                if (hasKw)
                    fileQuery = fileQuery.Where(f => f.Keyword.Contains(kw));

                var totalFiles = await fileQuery.CountAsync();
                var files = await fileQuery
                    .OrderBy(f => f.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FileListItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        SizeInBytes = f.SizeInBytes,
                        ContentType = f.ContentType,
                        Extension = f.Extension,
                        FolderId = f.FolderId,
                        CreatedAt = f.CreatedAt,
                        CreatedByName = f.Owner != null ? f.Owner.Name : string.Empty,
                    })
                    .ToListAsync();

                return Response.Success(new
                {
                    folders = new { items = folders, totalCount = totalFolders },
                    files = new { items = files, totalCount = totalFiles },
                    page,
                    pageSize,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared-with-me list.");
                return Response.Fail("Failed to load shared items.");
            }
        }
    }
}
