using TRT_backend.Models;
using TRT_backend.Models.DTO;
using TRT_backend.Repositories;

namespace TRT_backend.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly ILanguageRepository _languageRepository;
        private readonly IClaimLanguageRepository _claimLanguageRepository;

        public LanguageService(ILanguageRepository languageRepository, IClaimLanguageRepository claimLanguageRepository)
        {
            _languageRepository = languageRepository;
            _claimLanguageRepository = claimLanguageRepository;
        }

        public async Task<IEnumerable<LanguageDto>> GetAllLanguagesAsync()
        {
            var languages = await _languageRepository.GetAllAsync();
            return languages.Select(l => new LanguageDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name
            });
        }

        public async Task<ClaimLanguageDto?> GetClaimLanguageByClaimIdAndLanguageIdAsync(int claimId, int languageId)
        {
            var cl = await _claimLanguageRepository.GetByClaimIdAndLanguageIdAsync(claimId, languageId);
            if (cl == null) return null;
            return new ClaimLanguageDto
            {
                Id = cl.Id,
                ClaimId = cl.ClaimId,
                LanguageId = cl.LanguageId,
                Name = cl.Name,
                Description = cl.Description,
                ClaimName = cl.Claim.ClaimName,
                LanguageCode = cl.Language.Code,
                LanguageName = cl.Language.Name
            };
        }
    }
} 