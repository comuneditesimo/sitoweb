using ICWebApp.Application.Interface.Provider;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using System.Globalization;
using System.Diagnostics;
using AdobeSign.Rest.Model.LibraryDocuments;
using Stripe;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Application.Provider
{
    public class ChatProvider : IChatProvider
    {
        private readonly IFILEProvider _fileProvider;
        private readonly IUnitOfWork _unitOfWork;
        public ChatProvider(IUnitOfWork unitOfWork, IFILEProvider fileProvider)
        {
            _unitOfWork = unitOfWork;
            _fileProvider = fileProvider;
        }

        public async Task<List<V_CHAT_Message>> GetChatMessages(string contextElementId)
        {
            return await _unitOfWork.Repository<V_CHAT_Message>().ToListAsync(e => e.ContextElementId == contextElementId);
        }
        public async Task<List<V_CHAT_Message_Document>> GetChatDocuments(string contextElementId)
        {
            return await _unitOfWork.Repository<V_CHAT_Message_Document>().ToListAsync(e => e.ContextElementId == contextElementId);
        }
        public async Task<V_CHAT_Message?> MarkMessageAsRead(V_CHAT_Message msg)
        {
            CHAT_Message? _message = await _unitOfWork.Repository<CHAT_Message>().FirstOrDefaultAsync(p => p.ID == msg.ID);
            if (_message != null)
            {
                _message.ReadDate = DateTime.Now;
                await _unitOfWork.Repository<CHAT_Message>().InsertOrUpdateAsync(_message);
                V_CHAT_Message? _updatedMessage = await _unitOfWork.Repository<V_CHAT_Message>().FirstOrDefaultAsync(e => e.ID == msg.ID);
                return _updatedMessage;
            }
            return msg!;
        }
        public async Task<CHAT_Message?> SaveMessage(CHAT_Message msg, List<CHAT_Message_Document> docs)
        {
            CHAT_Message? message = await _unitOfWork.Repository<CHAT_Message>().InsertOrUpdateAsync(msg);
            if (docs != null && docs.Any() && message != null)
            {
                foreach (var doc in docs)
                {
                    doc.CHAT_Message_ID = message.ID;
                    await _fileProvider.SetFileInfo(doc.FILE_FileInfo);
                    await _unitOfWork.Repository<CHAT_Message_Document>().InsertOrUpdateAsync(doc);
                }
            }
            return message;
        }
        public async Task<bool> HasContextMessages(string contextElementId)
        {
            return (await _unitOfWork.Repository<CHAT_Message>()
                .FirstOrDefaultAsync(e => e.ContextElementId == contextElementId)) != null;
        }
        public async Task<List<V_CHAT_Messages_Responsible>> GetMessagesByUser(Guid AUTH_Users_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_CHAT_Messages_Responsible>().Where(p => p.AUTH_Frontend_Users_ID == AUTH_Users_ID && p.LANG_Language_ID == LANG_Language_ID).OrderByDescending(p => p.SendDate).ToListAsync();
        }
    }
}
