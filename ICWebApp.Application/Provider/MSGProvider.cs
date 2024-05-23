using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.DataStore.MSSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ICWebApp.Application.Provider
{
    public class MSGProvider : IMSGProvider
    {
        private IUnitOfWork _unitOfWork;
        public MSGProvider(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }
        public async Task<MSG_SystemMessages> GetSystemMessage(string Code)
        {
            Guid Lang = LanguageSettings.German;

            var Language = _unitOfWork.Repository<LANG_Languages>().First(p => p.Code == CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {
                Lang = Language.ID;

            }
            var msg = await _unitOfWork.Repository<MSG_SystemMessages>().FirstOrDefaultAsync(p => p.Code == Code && p.LANG_LanguagesID == Lang);

            if (msg != null)
                return msg;

            return new MSG_SystemMessages() { Code = "Error", Message = "404 - Error Code Not Found!" };
        }
        public string GetValidationMessage(string Code)
        {
            var Language = _unitOfWork.Repository<LANG_Languages>().First(p => p.Code == CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {
                var Lang = Language.ID;

                var msg = _unitOfWork.Repository<MSG_SystemMessages>().FirstOrDefault(p => p.Code == Code && p.LANG_LanguagesID == Lang);

                if (msg != null)
                    return msg.Message;
            }

            return "404 - Error Code Not Found!";
        }
        public async Task<bool> SetMail(MSG_Mailer Mail)
        {
            await _unitOfWork.Repository<MSG_Mailer>().InsertOrUpdateAsync(Mail);

            return true;
        }
        public async Task<bool> SetMailAttachment(MSG_Mailer Mail, List<MSG_Mailer_Attachment> MailAttachments)
        {
            foreach (var mailAttachment in MailAttachments)
            {
                mailAttachment.MSG_MailerID = Mail.ID;

                await _unitOfWork.Repository<MSG_Mailer_Attachment>().InsertOrUpdateAsync(mailAttachment);
            }

            return true;
        }
        public async Task<List<MSG_SystemNotifications>?> GetSystemNotifications()
        {
            var Language = _unitOfWork.Repository<LANG_Languages>().First(p => p.Code == CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {
                var Lang = Language.ID;

                var data = await _unitOfWork.Repository<MSG_SystemNotifications>().Where(p => (p.ShowFrom == null || p.ShowFrom <= DateTime.Now) && 
                                                                                              (p.ShowUntil == null || p.ShowUntil >= DateTime.Now) && 
                                                                                              p.LANG_Languages_ID == Lang).Include(p => p.MSG_SystemMessageTypes).ToListAsync();

                if (data != null && data.Count > 0)
                    return data;
            }

            return null;
        }
        public async Task<bool> SetSMS(MSG_SMS SMS)
        {
            await _unitOfWork.Repository<MSG_SMS>().InsertOrUpdateAsync(SMS);

            return true;
        }
        public async Task<bool> SetPush(MSG_Push Push)
        {
            await _unitOfWork.Repository<MSG_Push>().InsertOrUpdateAsync(Push);

            return true;
        }
        public async Task<MSG_Message?> SetMessage(MSG_Message? Data)
        {
            if (Data == null)
                return null;

            return await _unitOfWork.Repository<MSG_Message>().InsertOrUpdateAsync(Data);
        }
        public async Task<MSG_Message?> GetMessage(Guid ID)
        {
            return await _unitOfWork.Repository<MSG_Message>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<MSG_MessageType>> GetMessageType()
        {
            return await _unitOfWork.Repository<MSG_MessageType>().ToListAsync();
        }
        public async Task<List<MSG_Message>> GetMessagesByUserID(Guid AUTH_Users_ID, int? Amount = null)
        {
            if (Amount != null)
            {
                return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == AUTH_Users_ID && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).Take(Amount.Value).ToListAsync();
            }

            return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == AUTH_Users_ID && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).ToListAsync();
        }
        public async Task<List<MSG_Message>> GetMessagesToReadByUserID(Guid AUTH_Users_ID, int? Amount = null)
        {
            if (Amount != null)
            {
                return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == AUTH_Users_ID && p.FirstReadDate == null && p.ShowInList == true).Take(Amount.Value).AsNoTracking().OrderByDescending(p => p.CreationDate).ToListAsync();
            }

            return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == AUTH_Users_ID && p.FirstReadDate == null && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).ToListAsync();
        }
        public async Task<int> GetMessagesToReadCountByUserID(Guid AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == AUTH_Users_ID && p.FirstReadDate == null && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).CountAsync();
        }
        public async Task<bool> RemoveMessage(Guid ID)
        {
            return await _unitOfWork.Repository<MSG_Message>().DeleteAsync(ID);
        }
        public async Task<List<V_MSG_Messages_ToReSend>?> GetMessagesToSend()
        {
            return await _unitOfWork.Repository<V_MSG_Messages_ToReSend>().ToListAsync();
        }
    }
}
