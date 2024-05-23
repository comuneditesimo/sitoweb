using Microsoft.AspNetCore.SignalR;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Hubs
{
    public class SignalRChatHub : Hub
    {
        public async Task RegisterToGroup(string contextId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, contextId);
        }

        public async Task RemoveFromGroup(string contextId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, contextId);
        }

        public async Task NotifyGroup(string contextId, V_CHAT_Message message, List<V_CHAT_Message_Document> documents)
        {
            await Clients.Group(contextId).SendAsync("ReceiveMessage", message, documents);
        }
    }
}