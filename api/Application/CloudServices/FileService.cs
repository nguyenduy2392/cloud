using Application.CloudServices.Dtos;
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
        Task<Response> UploadAsync(IFormFile file, Guid? folderId);
        Task<(Stream? Stream, string FileName, string ContentType)> DownloadAsync(Guid id);
        Task<Response> RenameAsync(Guid id, string newName);
        Task<Response> MoveAsync(Guid id, Guid? newFolderId);
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

        public async Task<Response> UploadAsync(IFormFile file, Guid? folderId)
        {
            if (file == null || file.Length == 0)
                return Response.Fail("Please select a file to upload.");

            try
            {
                var currentUserId = _accessor.GetCurrentUserId();
                var dbName = _accessor.GetDatabaseName()?.ToLower() ?? "default";

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

                // Create CloudFile record
                var cloudFile = new CloudFile
                {
                    Id = fileId,
                    Name = file.FileName,
                    Keyword = Helper.StringHelper.BuildKeyword(file.FileName),
                    StoredFileName = storedFileName,
                    FilePath = $"{dbName}/Files/{storedFileName}",
                    FolderId = folderId,
                    SizeInBytes = file.Length,
                    ContentType = contentType,
                    Extension = extension,
                    OwnerId = currentUserId!.Value
                };

                await _repository.AddAsync(cloudFile);

                // Update folder size up the chain
                await RecalculateFolderSizeAsync(folderId);

                // Update user storage
                await UpdateUserStorageAsync(currentUserId!.Value, file.Length);

                _logger.LogInformation("Uploaded file [{Name}] ({Size} bytes) by [{By}].",
                    file.FileName, file.Length, currentUserId);

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

                await _repository.DeleteSoftAsync(file);

                // Recalculate folder size
                await RecalculateFolderSizeAsync(folderId);

                // Decrement user storage
                await UpdateUserStorageAsync(ownerId, -fileSize);

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

                foreach (var file in files)
                {
                    file.IsDeleted = true;
                    file.ModifiedAt = DateTime.Now;
                }

                await _repository.UpdateRangeAsync(files, saveChanges: true);

                // Recalculate affected folder sizes
                foreach (var folderId in affectedFolderIds)
                {
                    await RecalculateFolderSizeAsync(folderId);
                }

                // Decrement user storage
                await UpdateUserStorageAsync(ownerId, -totalSize);

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
                    MaxBytes = 1_073_741_824 // 1 GB default
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
