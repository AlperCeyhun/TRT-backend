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

        private bool HasClaim(string claimName)
        {
            return User.Claims.Any(c => c.Type == "permission" && c.Value == claimName);
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
            if (!HasClaim("Add Task"))
                return StatusCode(403, "You dont have permission to add task.");

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
        public async Task<IActionResult> GetAll()
        {
            int userId = GetUserIdFromToken();
            
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            var query = _context.Tasks
                .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
                .AsQueryable();
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
            if (!HasClaim("Delete Task"))
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
            if (!HasClaim("Edit Task"))
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