using TRT_backend.Models;
using System.Security.Claims;

namespace TRT_backend.Services
{
    public interface ITaskService
    {
        Task<List<TodoTask>> GetAllTasksAsync();
        Task<TodoTask> GetTaskByIdAsync(int id);
        Task<TodoTask> CreateTaskAsync(TodoTask task);
        Task<TodoTask> UpdateTaskAsync(int id, TodoTask task);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> CanUserEditTaskAsync(int userId, int taskId, string operation);
        Task<bool> CanUserDeleteTaskAsync(int userId, int taskId);
        Task<List<TodoTask>> GetTasksForUserAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 10);
        Task<int> GetTotalTaskCountAsync(ClaimsPrincipal user);
        Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId);
        Task AssignUserToTaskAsync(int taskId, int userId);
    }
} 