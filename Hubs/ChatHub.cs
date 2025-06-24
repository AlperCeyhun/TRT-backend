using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TRT_backend.Hubs
{
    public class ChatHub : Hub
    {
        
        private static ConcurrentDictionary<string, string> _connections = new();

        
        public Task RegisterUser(string userId)
        {
            _connections[userId] = Context.ConnectionId;
            return Task.CompletedTask;
        }

        
        public async Task SendPrivateMessage(string toUserId, string message)
        {
            if (_connections.TryGetValue(toUserId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
        }

        
        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            var item = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(item.Key))
            {
                _connections.TryRemove(item.Key, out _);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
