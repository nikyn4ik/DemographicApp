using DemographicApp.Models;

namespace DemographicApp.Services
{
    public interface IVerificationService
    {
        Task<List<VerificationRequest>> GetVerificationRequestsAsync();
        Task<VerificationRequest> GetVerificationRequestByIdAsync(int id);
        Task AddVerificationRequestAsync(VerificationRequest request);
        Task UpdateVerificationRequestAsync(VerificationRequest request);
        Task DeleteVerificationRequestAsync(int id);
    }
}
