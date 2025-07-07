using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public interface ITaskRepository : IRepository<TodoTask>
    {
        Task<List<TodoTask>> GetTasksWithAssigneesAsync();
        Task<TodoTask> GetTaskWithAssigneesAsync(int id);
        Task<List<TodoTask>> GetTasksForUserAsync(int userId, bool isAdmin, int pageNumber = 1, int pageSize = 10);
        Task<int> GetTotalTaskCountForUserAsync(int userId, bool isAdmin);
        Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId);
        Task AssignUserToTaskAsync(int taskId, int userId);
        Task UnassignUserFromTaskAsync(int taskId, int userId);
        Task<bool> IsUserAdminAsync(int userId);
        Task<bool> UserHasClaimAsync(int userId, string claimName);
    }
} 