using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public interface IClaimLanguageRepository : IRepository<ClaimLanguage>
    {
        Task<IEnumerable<ClaimLanguage>> GetByLanguageCodeAsync(string languageCode);
        Task<IEnumerable<ClaimLanguage>> GetByClaimIdAsync(int claimId);
        Task<ClaimLanguage?> GetByClaimAndLanguageAsync(int claimId, string languageCode);
        Task<ClaimLanguage?> GetByClaimIdAndLanguageIdAsync(int claimId, int languageId);
    }
} 