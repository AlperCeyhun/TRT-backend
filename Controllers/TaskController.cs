using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/todo-tasks")]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        private bool HasClaim(int userId, string claimName)
        {
            var roleClaimIds = _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => _context.RoleClaims.Where(rc => rc.RoleId == ur.RoleId).Select(rc => rc.ClaimId))
                .ToList();
            var roleClaims = _context.Claims.Where(c => roleClaimIds.Contains(c.Id)).Select(c => c.ClaimName);
            var userClaimNames = _context.UserClaims.Where(uc => uc.UserId == userId).Select(uc => uc.Claim.ClaimName);
            var allClaims = roleClaims.Concat(userClaimNames).Distinct();
            return allClaims.Contains(claimName);
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            int userId = GetUserIdFromToken();
            if (!HasClaim(userId, "Add Task"))
                return StatusCode(403, "Bu işlemi yapmaya yetkiniz yok.");

            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Completed = dto.Completed
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _context.Assignees.Add(new Assignee { TaskId = task.Id, UserId = userId });
            await _context.SaveChangesAsync();

            // Sadece temel alanları içeren bir DTO dön
            var result = new
            {
                task.Id,
                task.Title,
                task.Description,
                task.Category,
                task.Completed
            };

            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetTasks(int pageNumber = 1, int pageSize = 2)
        {
            var pagedTasks = _context.Tasks
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            var totalCount = _context.Tasks.Count();

            return Ok(new
          {
            Data = pagedTasks,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
          });
        }


        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = GetUserIdFromToken();
            if (!HasClaim(userId, "Delete Task"))
                return StatusCode(403, "You are not authorized to perform this operation.");

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !_context.Assignees.Any(a => a.TaskId == id && a.UserId == userId))
                return StatusCode(403, "You can only delete the task to which you are assigned.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto updates)
        {
            int userId = GetUserIdFromToken();
            if (!HasClaim(userId, "Edit Task"))
                return StatusCode(403, "You are not authorized to perform this operation.");

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }
            
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !_context.Assignees.Any(a => a.TaskId == id && a.UserId == userId))
                return StatusCode(403, "You can only update the task you are assigned to.");

            existingTask.Title = updates.Title;
            existingTask.Description = updates.Description;
            existingTask.Category = updates.Category;
            existingTask.Completed = updates.Completed;

            await _context.SaveChangesAsync();
            return Ok(existingTask);
        }

        public class UpdateTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public TaskCategory Category { get; set; }
            public bool Completed { get; set; }
        }

        public class CreateTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public TaskCategory Category { get; set; }
            public bool Completed { get; set; }
        }
    }
} 