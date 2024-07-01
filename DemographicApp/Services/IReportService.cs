using DemographicApp.Models;

namespace DemographicApp.Services
{
    public interface IReportService
    {
        Task<List<Report>> GetReportsAsync();
        Task<Report> GetReportByIdAsync(int id);
        Task AddReportAsync(Report report);
        Task UpdateReportAsync(Report report);
        Task DeleteReportAsync(int id);
    }
}
