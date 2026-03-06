using Microsoft.EntityFrameworkCore;

namespace BrandService
{
    public class BrandDbContext : DbContext
    {
        public BrandDbContext(DbContextOptions<BrandDbContext> options)
            : base(options) { }

        public DbSet<Brand> Brands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brand>()
                .HasIndex(b => b.Id)
                .IsUnique();
        }
    }
}