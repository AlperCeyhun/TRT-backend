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
                return Forbid("You do not have permission to perform this action.");

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

            return Ok(task);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int userId = GetUserIdFromToken();
            
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            var query = _context.Tasks
                .Include(t => t.Assignees)
                .ThenInclude(a => a.User);
            if (!isAdmin)
            {
                
                query = query.Where(t => t.Assignees.Any(a => a.UserId == userId));
            }
            var tasks = await query
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Category,
                    t.Completed,
                    Assignees = t.Assignees.Select(a => new {
                        a.UserId,
                        a.User.username
                    }).ToList()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = GetUserIdFromToken();
            if (!HasClaim(userId, "Delete Task"))
                return Forbid("You do not have permission to perform this action.");

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !_context.Assignees.Any(a => a.TaskId == id && a.UserId == userId))
                return Forbid("You can only delete the task to which you are assigned.");

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
                return Forbid("You do not have permission to perform this action.");

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }
            
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !_context.Assignees.Any(a => a.TaskId == id && a.UserId == userId))
                return Forbid("You can only update the task to which you are assigned.");

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