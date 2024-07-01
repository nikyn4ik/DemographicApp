using DemographicApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Services
{
    public class DemographicDataService : IDemographicDataService
    {
        private readonly ApplicationContext _context;

        public DemographicDataService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<DemographicData>> GetDemographicDataAsync()
        {
            return await _context.DemographicData.ToListAsync();
        }

        public async Task<DemographicData> GetDemographicDataByIdAsync(int id)
        {
            return await _context.DemographicData.FindAsync(id);
        }

        public async Task AddDemographicDataAsync(DemographicData demographicData)
        {
            _context.DemographicData.Add(demographicData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDemographicDataAsync(DemographicData demographicData)
        {
            _context.DemographicData.Update(demographicData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDemographicDataAsync(int id)
        {
            var demographicData = await _context.DemographicData.FindAsync(id);
            if (demographicData != null)
            {
                _context.DemographicData.Remove(demographicData);
                await _context.SaveChangesAsync();
            }
        }
    }
}
