using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/message")]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        [Tags("MessageManagement")]
        [HttpGet("{user1Name}/{user2Name}")]
         public async Task<IActionResult> GetMessages(string user1Name, string user2Name)
         {
             var fromUser = await _context.Users.FirstOrDefaultAsync(u => u.username == user1Name);
             var toUser = await _context.Users.FirstOrDefaultAsync(u => u.username == user2Name);
         
             if (fromUser == null || toUser == null)
                 return NotFound("User not found.");
         
             var messages = await _context.Messages
                 .Include(m => m.FromUser)
                 .Include(m => m.ToUser)
                 .Where(m =>
                     (m.FromUserId == fromUser.Id && m.ToUserId == toUser.Id) ||
                     (m.FromUserId == toUser.Id && m.ToUserId == fromUser.Id))
                 .OrderBy(m => m.CreatedAt)
                 .Select(m => new {
                     m.Id,
                     m.Content,
                     m.CreatedAt,
                     FromUserName = m.FromUser.username,
                     ToUserName = m.ToUser.username
                 })
                 .ToListAsync();
         
             return Ok(messages);
         }

    }
}