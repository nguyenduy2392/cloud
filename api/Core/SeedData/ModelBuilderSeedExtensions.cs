using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.SeedData
{
    public static class ModelBuilderSeedExtensions
    {
        public static void ApplySeedData(this ModelBuilder modelBuilder)
        {
            // Cloud không seed user mặc định — user được đồng bộ từ HRM qua sync-user
        }
    }
}
