using DemographicApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationContext _context;

        public ReportService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<Report>> GetReportsAsync()
        {
            return await _context.Reports.ToListAsync();
        }

        public async Task<Report> GetReportByIdAsync(int id)
        {
            return await _context.Reports.FindAsync(id);
        }

        public async Task AddReportAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReportAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }
    }
}