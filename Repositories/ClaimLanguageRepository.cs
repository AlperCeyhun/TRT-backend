using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public class ClaimLanguageRepository : Repository<ClaimLanguage>, IClaimLanguageRepository
    {
        public ClaimLanguageRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClaimLanguage>> GetByLanguageCodeAsync(string languageCode)
        {
            return await _context.ClaimLanguages
                .Include(cl => cl.Claim)
                .Include(cl => cl.Language)
                .Where(cl => cl.Language.Code.ToLower() == languageCode.ToLower())
                .OrderBy(cl => cl.Claim.ClaimName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClaimLanguage>> GetByClaimIdAsync(int claimId)
        {
            return await _context.ClaimLanguages
                .Include(cl => cl.Claim)
                .Include(cl => cl.Language)
                .Where(cl => cl.ClaimId == claimId)
                .OrderBy(cl => cl.Language.Name)
                .ToListAsync();
        }

        public async Task<ClaimLanguage?> GetByClaimAndLanguageAsync(int claimId, string languageCode)
        {
            return await _context.ClaimLanguages
                .Include(cl => cl.Claim)
                .Include(cl => cl.Language)
                .FirstOrDefaultAsync(cl => cl.ClaimId == claimId && 
                                          cl.Language.Code.ToLower() == languageCode.ToLower());
        }

        public async Task<ClaimLanguage?> GetByClaimIdAndLanguageIdAsync(int claimId, int languageId)
        {
            return await _context.ClaimLanguages
                .Include(cl => cl.Claim)
                .Include(cl => cl.Language)
                .FirstOrDefaultAsync(cl => cl.ClaimId == claimId && cl.LanguageId == languageId);
        }
    }
} 