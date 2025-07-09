using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public class LanguageRepository : Repository<Language>, ILanguageRepository
    {
        public LanguageRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Language?> GetByCodeAsync(string code)
        {
            return await _context.Languages
                .FirstOrDefaultAsync(l => l.Code.ToLower() == code.ToLower());
        }

        public async Task<IEnumerable<Language>> GetAllAsync()
        {
            return await _context.Languages
                .OrderBy(l => l.Name)
                .ToListAsync();
        }
    }
} 