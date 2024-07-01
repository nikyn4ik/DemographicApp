using DemographicApp.Models;

namespace DemographicApp.Services
{
    public interface IDemographicDataService
    {
        Task<List<DemographicData>> GetDemographicDataAsync();
        Task<DemographicData> GetDemographicDataByIdAsync(int id);
        Task AddDemographicDataAsync(DemographicData demographicData);
        Task UpdateDemographicDataAsync(DemographicData demographicData);
        Task DeleteDemographicDataAsync(int id);
    }
}