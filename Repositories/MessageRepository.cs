using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(AppDbContext context) : base(context) { }

        public async Task<List<Message>> GetMessagesBetweenUsersAsync(string user1Name, string user2Name)
        {
            var fromUser = await _context.Users.FirstOrDefaultAsync(u => u.username == user1Name);
            var toUser = await _context.Users.FirstOrDefaultAsync(u => u.username == user2Name);
            if (fromUser == null || toUser == null)
                return new List<Message>();

            return await _context.Messages
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .Where(m =>
                    (m.FromUserId == fromUser.Id && m.ToUserId == toUser.Id) ||
                    (m.FromUserId == toUser.Id && m.ToUserId == fromUser.Id))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }
    }
} 