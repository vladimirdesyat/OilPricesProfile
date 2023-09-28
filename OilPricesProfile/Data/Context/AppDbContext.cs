using Microsoft.EntityFrameworkCore;

namespace OilPricesProfile.Data.Context
{
    public class AppDbContext : DbContext
    {
        // public DbSet<WebPageData> WebPageData { get; set; }
        public DbSet<OilDepot> OilDepots { get; set; }
        public DbSet<PetroleumProduct> PetroleumProducts { get; set; }
        public DbSet<Price> Prices { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OilDepot>()
                .HasKey(o => new { o.Id });

            modelBuilder.Entity<OilDepot>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<PetroleumProduct>()
                .HasKey(p => new { p.Id });

            modelBuilder.Entity<PetroleumProduct>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Price>()
                .HasKey(p => new { p.Id });

            modelBuilder.Entity<Price>()
                .HasOne(p => p.PetroleumProduct)
                .WithMany()
                .HasForeignKey(p => p.PetroleumProductId)
                .IsRequired();

            modelBuilder.Entity<Price>()
                .HasOne(p => p.OilDepot)
                .WithMany()
                .HasForeignKey(p => p.OilDepotId)
                .IsRequired();
        }
    }
}
