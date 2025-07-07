using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public class TaskRepository : Repository<TodoTask>, ITaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<TodoTask>> GetTasksWithAssigneesAsync()
        {
            return await _context.Tasks
                .Include(t => t.Assignees)
                    .ThenInclude(a => a.User)
                .Include(t => t.Category)
                .ToListAsync();
        }

        public async Task<TodoTask> GetTaskWithAssigneesAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Assignees)
                    .ThenInclude(a => a.User)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TodoTask>> GetTasksForUserAsync(int userId, bool isAdmin, int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<TodoTask> query;

            if (isAdmin)
            {
                query = _context.Tasks
                    .Include(t => t.Assignees)
                        .ThenInclude(a => a.User)
                    .Include(t => t.Category);
            }
            else
            {
                query = _context.Assignees
                    .Where(a => a.UserId == userId)
                    .Include(a => a.Task)
                        .ThenInclude(t => t.Assignees)
                            .ThenInclude(ass => ass.User)
                    .Include(a => a.Task)
                        .ThenInclude(t => t.Category)
                    .Select(a => a.Task);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalTaskCountForUserAsync(int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return await _context.Tasks.CountAsync();
            }
            else
            {
                return await _context.Assignees
                    .Where(a => a.UserId == userId)
                    .CountAsync();
            }
        }

        public async Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId)
        {
            return await _context.Assignees
                .AnyAsync(a => a.TaskId == taskId && a.UserId == userId);
        }

        public async Task AssignUserToTaskAsync(int taskId, int userId)
        {
            if (!await _context.Assignees.AnyAsync(a => a.TaskId == taskId && a.UserId == userId))
            {
                var assignee = new Assignee
                {
                    TaskId = taskId,
                    UserId = userId
                };
                _context.Assignees.Add(assignee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnassignUserFromTaskAsync(int taskId, int userId)
        {
            var assignee = await _context.Assignees
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.UserId == userId);
            
            if (assignee != null)
            {
                _context.Assignees.Remove(assignee);
                await _context.SaveChangesAsync();
            }
        }
    }
} 