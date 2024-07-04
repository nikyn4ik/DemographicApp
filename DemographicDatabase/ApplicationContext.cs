using Microsoft.EntityFrameworkCore;
using DemographicDatabase.Models;

namespace DemographicDatabase
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Models.Region> Regions { get; set; }
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
            modelBuilder.Entity<Models.Region>()
                .Property(r => r.Name)
                .HasMaxLength(100) // Максимальная длина для Name
                .IsRequired(); // Обязательное поле

            modelBuilder.Entity<Models.Region>()
                .HasMany(r => r.ChildRegions) // Один ко многим для дочерних регионов
                .WithOne(r => r.ParentRegion)
                .HasForeignKey(r => r.ParentRegionId)
                .OnDelete(DeleteBehavior.Restrict); // Запрет удаления региона при наличии дочерних регионов

            // Конфигурация для сущности DemographicData
            modelBuilder.Entity<DemographicData>()
                .Property(d => d.Date)
                .HasColumnType("date"); // Тип данных для даты

            // Конфигурация для сущности User
            modelBuilder.Entity<User>()
                .HasMany(u => u.AuditLogs) // Один ко многим для логов действий
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

            // Конфигурация для сущности VerificationRequest
            modelBuilder.Entity<VerificationRequest>()
                .HasOne(v => v.DemographicData) // Один к одному для DemographicData
                .WithOne(d => d.VerificationRequest)
                .HasForeignKey<VerificationRequest>(v => v.DemographicDataId)
                .OnDelete(DeleteBehavior.Restrict); // Запрет удаления запроса на верификацию при наличии связанной DemographicData

            // Дополнительные конфигурации можно добавить по мере необходимости

            base.OnModelCreating(modelBuilder);
        }
    }
}