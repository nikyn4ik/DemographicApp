using DemographicApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly ApplicationContext _context;

        public VerificationService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<VerificationRequest>> GetVerificationRequestsAsync()
        {
            return await _context.VerificationRequests.ToListAsync();
        }

        public async Task<VerificationRequest> GetVerificationRequestByIdAsync(int id)
        {
            return await _context.VerificationRequests.FindAsync(id);
        }

        public async Task AddVerificationRequestAsync(VerificationRequest request)
        {
            _context.VerificationRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVerificationRequestAsync(VerificationRequest request)
        {
            _context.VerificationRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVerificationRequestAsync(int id)
        {
            var request = await _context.VerificationRequests.FindAsync(id);
            if (request != null)
            {
                _context.VerificationRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }
    }
}