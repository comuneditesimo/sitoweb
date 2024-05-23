using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IMSGProvider
    {
        public Task<MSG_SystemMessages> GetSystemMessage(string Code);
        public string GetValidationMessage(string Code);
        public Task<bool> SetMail(MSG_Mailer Mail);
        public Task<bool> SetSMS(MSG_SMS SMS);
        public Task<bool> SetPush(MSG_Push Push);
        public Task<bool> SetMailAttachment(MSG_Mailer Mail, List<MSG_Mailer_Attachment> MailAttachments);
        public Task<List<MSG_SystemNotifications>?> GetSystemNotifications();
        public Task<MSG_Message?> SetMessage(MSG_Message Data);
        public Task<MSG_Message?> GetMessage(Guid ID);
        public Task<List<MSG_MessageType>> GetMessageType();
        public Task<List<MSG_Message>> GetMessagesByUserID(Guid AUTH_Users_ID, int? Amount = null);
        public Task<List<MSG_Message>> GetMessagesToReadByUserID(Guid AUTH_Users_ID, int? Amount = null);
        public Task<int> GetMessagesToReadCountByUserID(Guid AUTH_Users_ID);
        public Task<bool> RemoveMessage(Guid ID);
        public Task<List<V_MSG_Messages_ToReSend>?> GetMessagesToSend();
    }
}
