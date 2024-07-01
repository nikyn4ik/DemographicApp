using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Models
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
    }
}