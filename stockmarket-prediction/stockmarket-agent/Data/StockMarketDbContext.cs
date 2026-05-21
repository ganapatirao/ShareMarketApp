using Microsoft.EntityFrameworkCore;
using stockmarket_agent.Models;

namespace stockmarket_agent.Data
{
    public class StockMarketDbContext : DbContext
    {
        public StockMarketDbContext(DbContextOptions<StockMarketDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Symbol).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Sector).HasMaxLength(50);
                entity.Property(e => e.Industry).HasMaxLength(50);
                entity.Property(e => e.MarketCapCategory).HasMaxLength(20);
            });
        }
    }
}
