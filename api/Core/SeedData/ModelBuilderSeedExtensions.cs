using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.SeedData
{
    public static class ModelBuilderSeedExtensions
    {
        public static void ApplySeedData(this ModelBuilder modelBuilder)
        {
            var seededAt = new DateTime(2026, 03, 31, 0, 0, 0, DateTimeKind.Utc);
            var adminId = new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d");

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = adminId,
                    UserName = "admin",
                    Name = "System Administrator",
                    Password = "cs80krLdOzJuOkLfDeo4h6uG6CS7dErnbCkhpuBFNkk=",
                    Email = "admin@cloud.local",
                    Phone = "0000000000",
                    Address = "Headquarters",
                    Description = "Seeded root administrator account",
                    Avatar = string.Empty,
                    Birthday = null,
                    Gender = 0,
                    LastLogin = null,
                    IsRootAdmin = true,
                    CreatedAt = seededAt,
                    ModifiedAt = seededAt,
                    CreatedBy = null,
                    ModifiedBy = null,
                    IsDeleted = false
                }
            );
        }
    }
}
