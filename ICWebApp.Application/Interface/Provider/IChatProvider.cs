using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IChatProvider
    {
        public Task<List<V_CHAT_Message>> GetChatMessages(string contextElementId);
        public Task<List<V_CHAT_Message_Document>> GetChatDocuments(string contextElementId);
        public Task<V_CHAT_Message?> MarkMessageAsRead(V_CHAT_Message msg);
        public Task<CHAT_Message?> SaveMessage(CHAT_Message msg, List<CHAT_Message_Document> docs);
        public Task<List<V_CHAT_Messages_Responsible>> GetMessagesByUser(Guid AUTH_Users_ID, Guid LANG_Language_ID);
        public Task<bool> HasContextMessages(string contextElementId);
    }
}
