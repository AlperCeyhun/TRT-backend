using Microsoft.AspNetCore.Mvc;
using TRT_backend.Models;
using TRT_backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/message")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [Tags("MessageManagement")]
        [HttpGet("{user1Name}/{user2Name}")]
        public async Task<IActionResult> GetMessages(string user1Name, string user2Name)
        {
            var messages = await _messageService.GetMessagesBetweenUsersAsync(user1Name, user2Name);
            if (messages == null || messages.Count == 0)
                return NotFound("User not found or no messages.");

            var result = messages.Select(m => new {
                m.Id,
                m.Content,
                m.CreatedAt,
                FromUserName = m.FromUser.username,
                ToUserName = m.ToUser.username
            });
            return Ok(result);
        }
    }
}