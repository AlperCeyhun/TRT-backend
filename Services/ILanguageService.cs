using TRT_backend.Models;
using TRT_backend.Models.DTO;

namespace TRT_backend.Services
{
    public interface ILanguageService
    {
        Task<IEnumerable<LanguageDto>> GetAllLanguagesAsync();
        Task<ClaimLanguageDto?> GetClaimLanguageByClaimIdAndLanguageIdAsync(int claimId, int languageId);
    }
} 