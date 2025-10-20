using MathSlidesBe.Entity;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe
{
    public class MathSlidesDbContext : DbContext
    {
        public DbSet<School> Schools { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Presentation> Presentations { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Component> Components { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                IConfiguration configuration = builder.Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.HasCharSet("utf8mb4");
        }
    }
}
