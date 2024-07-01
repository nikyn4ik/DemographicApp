namespace DemographicApp.Services
{
    public interface IRegionService
    {
        Task<List<Region>> GetRegionsAsync();
        Task<Region> GetRegionByIdAsync(int id);
        Task AddRegionAsync(Region region);
        Task UpdateRegionAsync(Region region);
        Task DeleteRegionAsync(int id);
    }
}
