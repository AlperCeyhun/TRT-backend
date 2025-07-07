using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public class TaskCategoryRepository : Repository<TaskCategory>, ITaskCategoryRepository
    {
        public TaskCategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskCategory>> GetAllWithTaskCountAsync()
        {
            return await _context.TaskCategories
                .Include(c => c.Tasks)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<TaskCategory?> GetByIdWithTasksAsync(int id)
        {
            return await _context.TaskCategories
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var query = _context.TaskCategories.Where(c => c.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
    }
} 