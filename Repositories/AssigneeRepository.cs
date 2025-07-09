using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public class AssigneeRepository : Repository<Assignee>, IAssigneeRepository
    {
        public AssigneeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Assignee>> GetAssigneesForTaskAsync(int taskId, int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return await _context.Assignees
                    .Include(a => a.User)
                    .Where(a => a.TaskId == taskId)
                    .ToListAsync();
            }
            else
            {
                return await _context.Assignees
                    .Include(a => a.User)
                    .Where(a => a.TaskId == taskId && a.UserId == userId)
                    .ToListAsync();
            }
        }

        public async Task<List<TodoTask>> GetTasksForUserAsync(int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return await _context.Tasks
                    .Include(t => t.Assignees)
                        .ThenInclude(a => a.User)
                    .ToListAsync();
            }
            else
            {
                return await _context.Assignees
                    .Include(a => a.Task)
                        .ThenInclude(t => t.Assignees)
                            .ThenInclude(ass => ass.User)
                    .Where(a => a.UserId == userId)
                    .Select(a => a.Task)
                    .ToListAsync();
            }
        }

        public async Task<bool> AssignUsersToTaskAsync(int taskId, List<int> userIds)
        {
            // Task ve user'ların varlığını tek sorguda kontrol et
            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
            if (!taskExists)
                return false;

            var existingUserIds = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            if (existingUserIds.Count != userIds.Count)
                return false;

            // Mevcut assignee'leri kontrol et ve yeni olanları ekle
            var existingAssignees = await _context.Assignees
                .Where(a => a.TaskId == taskId && userIds.Contains(a.UserId))
                .Select(a => a.UserId)
                .ToListAsync();

            var newUserIds = userIds.Except(existingAssignees).ToList();

            foreach (var userId in newUserIds)
            {
                var assignee = new Assignee
                {
                    TaskId = taskId,
                    UserId = userId
                };
                _context.Assignees.Add(assignee);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnassignUserFromTaskAsync(int taskId, int userId)
        {
            var assignee = await _context.Assignees
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.UserId == userId);

            if (assignee == null)
                return false;

            _context.Assignees.Remove(assignee);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId)
        {
            return await _context.Assignees
                .AnyAsync(a => a.TaskId == taskId && a.UserId == userId);
        }
    }
}
