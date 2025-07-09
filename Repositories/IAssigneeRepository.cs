using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public interface IAssigneeRepository : IRepository<Assignee>
    {
        Task<List<Assignee>> GetAssigneesForTaskAsync(int taskId, int userId, bool isAdmin);
        Task<List<TodoTask>> GetTasksForUserAsync(int userId, bool isAdmin);
        Task<bool> AssignUsersToTaskAsync(int taskId, List<int> userIds);
        Task<bool> UnassignUserFromTaskAsync(int taskId, int userId);
        Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId);
    }
} 