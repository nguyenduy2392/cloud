using Core;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.CloudServices
{
    public class TrashItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "file" | "folder"
        public DateTime DeletedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public long SizeInBytes { get; set; }
        public string? Extension { get; set; }
    }

    public class RestoreResultDto
    {
        public Guid Id { get; set; }
        public string? MergedIntoId { get; set; }
        public string? MergedIntoName { get; set; }
    }

    public interface ITrashService
    {
        Task<Response> GetTrashAsync();
        Task<Response> RestoreAsync(Guid id);
        Task<Response> PermanentDeleteAsync(Guid id);
        Task<Response> EmptyTrashAsync();
        Task CleanupExpiredTrashAsync(Guid userId);
    }

    public class TrashService : ITrashService
    {
        // Use AppDbContext directly to bypass GetQueryable's IsDeleted=false filter
        private readonly AppDbContext _db;
        private readonly IRepository _repository;
        private readonly IAppContextAccessor _accessor;
        private readonly ILogger<TrashService> _logger;

        public TrashService(AppDbContext db, IRepository repository, IAppContextAccessor accessor, ILogger<TrashService> logger)
        {
            _db = db;
            _repository = repository;
            _accessor = accessor;
            _logger = logger;
        }

        public async Task<Response> GetTrashAsync()
        {
            try
            {
                var userId = _accessor.GetCurrentUserId()!.Value;

                var files = await _db.Set<CloudFile>()
                    .AsNoTracking()
                    .Where(f => f.IsDeleted && f.DeletedAt != null && f.OwnerId == userId)
                    .Select(f => new TrashItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Type = "file",
                        DeletedAt = f.DeletedAt!.Value,
                        ExpiresAt = f.DeletedAt!.Value.AddDays(30),
                        SizeInBytes = f.SizeInBytes,
                        Extension = f.Extension
                    })
                    .ToListAsync();

                var folders = await _db.Set<CloudFolder>()
                    .AsNoTracking()
                    .Where(f => f.IsDeleted && f.DeletedAt != null && f.OwnerId == userId)
                    .Select(f => new TrashItemDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Type = "folder",
                        DeletedAt = f.DeletedAt!.Value,
                        ExpiresAt = f.DeletedAt!.Value.AddDays(30),
                        SizeInBytes = f.SizeInBytes
                    })
                    .ToListAsync();

                var items = files.Concat(folders)
                    .OrderByDescending(x => x.DeletedAt)
                    .ToList();

                return Response.Success(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trash.");
                return Response.Fail("Failed to load trash.");
            }
        }

        public async Task<Response> RestoreAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var userId = _accessor.GetCurrentUserId()!.Value;

                var file = await _db.Set<CloudFile>()
                    .FirstOrDefaultAsync(f => f.Id == id && f.IsDeleted && f.DeletedAt != null && f.OwnerId == userId);

                if (file != null)
                {
                    if (file.FolderId.HasValue)
                    {
                        var parentDeleted = await _db.Set<CloudFolder>()
                            .AnyAsync(f => f.Id == file.FolderId.Value && f.IsDeleted);
                        if (parentDeleted) file.FolderId = null;
                    }

                    file.IsDeleted = false;
                    file.DeletedAt = null;
                    file.ModifiedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    _logger.LogInformation("Restored file [{Id}].", id);
                    return Response.Success(new RestoreResultDto { Id = id });
                }

                var folder = await _db.Set<CloudFolder>()
                    .FirstOrDefaultAsync(f => f.Id == id && f.IsDeleted && f.DeletedAt != null && f.OwnerId == userId);
                if (folder == null) return Response.Fail("Item not found in trash.");

                if (!string.IsNullOrEmpty(folder.Tag))
                {
                    var activeTagged = await _db.Set<CloudFolder>()
                        .Where(f => f.OwnerId == userId && f.Tag == folder.Tag && !f.IsDeleted && f.Id != id)
                        .FirstOrDefaultAsync();

                    if (activeTagged != null)
                    {
                        await MergeFolderIntoAsync(folder.Id, activeTagged.Id);
                        await HardDeleteFolderRecordAsync(folder.Id);

                        _logger.LogInformation("Restored tagged folder [{Id}] merged into [{ActiveId}].", id, activeTagged.Id);
                        return Response.Success(new RestoreResultDto
                        {
                            Id = activeTagged.Id,
                            MergedIntoId = activeTagged.Id.ToString(),
                            MergedIntoName = activeTagged.Name
                        });
                    }
                }

                if (folder.ParentId.HasValue)
                {
                    var parentDeleted = await _db.Set<CloudFolder>()
                        .AnyAsync(f => f.Id == folder.ParentId.Value && f.IsDeleted);
                    if (parentDeleted) folder.ParentId = null;
                }

                folder.IsDeleted = false;
                folder.DeletedAt = null;
                folder.ModifiedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await RestoreDescendantsAsync(folder.Id);

                _logger.LogInformation("Restored folder [{Id}].", id);
                return Response.Success(new RestoreResultDto { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring [{Id}].", id);
                return Response.Fail($"Failed to restore: {ex.Message}");
            }
        }

        public async Task<Response> PermanentDeleteAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var userId = _accessor.GetCurrentUserId()!.Value;

                var file = await _db.Set<CloudFile>()
                    .FirstOrDefaultAsync(f => f.Id == id && f.IsDeleted && f.OwnerId == userId);
                if (file != null)
                {
                    var size = file.SizeInBytes;
                    var fileId = file.Id;
                    DeletePhysicalFile(file.FilePath);
                    _db.Set<CloudFile>().Remove(file);
                    await _db.SaveChangesAsync();
                    await DeletePermissionsForResourcesAsync([], [fileId]);
                    await DecrementUserStorageAsync(userId, size);
                    _logger.LogInformation("Permanently deleted file [{Id}].", id);
                    return Response.Success();
                }

                var folder = await _db.Set<CloudFolder>()
                    .FirstOrDefaultAsync(f => f.Id == id && f.IsDeleted && f.OwnerId == userId);
                if (folder == null) return Response.Fail("Item not found in trash.");

                var folderFileSize = await CollectTotalFileSizeAsync(folder.Id);
                await PermanentDeleteFolderAsync(folder.Id);
                await DecrementUserStorageAsync(userId, folderFileSize);
                _logger.LogInformation("Permanently deleted folder [{Id}].", id);
                return Response.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting [{Id}].", id);
                return Response.Fail($"Failed to permanently delete: {ex.Message}");
            }
        }

        public async Task<Response> EmptyTrashAsync()
        {
            try
            {
                var userId = _accessor.GetCurrentUserId()!.Value;

                var trashFiles = await _db.Set<CloudFile>()
                    .Where(f => f.IsDeleted && f.DeletedAt != null && f.OwnerId == userId)
                    .ToListAsync();
                var totalFileSize = trashFiles.Sum(f => f.SizeInBytes);
                foreach (var f in trashFiles) DeletePhysicalFile(f.FilePath);
                _db.Set<CloudFile>().RemoveRange(trashFiles);
                await _db.SaveChangesAsync();

                var trashFolderIds = await _db.Set<CloudFolder>()
                    .Where(f => f.IsDeleted && f.DeletedAt != null && f.OwnerId == userId)
                    .Select(f => f.Id)
                    .ToListAsync();
                long totalFolderFileSize = 0;
                foreach (var folderId in trashFolderIds)
                {
                    totalFolderFileSize += await CollectTotalFileSizeAsync(folderId);
                    await PermanentDeleteFolderAsync(folderId);
                }

                await DecrementUserStorageAsync(userId, totalFileSize + totalFolderFileSize);
                _logger.LogInformation("Emptied trash for user [{UserId}].", userId);
                return Response.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error emptying trash.");
                return Response.Fail($"Failed to empty trash: {ex.Message}");
            }
        }

        public async Task CleanupExpiredTrashAsync(Guid userId)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);

            var expiredFiles = await _db.Set<CloudFile>()
                .Where(f => f.IsDeleted && f.DeletedAt != null && f.DeletedAt < cutoff && f.OwnerId == userId)
                .ToListAsync();
            var totalExpiredSize = expiredFiles.Sum(f => f.SizeInBytes);
            foreach (var file in expiredFiles) DeletePhysicalFile(file.FilePath);
            _db.Set<CloudFile>().RemoveRange(expiredFiles);
            if (expiredFiles.Count > 0) await _db.SaveChangesAsync();

            var expiredFolderIds = await _db.Set<CloudFolder>()
                .Where(f => f.IsDeleted && f.DeletedAt != null && f.DeletedAt < cutoff && f.OwnerId == userId)
                .Select(f => f.Id)
                .ToListAsync();
            long totalExpiredFolderSize = 0;
            foreach (var folderId in expiredFolderIds)
            {
                totalExpiredFolderSize += await CollectTotalFileSizeAsync(folderId);
                await PermanentDeleteFolderAsync(folderId);
            }

            if (expiredFiles.Count > 0 || expiredFolderIds.Count > 0)
            {
                await DecrementUserStorageAsync(userId, totalExpiredSize + totalExpiredFolderSize);
                _logger.LogInformation(
                    "Cleaned up expired trash for user [{UserId}]: {Files} file(s), {Folders} folder(s).",
                    userId, expiredFiles.Count, expiredFolderIds.Count);
            }
        }

        #region Private helpers

        private async Task DecrementUserStorageAsync(Guid userId, long bytes)
        {
            if (bytes <= 0) return;
            var storage = await _db.Set<CloudUserStorage>()
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            if (storage == null) return;
            storage.UsedBytes = Math.Max(0, storage.UsedBytes - bytes);
            await _db.SaveChangesAsync();
        }

        private async Task<long> CollectTotalFileSizeAsync(Guid folderId)
        {
            var fileSize = await _db.Set<CloudFile>()
                .Where(f => f.FolderId == folderId)
                .SumAsync(f => (long?)f.SizeInBytes) ?? 0;

            var childIds = await _db.Set<CloudFolder>()
                .Where(f => f.ParentId == folderId)
                .Select(f => f.Id)
                .ToListAsync();

            foreach (var childId in childIds)
                fileSize += await CollectTotalFileSizeAsync(childId);

            return fileSize;
        }

        private async Task RestoreDescendantsAsync(Guid folderId)
        {
            var files = await _db.Set<CloudFile>()
                .Where(f => f.FolderId == folderId && f.IsDeleted && f.DeletedAt == null)
                .ToListAsync();
            foreach (var f in files) { f.IsDeleted = false; f.ModifiedAt = DateTime.UtcNow; }
            if (files.Count > 0) await _db.SaveChangesAsync();

            var childFolders = await _db.Set<CloudFolder>()
                .Where(f => f.ParentId == folderId && f.IsDeleted && f.DeletedAt == null)
                .ToListAsync();
            foreach (var child in childFolders)
            {
                child.IsDeleted = false;
                child.ModifiedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await RestoreDescendantsAsync(child.Id);
            }
        }

        private async Task MergeFolderIntoAsync(Guid sourceFolderId, Guid targetFolderId)
        {
            var files = await _db.Set<CloudFile>()
                .Where(f => f.FolderId == sourceFolderId && f.IsDeleted)
                .ToListAsync();
            foreach (var f in files)
            {
                f.FolderId = targetFolderId;
                f.IsDeleted = false;
                f.DeletedAt = null;
                f.ModifiedAt = DateTime.UtcNow;
            }
            if (files.Count > 0) await _db.SaveChangesAsync();

            var childFolders = await _db.Set<CloudFolder>()
                .Where(f => f.ParentId == sourceFolderId && f.IsDeleted)
                .ToListAsync();
            foreach (var child in childFolders)
            {
                child.ParentId = targetFolderId;
                child.IsDeleted = false;
                child.DeletedAt = null;
                child.ModifiedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await RestoreDescendantsAsync(child.Id);
            }
        }

        private async Task HardDeleteFolderRecordAsync(Guid folderId)
        {
            var folder = await _db.Set<CloudFolder>().FirstOrDefaultAsync(f => f.Id == folderId);
            if (folder != null) { _db.Set<CloudFolder>().Remove(folder); await _db.SaveChangesAsync(); }
        }

        internal async Task PermanentDeleteFolderAsync(Guid folderId)
        {
            var allFiles = new List<CloudFile>();
            await CollectAllFilesAsync(folderId, allFiles);

            var allFolderIds = new List<Guid>();
            await CollectAllFolderIdsAsync(folderId, allFolderIds);

            foreach (var file in allFiles) DeletePhysicalFile(file.FilePath);
            _db.Set<CloudFile>().RemoveRange(allFiles);
            if (allFiles.Count > 0) await _db.SaveChangesAsync();

            await DeleteFolderTreeAsync(folderId);

            await DeletePermissionsForResourcesAsync(allFolderIds, allFiles.Select(f => f.Id));
        }

        private async Task CollectAllFilesAsync(Guid folderId, List<CloudFile> result)
        {
            var files = await _db.Set<CloudFile>()
                .Where(f => f.FolderId == folderId)
                .ToListAsync();
            result.AddRange(files);

            var childIds = await _db.Set<CloudFolder>()
                .Where(f => f.ParentId == folderId)
                .Select(f => f.Id)
                .ToListAsync();
            foreach (var childId in childIds)
                await CollectAllFilesAsync(childId, result);
        }

        private async Task CollectAllFolderIdsAsync(Guid folderId, List<Guid> result)
        {
            result.Add(folderId);
            var childIds = await _db.Set<CloudFolder>()
                .Where(f => f.ParentId == folderId)
                .Select(f => f.Id)
                .ToListAsync();
            foreach (var childId in childIds)
                await CollectAllFolderIdsAsync(childId, result);
        }

        private async Task DeletePermissionsForResourcesAsync(IEnumerable<Guid> folderIds, IEnumerable<Guid> fileIds)
        {
            var fList = folderIds.ToList();
            var fiList = fileIds.ToList();
            if (fList.Count == 0 && fiList.Count == 0) return;

            var toDelete = await _db.Set<CloudResourcePermission>()
                .Where(p =>
                    (p.ResourceType == EnumResourceType.Folder && fList.Contains(p.ResourceId)) ||
                    (p.ResourceType == EnumResourceType.File && fiList.Contains(p.ResourceId)))
                .ToListAsync();

            if (toDelete.Count == 0) return;
            _db.Set<CloudResourcePermission>().RemoveRange(toDelete);
            await _db.SaveChangesAsync();
        }

        private async Task DeleteFolderTreeAsync(Guid folderId)
        {
            var childIds = await _db.Set<CloudFolder>()
                .Where(f => f.ParentId == folderId)
                .Select(f => f.Id)
                .ToListAsync();
            foreach (var childId in childIds)
                await DeleteFolderTreeAsync(childId);

            var folder = await _db.Set<CloudFolder>().FirstOrDefaultAsync(f => f.Id == folderId);
            if (folder != null) { _db.Set<CloudFolder>().Remove(folder); await _db.SaveChangesAsync(); }
        }

        internal void DeletePhysicalFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", filePath);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete physical file [{FilePath}].", filePath);
            }
        }

        #endregion
    }
}
