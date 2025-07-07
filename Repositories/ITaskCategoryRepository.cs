using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public interface ITaskCategoryRepository : IRepository<TaskCategory>
    {
        Task<IEnumerable<TaskCategory>> GetAllWithTaskCountAsync();
        Task<TaskCategory?> GetByIdWithTasksAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    }
} 