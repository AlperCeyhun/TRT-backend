using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public interface ILanguageRepository : IRepository<Language>
    {
        Task<Language?> GetByCodeAsync(string code);
        Task<IEnumerable<Language>> GetAllAsync();
    }
} 