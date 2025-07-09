using Microsoft.AspNetCore.Mvc;
using TRT_backend.Services;
using TRT_backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/assignees")]
    [Authorize]
    public class AssigneeController : ControllerBase
    {
        private readonly IAssigneeService _assigneeService;
        private readonly IUserService _userService;

        public AssigneeController(IAssigneeService assigneeService, IUserService userService)
        {
            _assigneeService = assigneeService;
            _userService = userService;
        }
        [EndpointSummary("AssignUsersToTask")]
        [Tags("AssigneeManagement")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignUsersToTask(int taskId, [FromBody] List<int> userIds)
        {
           
            if (!_userService.IsUserAdminFromToken(User))
                return StatusCode(403, "Only admins can assign users to tasks.");

            var success = await _assigneeService.AssignUsersToTaskAsync(taskId, userIds);
            if (!success)
                return NotFound("Task or user not found.");

            return Ok("User(s) assigned successfully.");
        }

        [EndpointSummary("GetAssigneesForTask")]
        [Tags("AssigneeManagement")]
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetAssigneesForTask(int taskId)
        {
            var assignees = await _assigneeService.GetAssigneesForTaskAsync(User, taskId);
            return Ok(assignees.Select(a => new {
                a.Id,
                UserId = a.User.Id,
                Username = a.User.username
            }));
        }
        [EndpointSummary("GetTasksForUser")]
        [Tags("AssigneeManagement")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksForUser(int userId)
        {
            var currentUserId = _userService.GetUserIdFromToken(User);
            
            
            if (!_userService.IsUserAdminFromToken(User) && currentUserId != userId)
                return StatusCode(403, "You can only view your own tasks.");

            var tasks = await _assigneeService.GetTasksForUserAsync(User);
            return Ok(tasks.Select(t => new {
                t.Id,
                t.Title,
                t.Description,
                t.Completed,
                Assignees = t.Assignees.Select(a => new {
                    a.Id,
                    UserId = a.User.Id,
                    Username = a.User.username
                }).ToList()
            }));
        }
        [EndpointSummary("UnassignUserFromTask")]
        [Tags("AssigneeManagement")]
        [HttpDelete]
        public async Task<IActionResult> UnassignUserFromTask(int taskId, int userId)
        {
            var currentUserId = _userService.GetUserIdFromToken(User);
            
            
            if (!_userService.IsUserAdminFromToken(User) && currentUserId != userId)
                return StatusCode(403, "You can only unassign yourself from tasks.");

            var success = await _assigneeService.UnassignUserFromTaskAsync(taskId, userId);
            if (!success)
                return NotFound("No such task was found assigned to this user.");

            return Ok("User's task assignment was removed successfully.");
        }
    }
} 