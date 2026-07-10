using Application.CloudServices.Dtos;
using Application.Helper;
using Core;
using Core.Common;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.CloudServices
{
    public interface IFileService
    {
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> UploadAsync(IFormFile file, Guid? folderId, string? folderTag = null);
        Task<(Stream? Stream, string FileName, string ContentType)> DownloadAsync(Guid id);
        Task<Response> RenameAsync(Guid id, string newName);
        Task<Response> MoveAsync(Guid id, Guid? newFolderId);
        Task<Response> MoveToTaggedFolderAsync(Guid id, string tag, string displayName);
        Task<Response> DeleteAsync(Guid id);
        Task<Response> BulkDeleteAsync(List<Guid> ids);
    }

    public class FileService : IFileService
    {
        private readonly IRepository _repository;
        private readonly IAppContextAccessor _accessor;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IRepository repository,
            IAppContextAccessor accessor,
            ILogger<FileService> logger)
        {
            _repository = repository;
            _accessor = accessor;
            _logger = logger;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var file = await _repository.GetQueryable<CloudFile>()
                    .AsNoTracking()
                    .Where(f => f.Id == id && !f.IsDeleted)
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
                    .FirstOrDefaultAsync();

                if (file == null) return Response.Fail("File not found.");

                return Response.Success(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file [{Id}].", id);
                return Response.Fail("Failed to load file.");
            }
        }

        public async Task<Response> UploadAsync(IFormFile file, Guid? folderId, string? folderTag = null)
        {
            if (file == null || file.Length == 0)
                return Response.Fail("Please select a file to upload.");

            try
            {
                var currentUserId = _accessor.GetCurrentUserId();
                var dbName = _accessor.GetDatabaseName()?.ToLower() ?? "default";

                // Nếu có folderTag thì tự get-or-create thư mục hệ thống cho user này
                if (!string.IsNullOrEmpty(folderTag))
                    folderId = await GetOrCreateTaggedFolderAsync(currentUserId!.Value, folderTag);

                // Check storage quota
                var storage = await _repository.FindAsync<CloudUserStorage>(
                    s => s.UserId == currentUserId!.Value && !s.IsDeleted);
                if (storage != null && storage.MaxBytes > 0
                    && storage.UsedBytes + file.Length > storage.MaxBytes)
                {
                    return Response.Fail(
                        $"Dung lượng lưu trữ không đủ. Đã dùng {FormatBytes(storage.UsedBytes)}/{FormatBytes(storage.MaxBytes)}, tệp cần {FormatBytes(file.Length)}.");
                }

                // Validate folder exists if provided
                if (folderId.HasValue)
                {
                    var folderExists = await _repository.IsExistsAsync<CloudFolder>(
                        f => f.Id == folderId.Value && !f.IsDeleted);
                    if (!folderExists) return Response.Fail("Folder not found.");
                }

                var fileId = Guid.NewGuid();
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var contentType = file.ContentType ?? "application/octet-stream";
                var displayName = file.FileName;

                // Một số app mobile (image picker iOS) tự convert HEIC -> JPEG nhưng vẫn giữ tên file gốc .HEIC
                // => nếu đuôi file báo heic/heif nhưng ContentType lại là 1 định dạng ảnh cụ thể khác, tin theo ContentType
                if (HeicConverter.IsHeic(extension))
                {
                    var realExtension = HeicConverter.ExtensionFromContentType(contentType);
                    if (realExtension != null && realExtension != extension)
                    {
                        extension = realExtension;
                        displayName = Path.GetFileNameWithoutExtension(file.FileName) + extension;
                    }
                }

                var storedFileName = $"{fileId}{extension}";

                // Build storage path
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", dbName, "Files");
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                var filePath = Path.Combine(basePath, storedFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // HEIC/HEIF (ảnh chụp từ iPhone) không hiển thị được trên web -> convert sang JPEG để lưu
                if (HeicConverter.IsHeic(extension))
                {
                    var jpegStoredFileName = $"{fileId}.jpg";
                    var jpegFilePath = Path.Combine(basePath, jpegStoredFileName);

                    var converted = await HeicConverter.ConvertToJpegAsync(filePath, jpegFilePath, _logger);
                    if (converted)
                    {
                        File.Delete(filePath);

                        extension = ".jpg";
                        contentType = "image/jpeg";
                        storedFileName = jpegStoredFileName;
                        filePath = jpegFilePath;
                        displayName = Path.GetFileNameWithoutExtension(file.FileName) + ".jpg";
                    }
                }

                var fileLength = new FileInfo(filePath).Length;

                // Create CloudFile record
                var cloudFile = new CloudFile
                {
                    Id = fileId,
                    Name = displayName,
                    Keyword = Helper.StringHelper.BuildKeyword(displayName),
                    StoredFileName = storedFileName,
                    FilePath = $"{dbName}/Files/{storedFileName}",
                    FolderId = folderId,
                    SizeInBytes = fileLength,
                    ContentType = contentType,
                    Extension = extension,
                    OwnerId = currentUserId!.Value
                };

                await _repository.AddAsync(cloudFile);

                // Update folder size up the chain
                await RecalculateFolderSizeAsync(folderId);

                // Update user storage
                await UpdateUserStorageAsync(currentUserId!.Value, fileLength);

                _logger.LogInformation("Uploaded file [{Name}] ({Size} bytes) by [{By}].",
                    cloudFile.Name, fileLength, currentUserId);

                return Response.Success(new UploadResultDto
                {
                    Id = cloudFile.Id,
                    Name = cloudFile.Name,
                    StoredFileName = cloudFile.StoredFileName,
                    FilePath = cloudFile.FilePath,
                    SizeInBytes = cloudFile.SizeInBytes,
                    ContentType = cloudFile.ContentType,
                    Extension = cloudFile.Extension,
                    FolderId = cloudFile.FolderId,
                    CreatedAt = cloudFile.CreatedAt,
                    CreatedByName = (await _repository.GetQueryable<Core.Entities.AppUser>()
                        .Where(u => u.Id == currentUserId!.Value)
                        .Select(u => u.Name)
                        .FirstOrDefaultAsync()) ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file [{FileName}].", file.FileName);
                return Response.Fail($"Failed to upload file: {ex.Message}");
            }
        }

        public async Task<(Stream? Stream, string FileName, string ContentType)> DownloadAsync(Guid id)
        {
            var file = await _repository.FindAsync<CloudFile>(f => f.Id == id && !f.IsDeleted);
            if (file == null)
                return (null, string.Empty, string.Empty);

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", file.FilePath);
            if (!File.Exists(fullPath))
                return (null, string.Empty, string.Empty);

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (stream, file.Name, file.ContentType);
        }

        public async Task<Response> RenameAsync(Guid id, string newName)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");
            if (string.IsNullOrWhiteSpace(newName)) return Response.Fail("File name is required.");

            try
            {
                var file = await _repository.FindAsync<CloudFile>(f => f.Id == id && !f.IsDeleted);
                if (file == null) return Response.Fail("File not found.");

                // Keep the original extension
                var originalExtension = file.Extension;
                var nameWithoutExt = Path.GetFileNameWithoutExtension(newName.Trim());
                file.Name = nameWithoutExt + originalExtension;
                file.Keyword = Helper.StringHelper.BuildKeyword(file.Name);

                await _repository.UpdateAsync(file, saveChanges: true);

                _logger.LogInformation("Renamed file [{Id}] to [{Name}].", id, file.Name);
                return Response.Success(new { file.Id, file.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming file [{Id}].", id);
                return Response.Fail($"Failed to rename file: {ex.Message}");
            }
        }

        public async Task<Response> MoveAsync(Guid id, Guid? newFolderId)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var file = await _repository.FindAsync<CloudFile>(f => f.Id == id && !f.IsDeleted);
                if (file == null) return Response.Fail("File not found.");

                // Validate target folder if provided
                if (newFolderId.HasValue)
                {
                    var folderExists = await _repository.IsExistsAsync<CloudFolder>(
                        f => f.Id == newFolderId.Value && !f.IsDeleted);
                    if (!folderExists) return Response.Fail("Target folder not found.");
                }

                var oldFolderId = file.FolderId;
                file.FolderId = newFolderId;
                await _repository.UpdateAsync(file, saveChanges: true);

                // Recalculate old and new folder sizes
                await RecalculateFolderSizeAsync(oldFolderId);
                await RecalculateFolderSizeAsync(newFolderId);

                _logger.LogInformation("Moved file [{Id}] to folder [{NewFolderId}].", id, newFolderId);
                return Response.Success(new { file.Id, file.FolderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving file [{Id}].", id);
                return Response.Fail($"Failed to move file: {ex.Message}");
            }
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");

            try
            {
                var file = await _repository.FindAsync<CloudFile>(f => f.Id == id && !f.IsDeleted);
                if (file == null) return Response.Fail("File not found.");

                var folderId = file.FolderId;
                var fileSize = file.SizeInBytes;
                var ownerId = file.OwnerId;

                file.DeletedAt = DateTime.UtcNow;
                await _repository.DeleteSoftAsync(file);

                // Recalculate folder size
                await RecalculateFolderSizeAsync(folderId);

                // Dung lượng chỉ giảm khi xóa vĩnh viễn khỏi thùng rác, không giảm khi chuyển vào thùng rác

                _logger.LogInformation("Deleted file [{Id}] by [{By}].", id, _accessor.GetCurrentUserId());
                return Response.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file [{Id}].", id);
                return Response.Fail($"Failed to delete file: {ex.Message}");
            }
        }

        public async Task<Response> BulkDeleteAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0) return Response.Fail("ID list is empty.");

            ids = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0) return Response.Fail("ID list is invalid.");

            try
            {
                var files = await _repository.GetQueryable<CloudFile>()
                    .Where(f => ids.Contains(f.Id) && !f.IsDeleted)
                    .ToListAsync();

                if (files.Count == 0) return Response.Fail("No files found.");

                var affectedFolderIds = files.Select(f => f.FolderId).Distinct().ToList();
                var totalSize = files.Sum(f => f.SizeInBytes);
                var ownerId = files.First().OwnerId;

                var now = DateTime.UtcNow;
                foreach (var file in files)
                {
                    file.IsDeleted = true;
                    file.ModifiedAt = now;
                    file.DeletedAt = now;
                }

                await _repository.UpdateRangeAsync(files, saveChanges: true);

                // Recalculate affected folder sizes
                foreach (var folderId in affectedFolderIds)
                {
                    await RecalculateFolderSizeAsync(folderId);
                }

                // Dung lượng chỉ giảm khi xóa vĩnh viễn khỏi thùng rác, không giảm khi chuyển vào thùng rác

                _logger.LogInformation("Bulk deleted [{Count}] files by [{By}].", files.Count, _accessor.GetCurrentUserId());
                return Response.Success(new { deleted = files.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting files.");
                return Response.Fail($"Failed to delete files: {ex.Message}");
            }
        }

        #region Private methods

        private static readonly Dictionary<string, string> TagDisplayNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["taskflow-received"] = "Taskflow Received Files",
            ["hrm-other"] = "Other files"
        };

        private async Task<Guid?> GetOrCreateTaggedFolderAsync(Guid userId, string tag, string? displayName = null)
        {
            var finalDisplayName = displayName
                ?? (TagDisplayNames.TryGetValue(tag, out var dn) ? dn : tag);

            var existing = await _repository.GetQueryable<CloudFolder>()
                .Where(f => f.OwnerId == userId && f.Tag == tag && !f.IsDeleted)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                if (existing.Name != finalDisplayName)
                {
                    existing.Name = finalDisplayName;
                    existing.Keyword = Helper.StringHelper.BuildKeyword(finalDisplayName);
                    await _repository.UpdateAsync(existing, saveChanges: true);
                }
                return existing.Id;
            }

            var folder = new CloudFolder
            {
                Name = finalDisplayName,
                Keyword = Helper.StringHelper.BuildKeyword(finalDisplayName),
                ParentId = null,
                OwnerId = userId,
                SizeInBytes = 0,
                Tag = tag
            };
            var result = await _repository.AddAsync(folder);
            _logger.LogInformation("Auto-created tagged folder [{Tag}] for user [{UserId}].", tag, userId);
            return result.Id;
        }

        public async Task<Response> MoveToTaggedFolderAsync(Guid id, string tag, string displayName)
        {
            if (id == Guid.Empty) return Response.Fail("Id is invalid.");
            if (string.IsNullOrWhiteSpace(tag)) return Response.Fail("Tag is required.");

            try
            {
                var file = await _repository.FindAsync<CloudFile>(f => f.Id == id && !f.IsDeleted);
                if (file == null) return Response.Fail("File not found.");

                var folderId = await GetOrCreateTaggedFolderAsync(file.OwnerId, tag, displayName);

                var oldFolderId = file.FolderId;
                file.FolderId = folderId;
                await _repository.UpdateAsync(file, saveChanges: true);

                await RecalculateFolderSizeAsync(oldFolderId);
                await RecalculateFolderSizeAsync(folderId);

                _logger.LogInformation("Moved file [{Id}] to tagged folder [{Tag}].", id, tag);
                return Response.Success(new { file.Id, file.FolderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving file [{Id}] to tagged folder [{Tag}].", id, tag);
                return Response.Fail($"Failed to move file: {ex.Message}");
            }
        }

        private async Task RecalculateFolderSizeAsync(Guid? folderId)
        {
            if (!folderId.HasValue) return;

            var folder = await _repository.FindAsync<CloudFolder>(
                f => f.Id == folderId.Value && !f.IsDeleted);
            if (folder == null) return;

            var fileSize = await _repository.GetQueryable<CloudFile>()
                .Where(f => f.FolderId == folderId && !f.IsDeleted)
                .SumAsync(f => f.SizeInBytes);

            var childFolderSize = await _repository.GetQueryable<CloudFolder>()
                .Where(f => f.ParentId == folderId && !f.IsDeleted)
                .SumAsync(f => f.SizeInBytes);

            folder.SizeInBytes = fileSize + childFolderSize;
            await _repository.UpdateAsync(folder, saveChanges: true);

            // Propagate up
            await RecalculateFolderSizeAsync(folder.ParentId);
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F1} GB";
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F0} MB";
            if (bytes >= 1_024) return $"{bytes / 1_024.0:F0} KB";
            return $"{bytes} B";
        }

        private async Task UpdateUserStorageAsync(Guid userId, long deltaBytes)
        {
            var storage = await _repository.FindAsync<CloudUserStorage>(
                s => s.UserId == userId && !s.IsDeleted);

            if (storage == null)
            {
                // Create storage record if not exists
                storage = new CloudUserStorage
                {
                    UserId = userId,
                    UsedBytes = Math.Max(0, deltaBytes),
                    MaxBytes = 5_368_709_120 // 5 GB default
                };
                await _repository.AddAsync(storage);
            }
            else
            {
                storage.UsedBytes = Math.Max(0, storage.UsedBytes + deltaBytes);
                await _repository.UpdateAsync(storage, saveChanges: true);
            }
        }

        #endregion
    }
}
