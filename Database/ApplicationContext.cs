using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Database.Models;
using System.Threading.Tasks;

namespace Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Region> Regions { get; set; }
        public DbSet<DemographicData> DemographicData { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Role> Roles { get; set; }

        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=DemographicApp;Trusted_Connection=True;";
            optionsBuilder.UseSqlServer(connectionString);
            optionsBuilder.UseLoggerFactory(MyLoggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Region>()
                .Property(r => r.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<DemographicData>()
                .Property(d => d.Date)
                .HasColumnType("date");

            base.OnModelCreating(modelBuilder);
        }

        public async Task InitializeRolesAsync()
        {
            if (!await Roles.AnyAsync())
            {
                Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" }
                );
                await SaveChangesAsync();
            }
        }

        public async Task<bool> IsAdminUserExists()
        {
            var adminRole = await Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                return false;
            }

            return await Users.AnyAsync(u => u.RoleId == adminRole.Id);
        }
    }
}
