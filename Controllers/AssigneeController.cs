using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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

        [Tags("AssigneeManagement")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignUsersToTask(int taskId, [FromBody] List<int> userIds)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            foreach (var userId in userIds)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == userId))
                    return NotFound($"User not found: {userId}");

                if (!await _context.Assignees.AnyAsync(a => a.TaskId == taskId && a.UserId == userId))
                {
                    var assignee = new Assignee
                    {
                        TaskId = taskId,
                        UserId = userId
                    };
                    _context.Assignees.Add(assignee);
                }
            }
            await _context.SaveChangesAsync();
            return Ok("User(s) assigned successfully.");
        }

        [Tags("AssigneeManagement")]
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetAssigneesForTask(int taskId)
        {
            var assignees = await _context.Assignees
                .Where(a => a.TaskId == taskId)
                .Select(a => new { a.UserId, a.User.username })
                .ToListAsync();

            return Ok(assignees);
        }

        [Tags("AssigneeManagement")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksForUser(int userId)
        {
            var tasks = await _context.Assignees
                .Where(a => a.UserId == userId)
                .Select(a => new { a.TaskId, a.Task.Title, a.Task.Description })
                .ToListAsync();

            return Ok(tasks);
        }

        [Tags("AssigneeManagement")]
        [HttpDelete]
        public async Task<IActionResult> UnassignUserFromTask(int taskId, int userId)
        {
            var assignee = await _context.Assignees.FirstOrDefaultAsync(a => a.TaskId == taskId && a.UserId == userId);
            if (assignee == null)
            {
                return NotFound("No such task was found assigned to this user.");
            }

            _context.Assignees.Remove(assignee);
            await _context.SaveChangesAsync();
            return Ok("User's task assignment was removed successfully.");
        }
    }
} 