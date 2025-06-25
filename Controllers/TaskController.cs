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
       public IActionResult GetTasks(int userId, int pageNumber = 1, int pageSize = 2)
       {
           var user = _context.Users
               .Include(u => u.UserRoles)
                   .ThenInclude(ur => ur.Role)
               .FirstOrDefault(u => u.Id == userId);
       
           if (user == null)
               return NotFound("Kullanıcı bulunamadı");
       
           IQueryable<TodoTask> query;
       
           bool isAdmin = user.UserRoles.Any(ur => ur.Role.RoleName == "Admin");
       
           if (isAdmin)
           {
               query = _context.Tasks.AsQueryable();
           }
           else
           {
               query = _context.Assignees
                   .Where(a => a.UserId == userId)
                   .Include(a => a.Task)
                   .Select(a => a.Task);
           }
       
           var totalCount = query.Count();
       
           var pagedTasks = query
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();
       
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
            
            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }
            
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !_context.Assignees.Any(a => a.TaskId == id && a.UserId == userId))
                return StatusCode(403, "You can only update the task you are assigned to.");

            // Sadece admin olmayanlar için, sadece değişen alanlarda claim kontrolü
            if (!isAdmin) {
                if (updates.Title != null && updates.Title != existingTask.Title && !HasClaim(userId, "Edit Task Title"))
                    return StatusCode(403, "You don't have permission to edit task title.");
                if (updates.Description != null && updates.Description != existingTask.Description && !HasClaim(userId, "Edit Task Description"))
                    return StatusCode(403, "You don't have permission to edit task description.");
                if (updates.Completed != null && updates.Completed != existingTask.Completed && !HasClaim(userId, "Edit Task Status"))
                    return StatusCode(403, "You don't have permission to edit task status.");
                if (updates.Category != null && updates.Category != existingTask.Category && !HasClaim(userId, "Edit Task Assignees"))
                    return StatusCode(403, "You don't have permission to edit task category.");
            }

            // Sadece izin verilen ve değişen alanları güncelle
            if (updates.Title != null && updates.Title != existingTask.Title)
                existingTask.Title = updates.Title;
            if (updates.Description != null && updates.Description != existingTask.Description)
                existingTask.Description = updates.Description;
            if (updates.Completed != null && updates.Completed != existingTask.Completed)
                existingTask.Completed = updates.Completed;
            if (updates.Category != null && updates.Category != existingTask.Category)
                existingTask.Category = updates.Category;

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