using Core.Entities;
using Core.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Core
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplySeedData();
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<AppAudit> AppAudits { get; set; }
        public DbSet<CloudFolder> CloudFolders { get; set; }
        public DbSet<CloudFile> CloudFiles { get; set; }
        public DbSet<CloudResourcePermission> CloudResourcePermissions { get; set; }
        public DbSet<CloudUserStorage> CloudUserStorages { get; set; }
    }
}
