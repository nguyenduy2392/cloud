using Application.Helper;
using Core;
using Core.Common;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.CloudServices
{
    public interface IDatabaseService
    {
        Task<Response> RunMigrationAsync(string? identity);
        Task<Response> InitializeDatabaseAsync(string databaseName, string userName, string password);
        Task<Response> BackfillKeywordsAsync(string? identity = null);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly AppDbContext _context;
        private readonly IAppContextAccessor _accessor;
        private readonly ICryptorFactory _cryptorFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(
            AppDbContext context,
            IAppContextAccessor accessor,
            ICryptorFactory cryptorFactory,
            IConfiguration configuration,
            ILogger<DatabaseService> logger)
        {
            _context = context;
            _accessor = accessor;
            _cryptorFactory = cryptorFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Response> RunMigrationAsync(string? identity)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(identity))
                {
                    var connectionString = _accessor.GetConnectionString(identity.Trim());
                    _context.Database.SetConnectionString(connectionString);
                }

                if (!await _context.Database.CanConnectAsync())
                {
                    await _context.Database.MigrateAsync();
                    return Response.Success(new { Message = "Database chưa tồn tại. Đã tạo mới database và áp dụng toàn bộ migration." });
                }

                var pending = (await _context.Database.GetPendingMigrationsAsync()).ToList();
                if (!pending.Any())
                    return Response.Success(new { Message = "Database đã được cập nhật mới nhất, không có migration nào cần chạy." });

                await _context.Database.MigrateAsync();

                return Response.Success(new
                {
                    Message = $"Đã chạy thành công {pending.Count} migration(s).",
                    Applied = pending
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RunMigrationAsync: Lỗi khi chạy migration.");
                return Response.Fail("Chạy migration không thành công.");
            }
        }

        public async Task<Response> InitializeDatabaseAsync(string databaseName, string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) return Response.Fail("Tên database không được để trống.");
            if (string.IsNullOrWhiteSpace(userName)) return Response.Fail("Tên đăng nhập không được để trống.");
            if (string.IsNullOrWhiteSpace(password)) return Response.Fail("Mật khẩu không được để trống.");

            try
            {
                var baseConnection = _configuration.GetConnectionString("DefaultConnection")!;
                var dbFullName = $"Cloud_{databaseName.Trim()}";
                var connectionString = System.Text.RegularExpressions.Regex
                    .Replace(baseConnection, @"Database=[^;]+", $"Database={dbFullName}");

                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlServer(connectionString, o => o.MigrationsAssembly("Core"));

                await using var context = new AppDbContext(optionsBuilder.Options);
                await context.Database.MigrateAsync();

                var admin = await context.Users.FirstOrDefaultAsync(x => x.IsRootAdmin && !x.IsDeleted);
                if (admin != null)
                {
                    admin.UserName = userName.Trim();
                    admin.Password = _cryptorFactory.ToHashPassword(password);
                    await context.SaveChangesAsync();
                }

                return Response.Success(new { Database = dbFullName, DefaultUser = new { UserName = userName.Trim() } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InitializeDatabaseAsync: Lỗi khi khởi tạo database {Database}.", databaseName);
                return Response.Fail("Khởi tạo database không thành công.");
            }
        }

        public async Task<Response> BackfillKeywordsAsync(string? identity = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(identity))
                {
                    var connectionString = _accessor.GetConnectionString(identity.Trim());
                    _context.Database.SetConnectionString(connectionString);
                }

                var files = await _context.Set<CloudFile>()
                    .Where(f => !f.IsDeleted && (f.Keyword == null || f.Keyword == ""))
                    .ToListAsync();
                foreach (var f in files)
                    f.Keyword = StringHelper.BuildKeyword(f.Name);
                if (files.Count > 0) await _context.SaveChangesAsync();

                var folders = await _context.Set<CloudFolder>()
                    .Where(f => !f.IsDeleted && (f.Keyword == null || f.Keyword == ""))
                    .ToListAsync();
                foreach (var f in folders)
                    f.Keyword = StringHelper.BuildKeyword(f.Name);
                if (folders.Count > 0) await _context.SaveChangesAsync();

                return Response.Success(new { files = files.Count, folders = folders.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backfilling keywords.");
                return Response.Fail($"Backfill failed: {ex.Message}");
            }
        }
    }
}
