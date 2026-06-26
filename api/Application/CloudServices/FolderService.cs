using Application.CloudServices.Dtos;
using Core;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.CloudServices
{
    public interface IFolderService
    {
        Task<Response> GetContentsAsync(Guid? parentId, int page = 1, int pageSize = 50, string? keyword = null);
        Task<Response> GetTreeAsync();
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(CreateFolderDto dto);
        Task<Response> RenameAsync(Guid id, string newName);
        Task<Response> MoveAsync(Guid id, Guid? newParentId);
        Task<Response> DeleteAsync(Guid id);
        Task<Response> GetSharedWithMeAsync(int page = 1, int pageSize = 50);
    }

    public class FolderService : IFolderService
    {
        private readonly IRepository _repository;
        private readonly IAppContextAccessor _accessor;
        private readonly ILogger<FolderService> _logger;

        public FolderService(
            IRepository repository,
            IAppContextAccessor accessor,
            ILogger<FolderService> logger)
        {
            _repository = repository;
            _accessor = accessor;
            _logger = logger;
        }

        public async Task<Response> GetContentsAsync(Guid? parentId, int page = 1, int pageSize = 50, string? keyword = null)
        {
            try
            {
                var currentUserId = _accessor.GetCurrentUserId();
                var hasKeyword = !string.IsNullOrWhiteSpace(keyword);
                var kw = keyword?.Trim().ToLower() ?? "";

                // Check access: owner or has permission on this folder or any ancestor
                if (parentId.HasValue && !await HasAccessAsync(parentId.Value, currentUserId!.Value))
                    return Response.Fail("Bạn không có quyền truy cập thư mục này.");

                // Folders at this level (owned by anyone — access is already verified on the parent)
                var folderQuery = _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => !f.IsDeleted && f.ParentId == parentId);

                // At root level, only show user's own folders
                if (!parentId.HasValue)
                    folderQuery = folderQuery.Where(f => f.OwnerId == currentUserId);

                if (hasKeyword)
                    folderQuery = folderQuery.Where(f => f.Keyword.Contains(kw));

                var totalFolders = await folderQuery.CountAsync();
                var folders = await folderQuery
                    .OrderByDescending(f => f.ModifiedAt)
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
                        SubFolderCount = f.Children.Count(c => !c.IsDeleted),
                        FileCount = f.Files.Count(fi => !fi.IsDeleted)
                    })
                    .ToListAsync();

                // Files at this level
                var fileQuery = _repository.GetQueryable<CloudFile>()
                    .AsNoTracking()
                    .Where(f => !f.IsDeleted && f.FolderId == parentId);

                if (!parentId.HasValue)
                    fileQuery = fileQuery.Where(f => f.OwnerId == currentUserId);

                if (hasKeyword)
                    fileQuery = fileQuery.Where(f => f.Keyword.Contains(kw));

                var totalFiles = await fileQuery.CountAsync();
                var files = await fileQuery
                    .OrderByDescending(f => f.ModifiedAt)
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
                        CreatedByName = f.Owner != null ? f.Owner.Name : string.Empty
                    })
                    .ToListAsync();

                var breadcrumbs = await BuildBreadcrumbsAsync(parentId);

                return Response.Success(new
                {
                    breadcrumbs,
                    folders = new { items = folders, totalCount = totalFolders },
                    files = new { items = files, totalCount = totalFiles },
                    page,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting folder contents for parentId [{ParentId}].", parentId);
                return Response.Fail("Failed to load folder contents.");
            }
        }

        public async Task<Response> GetTreeAsync()
        {
            try
            {
                var currentUserId = _accessor.GetCurrentUserId();

                var allFolders = await _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => !f.IsDeleted && f.OwnerId == currentUserId)
                    .OrderBy(f => f.Name)
                    .Select(f => new FolderTreeItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        ParentId = f.ParentId
                    })
                    .ToListAsync();

                var lookup = allFolders.ToLookup(f => f.ParentId);
                var roots = BuildTree(lookup, null);

                return Response.Success(roots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting folder tree.");
                return Response.Fail("Failed to load folder tree.");
            }
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var folder = await _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => f.Id == id && !f.IsDeleted)
                    .Select(f => new FolderListItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        ParentId = f.ParentId,
                        SizeInBytes = f.SizeInBytes,
                        CreatedAt = f.CreatedAt,
                        CreatedByName = f.Owner != null ? f.Owner.Name : string.Empty,
                        SubFolderCount = f.Children.Count(c => !c.IsDeleted),
                        FileCount = f.Files.Count(fi => !fi.IsDeleted)
                    })
                    .FirstOrDefaultAsync();

                if (folder == null) return Response.Fail("Folder not found.");

                return Response.Success(folder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting folder [{Id}].", id);
                return Response.Fail("Failed to load folder.");
            }
        }

        public async Task<Response> CreateAsync(CreateFolderDto dto)
        {
            if (dto == null) return Response.Fail("Invalid data.");
            if (string.IsNullOrWhiteSpace(dto.Name)) return Response.Fail("Folder name is required.");

            try
            {
                var currentUserId = _accessor.GetCurrentUserId();

                // Validate parent exists if provided
                if (dto.ParentId.HasValue)
                {
                    var parentExists = await _repository.IsExistsAsync<CloudFolder>(
                        f => f.Id == dto.ParentId.Value && !f.IsDeleted);
                    if (!parentExists) return Response.Fail("Parent folder not found.");
                }

                // Check duplicate name in same parent
                var duplicate = await _repository.IsExistsAsync<CloudFolder>(
                    f => f.Name == dto.Name.Trim() && f.ParentId == dto.ParentId
                         && f.OwnerId == currentUserId && !f.IsDeleted);
                if (duplicate) return Response.Fail($"Folder \"{dto.Name.Trim()}\" already exists in this location.");

                var folder = new CloudFolder
                {
                    Name = dto.Name.Trim(),
                    Keyword = Helper.StringHelper.BuildKeyword(dto.Name.Trim()),
                    ParentId = dto.ParentId,
                    OwnerId = currentUserId!.Value,
                    SizeInBytes = 0
                };

                var result = await _repository.AddAsync(folder);
                _logger.LogInformation("Created folder [{Name}] by [{By}].", folder.Name, currentUserId);

                return Response.Success(new { result.Id, result.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder [{Name}].", dto.Name);
                return Response.Fail($"Failed to create folder: {ex.Message}");
            }
        }

        public async Task<Response> RenameAsync(Guid id, string newName)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");
            if (string.IsNullOrWhiteSpace(newName)) return Response.Fail("Folder name is required.");

            try
            {
                var folder = await _repository.FindAsync<CloudFolder>(
                    f => f.Id == id && !f.IsDeleted);
                if (folder == null) return Response.Fail("Folder not found.");

                // Check duplicate name in same parent
                var duplicate = await _repository.IsExistsAsync<CloudFolder>(
                    f => f.Id != id && f.Name == newName.Trim() && f.ParentId == folder.ParentId
                         && f.OwnerId == folder.OwnerId && !f.IsDeleted);
                if (duplicate) return Response.Fail($"Folder \"{newName.Trim()}\" already exists in this location.");

                folder.Name = newName.Trim();
                folder.Keyword = Helper.StringHelper.BuildKeyword(newName.Trim());
                await _repository.UpdateAsync(folder, saveChanges: true);

                _logger.LogInformation("Renamed folder [{Id}] to [{Name}].", id, newName);
                return Response.Success(new { folder.Id, folder.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming folder [{Id}].", id);
                return Response.Fail($"Failed to rename folder: {ex.Message}");
            }
        }

        public async Task<Response> MoveAsync(Guid id, Guid? newParentId)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var folder = await _repository.FindAsync<CloudFolder>(
                    f => f.Id == id && !f.IsDeleted);
                if (folder == null) return Response.Fail("Folder not found.");

                // Prevent moving to itself
                if (newParentId.HasValue && newParentId.Value == id)
                    return Response.Fail("Cannot move a folder into itself.");

                // Prevent circular reference: check if newParentId is a descendant of this folder
                if (newParentId.HasValue)
                {
                    var parentExists = await _repository.IsExistsAsync<CloudFolder>(
                        f => f.Id == newParentId.Value && !f.IsDeleted);
                    if (!parentExists) return Response.Fail("Target folder not found.");

                    if (await IsDescendantAsync(newParentId.Value, id))
                        return Response.Fail("Cannot move a folder into one of its subfolders.");
                }

                var oldParentId = folder.ParentId;
                folder.ParentId = newParentId;
                await _repository.UpdateAsync(folder, saveChanges: true);

                // Recalculate sizes for old and new parent
                await RecalculateFolderSizeAsync(oldParentId);
                await RecalculateFolderSizeAsync(newParentId);

                _logger.LogInformation("Moved folder [{Id}] to parent [{NewParentId}].", id, newParentId);
                return Response.Success(new { folder.Id, folder.ParentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving folder [{Id}].", id);
                return Response.Fail($"Failed to move folder: {ex.Message}");
            }
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var folder = await _repository.FindAsync<CloudFolder>(
                    f => f.Id == id && !f.IsDeleted);
                if (folder == null) return Response.Fail("Folder not found.");

                var parentId = folder.ParentId;

                // Recursively soft delete all children and files
                await SoftDeleteRecursiveAsync(id);

                // Soft delete the folder itself
                await _repository.DeleteSoftAsync(folder);

                // Recalculate parent folder size
                await RecalculateFolderSizeAsync(parentId);

                _logger.LogInformation("Deleted folder [{Id}] by [{By}].", id, _accessor.GetCurrentUserId());
                return Response.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting folder [{Id}].", id);
                return Response.Fail($"Failed to delete folder: {ex.Message}");
            }
        }

        public async Task<Response> GetSharedWithMeAsync(int page = 1, int pageSize = 50)
        {
            try
            {
                var currentUserId = _accessor.GetCurrentUserId();

                // Get resource IDs where user has CanView permission
                var permissions = await _repository.GetQueryable<CloudResourcePermission>()
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserId && p.CanView && !p.IsDeleted)
                    .ToListAsync();

                var folderIds = permissions
                    .Where(p => p.ResourceType == EnumResourceType.Folder)
                    .Select(p => p.ResourceId)
                    .ToList();

                var fileIds = permissions
                    .Where(p => p.ResourceType == EnumResourceType.File)
                    .Select(p => p.ResourceId)
                    .ToList();

                var folders = await _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => folderIds.Contains(f.Id) && !f.IsDeleted)
                    .OrderByDescending(f => f.ModifiedAt)
                    .Select(f => new FolderListItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        ParentId = f.ParentId,
                        SizeInBytes = f.SizeInBytes,
                        CreatedAt = f.CreatedAt,
                        CreatedByName = f.Owner != null ? f.Owner.Name : string.Empty,
                        SubFolderCount = f.Children.Count(c => !c.IsDeleted),
                        FileCount = f.Files.Count(fi => !fi.IsDeleted)
                    })
                    .ToListAsync();

                var files = await _repository.GetQueryable<CloudFile>()
                    .AsNoTracking()
                    .Where(f => fileIds.Contains(f.Id) && !f.IsDeleted)
                    .OrderByDescending(f => f.ModifiedAt)
                    .Select(f => new FileListItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        SizeInBytes = f.SizeInBytes,
                        ContentType = f.ContentType,
                        Extension = f.Extension,
                        FolderId = f.FolderId,
                        CreatedAt = f.CreatedAt,
                        CreatedByName = f.Owner != null ? f.Owner.Name : string.Empty
                    })
                    .ToListAsync();

                return Response.Success(new
                {
                    folders = new { items = folders, totalCount = folders.Count },
                    files = new { items = files, totalCount = files.Count }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared-with-me items.");
                return Response.Fail("Failed to load shared items.");
            }
        }

        #region Private methods

        private List<FolderTreeItemDto> BuildTree(ILookup<Guid?, FolderTreeItemDto> lookup, Guid? parentId)
        {
            var items = lookup[parentId].ToList();
            foreach (var item in items)
            {
                item.Children = BuildTree(lookup, item.Id);
            }
            return items;
        }

        private async Task<List<FolderListItemDto>> BuildBreadcrumbsAsync(Guid? folderId)
        {
            var crumbs = new List<FolderListItemDto>();
            var currentId = folderId;
            var visited = new HashSet<Guid>();

            while (currentId.HasValue && visited.Add(currentId.Value))
            {
                var folder = await _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => f.Id == currentId.Value && !f.IsDeleted)
                    .Select(f => new FolderListItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        ParentId = f.ParentId,
                    })
                    .FirstOrDefaultAsync();

                if (folder == null) break;
                crumbs.Add(folder);
                currentId = folder.ParentId;
            }

            crumbs.Reverse();
            return crumbs;
        }

        private async Task<bool> HasAccessAsync(Guid folderId, Guid userId)
        {
            var currentId = (Guid?)folderId;
            var visited = new HashSet<Guid>();

            while (currentId.HasValue && visited.Add(currentId.Value))
            {
                var folder = await _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => f.Id == currentId.Value && !f.IsDeleted)
                    .Select(f => new { f.OwnerId, f.ParentId })
                    .FirstOrDefaultAsync();

                if (folder == null) return false;
                if (folder.OwnerId == userId) return true;

                var hasPermission = await _repository.GetQueryable<CloudResourcePermission>()
                    .AnyAsync(p => p.ResourceType == EnumResourceType.Folder
                        && p.ResourceId == currentId.Value
                        && p.UserId == userId
                        && p.CanView && !p.IsDeleted);
                if (hasPermission) return true;

                currentId = folder.ParentId;
            }

            return false;
        }

        private async Task<bool> IsDescendantAsync(Guid potentialDescendantId, Guid ancestorId)
        {
            var currentId = potentialDescendantId;
            var visited = new HashSet<Guid>();

            while (true)
            {
                if (!visited.Add(currentId)) return false; // cycle protection

                var folder = await _repository.GetQueryable<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => f.Id == currentId && !f.IsDeleted)
                    .Select(f => new { f.Id, f.ParentId })
                    .FirstOrDefaultAsync();

                if (folder == null) return false;
                if (folder.ParentId == null) return false;
                if (folder.ParentId == ancestorId) return true;

                currentId = folder.ParentId.Value;
            }
        }

        private async Task SoftDeleteRecursiveAsync(Guid folderId)
        {
            // Soft delete all files in this folder
            var files = await _repository.GetQueryable<CloudFile>()
                .Where(f => f.FolderId == folderId && !f.IsDeleted)
                .ToListAsync();

            foreach (var file in files)
            {
                file.IsDeleted = true;
                file.ModifiedAt = DateTime.Now;
            }

            if (files.Count > 0)
                await _repository.UpdateRangeAsync(files, saveChanges: true);

            // Get child folders and recurse
            var childFolders = await _repository.GetQueryable<CloudFolder>()
                .Where(f => f.ParentId == folderId && !f.IsDeleted)
                .ToListAsync();

            foreach (var child in childFolders)
            {
                await SoftDeleteRecursiveAsync(child.Id);
                child.IsDeleted = true;
                child.ModifiedAt = DateTime.Now;
                await _repository.UpdateAsync(child, saveChanges: true);
            }
        }

        private async Task RecalculateFolderSizeAsync(Guid? folderId)
        {
            if (!folderId.HasValue) return;

            var folder = await _repository.FindAsync<CloudFolder>(
                f => f.Id == folderId.Value && !f.IsDeleted);
            if (folder == null) return;

            // Sum direct file sizes
            var fileSize = await _repository.GetQueryable<CloudFile>()
                .Where(f => f.FolderId == folderId && !f.IsDeleted)
                .SumAsync(f => f.SizeInBytes);

            // Sum children folder sizes
            var childFolderSize = await _repository.GetQueryable<CloudFolder>()
                .Where(f => f.ParentId == folderId && !f.IsDeleted)
                .SumAsync(f => f.SizeInBytes);

            folder.SizeInBytes = fileSize + childFolderSize;
            await _repository.UpdateAsync(folder, saveChanges: true);

            // Propagate up to parent
            await RecalculateFolderSizeAsync(folder.ParentId);
        }

        #endregion
    }
}
