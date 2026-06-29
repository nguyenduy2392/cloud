using Application.CloudServices.Dtos;
using Core;
using Core.Common;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.CloudServices
{
    public interface IUserStorageService
    {
        Task<Response> GetMyStorageAsync();
        Task<Response> GetUserStorageAsync(Guid userId);
        Task<Response> SetQuotaAsync(Guid userId, long maxBytes);
    }

    public class UserStorageService : IUserStorageService
    {
        private readonly IRepository _repository;
        private readonly IAppContextAccessor _accessor;
        private readonly ILogger<UserStorageService> _logger;

        public UserStorageService(
            IRepository repository,
            IAppContextAccessor accessor,
            ILogger<UserStorageService> logger)
        {
            _repository = repository;
            _accessor = accessor;
            _logger = logger;
        }

        public async Task<Response> GetMyStorageAsync()
        {
            try
            {
                var currentUserId = _accessor.GetCurrentUserId();
                if (!currentUserId.HasValue || currentUserId.Value == Guid.Empty)
                    return Response.Fail("Cannot determine current user.");

                return await GetOrCreateStorageAsync(currentUserId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user storage.");
                return Response.Fail("Failed to load storage info.");
            }
        }

        public async Task<Response> GetUserStorageAsync(Guid userId)
        {
            if (userId == Guid.Empty) return Response.Fail("UserId is invalid.");

            try
            {
                return await GetOrCreateStorageAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user storage [{UserId}].", userId);
                return Response.Fail("Failed to load storage info.");
            }
        }

        public async Task<Response> SetQuotaAsync(Guid userId, long maxBytes)
        {
            if (userId == Guid.Empty) return Response.Fail("UserId is invalid.");
            if (maxBytes <= 0) return Response.Fail("Max bytes must be greater than 0.");

            try
            {
                var storage = await _repository.FindAsync<CloudUserStorage>(
                    s => s.UserId == userId && !s.IsDeleted);

                if (storage == null)
                {
                    storage = new CloudUserStorage
                    {
                        UserId = userId,
                        UsedBytes = 0,
                        MaxBytes = maxBytes
                    };
                    await _repository.AddAsync(storage);
                }
                else
                {
                    storage.MaxBytes = maxBytes;
                    await _repository.UpdateAsync(storage, saveChanges: true);
                }

                _logger.LogInformation("Set storage quota for user [{UserId}] to [{MaxBytes}] bytes by [{By}].",
                    userId, maxBytes, _accessor.GetCurrentUserId());

                return Response.Success(new { userId, maxBytes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting quota for user [{UserId}].", userId);
                return Response.Fail($"Failed to set quota: {ex.Message}");
            }
        }

        #region Private methods

        private async Task<Response> GetOrCreateStorageAsync(Guid userId)
        {
            var storage = await _repository.GetQueryable<CloudUserStorage>()
                .AsNoTracking()
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .Select(s => new UserStorageDto
                {
                    UserId = s.UserId,
                    UserName = s.User != null ? s.User.Name : string.Empty,
                    UsedBytes = s.UsedBytes,
                    MaxBytes = s.MaxBytes
                })
                .FirstOrDefaultAsync();

            if (storage != null)
                return Response.Success(storage);

            // Create default storage record
            var newStorage = new CloudUserStorage
            {
                UserId = userId,
                UsedBytes = 0,
                MaxBytes = 5_368_709_120 // 5 GB default
            };
            await _repository.AddAsync(newStorage);

            // Fetch user name
            var user = await _repository.FindAsync<AppUser>(userId);
            var userName = user?.Name ?? string.Empty;

            return Response.Success(new UserStorageDto
            {
                UserId = userId,
                UserName = userName,
                UsedBytes = 0,
                MaxBytes = newStorage.MaxBytes
            });
        }

        #endregion
    }
}
