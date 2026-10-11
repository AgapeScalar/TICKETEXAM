using EXAM.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Ticket>().Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
