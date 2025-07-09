using TRT_backend.Models;
using TRT_backend.Repositories;

namespace TRT_backend.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<List<Message>> GetMessagesBetweenUsersAsync(string user1Name, string user2Name)
        {
            return await _messageRepository.GetMessagesBetweenUsersAsync(user1Name, user2Name);
        }
    }
} 