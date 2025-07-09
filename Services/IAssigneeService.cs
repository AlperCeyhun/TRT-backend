using TRT_backend.Models;
using System.Security.Claims;

namespace TRT_backend.Services
{
    public interface IAssigneeService
    {
        Task<bool> AssignUsersToTaskAsync(int taskId, List<int> userIds);
        Task<List<Assignee>> GetAssigneesForTaskAsync(ClaimsPrincipal user, int taskId);
        Task<List<TodoTask>> GetTasksForUserAsync(ClaimsPrincipal user);
        Task<bool> UnassignUserFromTaskAsync(int taskId, int userId);
        Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId);
    }
} 