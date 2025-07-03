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

        [Tags("TaskManagement")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            int userId = GetUserIdFromToken();
            if (!HasClaim(userId, "Add Task"))
                return StatusCode(403, "You dont have permission to add task.");

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest("Category not found.");

            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                Category = category,
                Completed = dto.Completed
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _context.Assignees.Add(new Assignee { TaskId = task.Id, UserId = userId });
            await _context.SaveChangesAsync();

            var result = new
            {
                task.Id,
                task.Title,
                task.Description,
                task.CategoryId,
                CategoryName = category.Name,
                task.Completed
            };

            return Ok(result);
        }

       [Tags("TaskManagement")]
       [HttpGet]
       public IActionResult GetTasks(int pageNumber = 1, int pageSize = 2)
       {
           int userId = GetUserIdFromToken();
           if (userId == 0)
               return Unauthorized("Token is invalid.");
               
           var user = _context.Users
               .Include(u => u.UserRoles)
                   .ThenInclude(ur => ur.Role)
               .FirstOrDefault(u => u.Id == userId);
       
           if (user == null)
               return NotFound("User not found.");
       
           IQueryable<TodoTask> query;
       
           bool isAdmin = user.UserRoles.Any(ur => ur.Role.RoleName == "Admin");
       
           if (isAdmin)
           {
               query = _context.Tasks
                   .Include(t => t.Assignees)
                       .ThenInclude(a => a.User);
           }
           else
           {
               query = _context.Assignees
                   .Where(a => a.UserId == userId)
                   .Include(a => a.Task)
                       .ThenInclude(t => t.Assignees)
                           .ThenInclude(ass => ass.User)
                   .Select(a => a.Task);
           }
       
           var totalCount = query.Count();
       
           var pagedTasks = query
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .Select(t => new
               {
                   t.Id,
                   t.Title,
                   t.Description,
                   t.CategoryId,
                   CategoryName = t.Category.Name,
                   t.Completed,
                   Assignees = t.Assignees.Select(a => new
                   {
                       a.Id,
                       UserId = a.User.Id,
                       Username = a.User.username
                   }).ToList()
               })
               .ToList();
       
           return Ok(new
           {
               Data = pagedTasks,
               TotalCount = totalCount,
               PageNumber = pageNumber,
               PageSize = pageSize
           });
       }

        [Tags("TaskManagement")]
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

        [Tags("TaskManagement")]
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto updates)
        {
            int userId = GetUserIdFromToken();
            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !_context.Assignees.Any(a => a.TaskId == id && a.UserId == userId))
                return StatusCode(403, "You can only update the task you are assigned to.");
            if (!isAdmin) {
                if (updates.Title != null && updates.Title != existingTask.Title && !HasClaim(userId, "Edit Task Title"))
                    return StatusCode(403, "You don't have permission to edit task title.");
                if (updates.Description != null && updates.Description != existingTask.Description && !HasClaim(userId, "Edit Task Description"))
                    return StatusCode(403, "You don't have permission to edit task description.");
                if (updates.Completed != null && updates.Completed != existingTask.Completed && !HasClaim(userId, "Edit Task Status"))
                    return StatusCode(403, "You don't have permission to edit task status.");
                if (updates.CategoryId != null && updates.CategoryId != existingTask.CategoryId && !HasClaim(userId, "Edit Task Assignees"))
                    return StatusCode(403, "You don't have permission to edit task category.");
            }
            if (updates.Title != null && updates.Title != existingTask.Title)
                existingTask.Title = updates.Title;
            if (updates.Description != null && updates.Description != existingTask.Description)
                existingTask.Description = updates.Description;
            if (updates.Completed != null && updates.Completed != existingTask.Completed)
                existingTask.Completed = updates.Completed.Value;
            if (updates.CategoryId != null && updates.CategoryId != existingTask.CategoryId)
            {
                var category = await _context.Categories.FindAsync(updates.CategoryId);
                if (category == null)
                    return BadRequest("Category not found.");
                existingTask.CategoryId = updates.CategoryId.Value;
                existingTask.Category = category;
            }
            await _context.SaveChangesAsync();
            return Ok(existingTask);
        }

        public class UpdateTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int? CategoryId { get; set; }
            public bool? Completed { get; set; }
        }

        public class CreateTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int CategoryId { get; set; }
            public bool Completed { get; set; }
        }
    }
} 