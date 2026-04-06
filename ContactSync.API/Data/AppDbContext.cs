using Microsoft.EntityFrameworkCore;
using ContactSync.API.Models;

namespace ContactSync.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Company).HasMaxLength(100);
                entity.Property(e => e.SyncStatus).HasConversion<string>();
            });

            // Seed some initial data so the app isn't empty on first run
            modelBuilder.Entity<Contact>().HasData(
                new Contact { Id = 1, FirstName = "Sarah", LastName = "Chen", Email = "sarah.chen@acmecorp.com", Phone = "780-555-0101", Company = "Acme Corp", SyncStatus = SyncStatus.Synced, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Contact { Id = 2, FirstName = "James", LastName = "Okafor", Email = "j.okafor@northbank.com", Phone = "780-555-0182", Company = "North Bank", SyncStatus = SyncStatus.Pending, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Contact { Id = 3, FirstName = "Maria", LastName = "Reyes", Email = "maria.r@fingroup.ca", Phone = "780-555-0143", Company = "Fin Group", SyncStatus = SyncStatus.Failed, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}