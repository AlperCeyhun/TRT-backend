using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Hubs
{
    public class ChatHub : Hub
    {
        // Bağlı kullanıcıların connectionId'si tutulur
        private static ConcurrentDictionary<string, string> _connections = new();

        private readonly IServiceProvider _serviceProvider;

        public ChatHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Kullanıcı bağlandığında çağrılır
        public Task RegisterUser(string userId)
        {
            var connId = Context.ConnectionId;
            Console.WriteLine($"RegisterUser → userId: {userId}, connId: {connId}");
            _connections[userId] = connId;
            return Task.CompletedTask;
        }

        // Özel mesaj gönderme işlemi
        public async Task SendPrivateMessage(string fromUserName, string toUserName, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Kullanıcılar veritabanından alınır
            var fromUser = await db.Users.FirstOrDefaultAsync(u => u.username == fromUserName);
            var toUser = await db.Users.FirstOrDefaultAsync(u => u.username == toUserName);

            if (fromUser == null || toUser == null)
            {
                Console.WriteLine($"Kullanıcı bulunamadı → from: {fromUserName}, to: {toUserName}");
                return;
            }

            // Mesaj nesnesi oluşturulur
            var message = new Message
            {
                FromUserId = fromUser.Id,
                ToUserId = toUser.Id,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            // Alıcı bağlıysa mesajı gönder
            if (_connections.TryGetValue(toUser.Id.ToString(), out var receiverConnId))
            {
                Console.WriteLine($"→ Mesaj gönderiliyor: {toUser.username} ({receiverConnId})");
                await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.FromUserId,
                    message.ToUserId,
                    message.Content,
                    message.CreatedAt,
                    FromUserName = fromUser.username,
                    ToUserName = toUser.username
                });
            }
            else
            {
                Console.WriteLine($"Alıcı çevrimdışı: {toUser.username}");
            }

            // Göndericiye de mesaj gönderildi onayı
            if (_connections.TryGetValue(fromUser.Id.ToString(), out var senderConnId))
            {
                await Clients.Client(senderConnId).SendAsync("MessageSent", new
                {
                    message.Id,
                    message.ToUserId,
                    message.Content,
                    message.CreatedAt
                });
            }

            // Mesaj veritabanına kaydedilir
            db.Messages.Add(message);
            await db.SaveChangesAsync();
        }

        // Kullanıcı bağlantısını kopardığında tetiklenir
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var item = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(item.Key))
            {
                _connections.TryRemove(item.Key, out _);
                Console.WriteLine($"Kullanıcı bağlantısı koptu → userId: {item.Key}");
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
