using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Interface.Services
{
    public interface IChatNotificationService
    {
        public List<V_CHAT_Unread_Messages_Responsible> UnreadMessages { get; }

        public event Action? OnCitizenChatStatusChange;
        public event Action? OnMunicipalChatStatusChange;

        public Task NotifyCitizenStatusChange();
        public Task NotifyMunicipalStatusChange();
        public Task<List<V_CHAT_Unread_Messages_Responsible>> ReadAllUnreadMessagesWithResponsibles();
    }
}
