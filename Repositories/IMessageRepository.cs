using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<List<Message>> GetMessagesBetweenUsersAsync(string user1Name, string user2Name);
    }
} 