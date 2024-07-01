using DemographicApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationContext _context;

        public AuditLogService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync()
        {
            return await _context.AuditLogs.ToListAsync();
        }

        public async Task<AuditLog> GetAuditLogByIdAsync(int id)
        {
            return await _context.AuditLogs.FindAsync(id);
        }

        public async Task AddAuditLogAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAuditLogAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Update(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAuditLogAsync(int id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog != null)
            {
                _context.AuditLogs.Remove(auditLog);
                await _context.SaveChangesAsync();
            }
        }
    }
}
