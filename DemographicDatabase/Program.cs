using Microsoft.EntityFrameworkCore;

namespace DemographicDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Applying migrations...");

            using (var context = new ApplicationContext())
            {
                context.Database.Migrate();
            }

            Console.WriteLine("Migrations applied successfully.");
        }
    }
}
