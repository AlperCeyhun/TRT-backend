using TRT_backend.Models;

namespace TRT_backend.Services
{
    public interface IMessageService
    {
        Task<List<Message>> GetMessagesBetweenUsersAsync(string user1Name, string user2Name);
    }
} 