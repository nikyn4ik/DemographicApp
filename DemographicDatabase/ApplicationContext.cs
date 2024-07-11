using Microsoft.EntityFrameworkCore;
using DemographicDatabase.Models;

namespace DemographicDatabase
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Region> Regions { get; set; }
        public DbSet<DemographicData> DemographicData { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<VerificationRequest> VerificationRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=DemographicApp;Trusted_Connection=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация для сущности Region
            modelBuilder.Entity<Region>()
                .Property(r => r.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Region>()
                .HasMany(r => r.ChildRegions)
                .WithOne(r => r.ParentRegion)
                .HasForeignKey(r => r.ParentRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация для сущности DemographicData
            modelBuilder.Entity<DemographicData>()
                .Property(d => d.Date)
                .HasColumnType("date");

            // Конфигурация для сущности User
            modelBuilder.Entity<User>()
                .HasMany(u => u.AuditLogs)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

            // Конфигурация для сущности VerificationRequest
            modelBuilder.Entity<VerificationRequest>()
                .HasOne(v => v.DemographicData)
                .WithOne(d => d.VerificationRequest)
                .HasForeignKey<VerificationRequest>(v => v.DemographicDataId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
