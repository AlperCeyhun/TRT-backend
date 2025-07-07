using TRT_backend.Data;
using TRT_backend.Models;
using TRT_backend.Repositories;
using System.Security.Claims;

namespace TRT_backend.Services
{
    public class AssigneeService : IAssigneeService
    {
        private readonly IAssigneeRepository _assigneeRepository;
        private readonly ICacheService _cache;

        public AssigneeService(IAssigneeRepository assigneeRepository, ICacheService cache)
        {
            _assigneeRepository = assigneeRepository;
            _cache = cache;
        }

        public async Task<bool> AssignUsersToTaskAsync(int taskId, List<int> userIds)
        {
            var result = await _assigneeRepository.AssignUsersToTaskAsync(taskId, userIds);
            
            if (result)
            {
                // Cache'leri temizle
                _cache.Remove($"task_assignees_{taskId}");
                _cache.Remove("all_tasks");
                _cache.Remove("user_tasks");
                
                foreach (var userId in userIds)
                {
                    _cache.Remove($"user_assigned_task_{userId}_{taskId}");
                    _cache.Remove($"user_tasks_{userId}");
                }
            }
            
            return result;
        }

        public async Task<List<Assignee>> GetAssigneesForTaskAsync(ClaimsPrincipal user, int taskId)
        {
            var userId = int.Parse(user.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = bool.Parse(user.FindFirst("IsAdmin")?.Value ?? "false");
            
            var cacheKey = $"task_assignees_{taskId}";
            var cachedAssignees = _cache.Get<List<Assignee>>(cacheKey);
            
            if (cachedAssignees != null)
                return cachedAssignees;

            var assignees = await _assigneeRepository.GetAssigneesForTaskAsync(taskId, userId, isAdmin);
            _cache.Set(cacheKey, assignees, TimeSpan.FromMinutes(10));
            return assignees;
        }

        public async Task<List<TodoTask>> GetTasksForUserAsync(ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = bool.Parse(user.FindFirst("IsAdmin")?.Value ?? "false");
            
            var cacheKey = $"user_assigned_tasks_{userId}";
            var cachedTasks = _cache.Get<List<TodoTask>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = await _assigneeRepository.GetTasksForUserAsync(userId, isAdmin);
            _cache.Set(cacheKey, tasks, TimeSpan.FromMinutes(10));
            return tasks;
        }

        public async Task<bool> UnassignUserFromTaskAsync(int taskId, int userId)
        {
            var result = await _assigneeRepository.UnassignUserFromTaskAsync(taskId, userId);
            
            if (result)
            {
                // Cache'leri temizle
                _cache.Remove($"task_assignees_{taskId}");
                _cache.Remove($"user_assigned_task_{userId}_{taskId}");
                _cache.Remove($"user_assigned_tasks_{userId}");
                _cache.Remove("all_tasks");
                _cache.Remove("user_tasks");
            }
            
            return result;
        }

        public async Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId)
        {
            var cacheKey = $"user_assigned_task_{userId}_{taskId}";
            var cachedResult = _cache.Get<bool?>(cacheKey);
            
            if (cachedResult.HasValue)
                return cachedResult.Value;

            var assignees = await GetAssigneesForTaskAsync(null, taskId);
            var isAssigned = assignees.Any(a => a.UserId == userId);
            _cache.Set(cacheKey, isAssigned, TimeSpan.FromMinutes(10));
            return isAssigned;
        }
    }
} 