using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;
using TRT_backend.Repositories;
using System.Security.Claims;

namespace TRT_backend.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly AppDbContext _context;
        private readonly ICacheService _cache;

        public TaskService(ITaskRepository taskRepository, AppDbContext context, ICacheService cache)
        {
            _taskRepository = taskRepository;
            _context = context;
            _cache = cache;
        }

        public async Task<List<TodoTask>> GetAllTasksAsync()
        {
            var cacheKey = "all_tasks";
            var cachedTasks = _cache.Get<List<TodoTask>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = await _taskRepository.GetTasksWithAssigneesAsync();
            _cache.Set(cacheKey, tasks, TimeSpan.FromMinutes(15));
            return tasks;
        }

        public async Task<TodoTask> GetTaskByIdAsync(int id)
        {
            var cacheKey = $"task_{id}";
            var cachedTask = _cache.Get<TodoTask>(cacheKey);
            
            if (cachedTask != null)
                return cachedTask;

            var task = await _taskRepository.GetTaskWithAssigneesAsync(id);
            if (task != null)
                _cache.Set(cacheKey, task, TimeSpan.FromMinutes(15));
            
            return task;
        }

        public async Task<TodoTask> CreateTaskAsync(TodoTask task)
        {
            var createdTask = await _taskRepository.AddAsync(task);
            
            // Cache'leri temizle
            _cache.Remove("all_tasks");
            _cache.Remove("user_tasks");
            
            return createdTask;
        }

        public async Task<TodoTask> UpdateTaskAsync(int id, TodoTask task)
        {
            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
                throw new Exception("Task not found");

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Completed = task.Completed;

            await _taskRepository.UpdateAsync(existingTask);
            
            // Cache'leri temizle
            _cache.Remove($"task_{id}");
            _cache.Remove("all_tasks");
            _cache.Remove("user_tasks");
            
            return existingTask;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return false;

            await _taskRepository.DeleteAsync(id);
            
            // Cache'leri temizle
            _cache.Remove($"task_{id}");
            _cache.Remove("all_tasks");
            _cache.Remove("user_tasks");
            
            return true;
        }

        public async Task<bool> CanUserEditTaskAsync(int userId, int taskId, string operation)
        {
            // Admin her şeyi yapabilir
            var isAdmin = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            
            if (isAdmin)
                return true;

            // Kullanıcı task'a atanmış mı kontrol et
            var isAssigned = await _taskRepository.IsUserAssignedToTaskAsync(userId, taskId);

            if (!isAssigned)
                return false;

            // Operation'a göre claim kontrolü
            var hasClaim = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => _context.RoleClaims
                    .Where(rc => rc.RoleId == ur.RoleId)
                    .Select(rc => rc.Claim.ClaimName))
                .Concat(_context.UserClaims
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => uc.Claim.ClaimName))
                .Distinct()
                .ContainsAsync(operation);

            return hasClaim;
        }

        public async Task<bool> CanUserDeleteTaskAsync(int userId, int taskId)
        {
            // Admin her şeyi silebilir
            var isAdmin = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            
            if (isAdmin)
                return true;

            // Kullanıcı task'a atanmış mı kontrol et
            var isAssigned = await _taskRepository.IsUserAssignedToTaskAsync(userId, taskId);

            if (!isAssigned)
                return false;

            // Delete Task claim'i var mı kontrol et
            var hasDeleteClaim = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => _context.RoleClaims
                    .Where(rc => rc.RoleId == ur.RoleId)
                    .Select(rc => rc.Claim.ClaimName))
                .Concat(_context.UserClaims
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => uc.Claim.ClaimName))
                .Distinct()
                .ContainsAsync("Delete Task");

            return hasDeleteClaim;
        }

        public async Task<List<TodoTask>> GetTasksForUserAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 10)
        {
            var userId = int.Parse(user.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = bool.Parse(user.FindFirst("IsAdmin")?.Value ?? "false");
            
            var cacheKey = $"user_tasks_{userId}_{pageNumber}_{pageSize}";
            var cachedTasks = _cache.Get<List<TodoTask>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = await _taskRepository.GetTasksForUserAsync(userId, isAdmin, pageNumber, pageSize);
            _cache.Set(cacheKey, tasks, TimeSpan.FromMinutes(10));
            return tasks;
        }

        public async Task<int> GetTotalTaskCountAsync(ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = bool.Parse(user.FindFirst("IsAdmin")?.Value ?? "false");
            
            var cacheKey = $"user_task_count_{userId}";
            var cachedCount = _cache.Get<int?>(cacheKey);
            
            if (cachedCount.HasValue)
                return cachedCount.Value;

            var count = await _taskRepository.GetTotalTaskCountForUserAsync(userId, isAdmin);
            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(10));
            return count;
        }

        public async Task<bool> IsUserAssignedToTaskAsync(int userId, int taskId)
        {
            var cacheKey = $"user_assigned_task_{userId}_{taskId}";
            var cachedResult = _cache.Get<bool?>(cacheKey);
            
            if (cachedResult.HasValue)
                return cachedResult.Value;

            var isAssigned = await _taskRepository.IsUserAssignedToTaskAsync(userId, taskId);
            _cache.Set(cacheKey, isAssigned, TimeSpan.FromMinutes(10));
            return isAssigned;
        }

        public async Task AssignUserToTaskAsync(int taskId, int userId)
        {
            var assignee = new Assignee
            {
                TaskId = taskId,
                UserId = userId
            };
            
            _context.Assignees.Add(assignee);
            await _context.SaveChangesAsync();
            
            // Cache'leri temizle
            _cache.Remove($"user_assigned_task_{userId}_{taskId}");
            _cache.Remove($"user_tasks_{userId}");
            _cache.Remove("all_tasks");
            _cache.Remove("user_tasks");
        }
    }
} 