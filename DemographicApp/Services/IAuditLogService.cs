using DemographicApp.Models;

namespace DemographicApp.Services
{
    public interface IAuditLogService
    {
        Task<List<AuditLog>> GetAuditLogsAsync();
        Task<AuditLog> GetAuditLogByIdAsync(int id);
        Task AddAuditLogAsync(AuditLog auditLog);
        Task UpdateAuditLogAsync(AuditLog auditLog);
        Task DeleteAuditLogAsync(int id);
    }
}
