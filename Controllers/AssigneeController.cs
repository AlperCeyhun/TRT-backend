using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using System.Linq;
using System.Collections.Generic;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/assignees")]
    public class AssigneeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssigneeController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpPost("assign")]
        public IActionResult AssignUsersToTask(int taskId, [FromBody] List<int> userIds)
        {
            var task = _context.Tasks.Find(taskId);
            if (task == null)
                return NotFound("Task bulunamadı.");

            foreach (var userId in userIds)
            {
                if (!_context.Users.Any(u => u.Id == userId))
                    return NotFound($"Kullanıcı bulunamadı: {userId}");

               
                if (!_context.Assignees.Any(a => a.TaskId == taskId && a.UserId == userId))
                {
                    var assignee = new Assignee
                    {
                        TaskId = taskId,
                        UserId = userId
                    };
                    _context.Assignees.Add(assignee);
                }
            }
            _context.SaveChanges();
            return Ok("Kullanıcı(lar) başarıyla atandı.");
        }

        [HttpGet("task/{taskId}")]
        public IActionResult GetAssigneesForTask(int taskId)
        {
            var assignees = _context.Assignees
                .Where(a => a.TaskId == taskId)
                .Select(a => new { a.UserId, a.User.username })
                .ToList();

            return Ok(assignees);
        }

        
        [HttpGet("user/{userId}")]
        public IActionResult GetTasksForUser(int userId)
        {
            var tasks = _context.Assignees
                .Where(a => a.UserId == userId)
                .Select(a => new { a.TaskId, a.Task.Title, a.Task.Description })
                .ToList();

            return Ok(tasks);
        }

       
        [HttpDelete("{assigneeId}")]
        public IActionResult DeleteAssignee(int assigneeId)
        {
            var assignee = _context.Assignees.Find(assigneeId);
            if (assignee == null)
                return NotFound("Assignee bulunamadı.");

            _context.Assignees.Remove(assignee);
            _context.SaveChanges();
            return Ok("Assignee silindi.");
        }
    }
} 