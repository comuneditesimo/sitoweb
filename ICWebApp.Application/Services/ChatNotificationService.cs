using ICWebApp.Application.Interface.Services;
using Microsoft.EntityFrameworkCore;
using ICWebApp.DataStore.MSSQL;
using ICWebApp.Domain.DBModels;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System;
using System.IO;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;

namespace ICWebApp.Application.Services
{
    public class ChatNotificationService : IChatNotificationService
    {
        private IUnitOfWork _unitOfWork;
        public event Action? OnCitizenChatStatusChange;
        public event Action? OnMunicipalChatStatusChange;
        private List<V_CHAT_Unread_Messages_Responsible> _unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();
        public List<V_CHAT_Unread_Messages_Responsible> UnreadMessages
        {
            get
            {
                return _unreadMessages;
            }
        }

        public ChatNotificationService(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;

            InitializeChatNotificationService();

        }

        public async Task NotifyCitizenStatusChange()
        {
            await ReadAllUnreadMessagesWithResponsibles();
            if (OnCitizenChatStatusChange != null)
            {

                WriteEventInFile(string.Format("Update Citizen Chat Status. DateTime: {0}.", DateTime.Now));

                OnCitizenChatStatusChange.Invoke();
            }
        }
        public async Task NotifyMunicipalStatusChange()
        {
            await ReadAllUnreadMessagesWithResponsibles();
            if (OnMunicipalChatStatusChange != null)
            {

                WriteEventInFile(string.Format("Update Municipal Chat Status. DateTime: {0}", DateTime.Now));

                OnMunicipalChatStatusChange.Invoke();
            }
        }
        public async Task<List<V_CHAT_Unread_Messages_Responsible>> ReadAllUnreadMessagesWithResponsibles()
        {
            List<V_CHAT_Unread_Messages_Responsible> unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();

            unreadMessages = await _unitOfWork.Repository<V_CHAT_Unread_Messages_Responsible>().ToListAsync();

            if (unreadMessages == null)
            {
                unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();
            }
            _unreadMessages = unreadMessages;

            return unreadMessages;
        }
        private async Task InitializeChatNotificationService()
        {
            await ReadAllUnreadMessagesWithResponsibles();

            WriteEventInFile(string.Format("Init new ChatNotificationService. DateTime: {0}", DateTime.Now));
            NotifyCitizenStatusChange();
            NotifyMunicipalStatusChange();

        }
        private async Task WriteEventInFile(string text)
        {
            try
            {
                string filePath = @"C:\Temp\Comunix";
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                using (StreamWriter streamwriter = File.AppendText(filePath + @"\ChatNotificationServiceLog.txt"))
                {
                    streamwriter.WriteLine(text);
                }
            }
            catch
            { }
        }
    }
}