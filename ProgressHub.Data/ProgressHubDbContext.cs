using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProgressHub.Core.Models;
namespace ProgressHub.Data
{

    public class ProgressHubDbContextFactory : IDesignTimeDbContextFactory<ProgressHubDbContext>
    {
        public ProgressHubDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProgressHubDbContext>();
            optionsBuilder.UseSqlite("Data Source=progresshub.db");

            return new ProgressHubDbContext(optionsBuilder.Options);
        }
    }

    public class ProgressHubDbContext : DbContext
    {

        public DbSet<User> Users => Set<User>(); // entity User
        public DbSet<DailyLog> DailyLog => Set<DailyLog>(); // entity Dayily Log


        public ProgressHubDbContext(DbContextOptions<ProgressHubDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyLog>()
                .HasOne(d => d.User)
                .WithMany(u => u.DailyLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
