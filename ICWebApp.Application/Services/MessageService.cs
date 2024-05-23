using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Services;
using Microsoft.EntityFrameworkCore;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Services
{
    public class MessageService : IMessageService
    {
        private IUnitOfWork _unitOfWork;
        private IPushService _PushService;
        private ISMSService _SmsService;
        private IMailerService _MailerService;

        public event Action? RefreshRequested;

        public MessageService(IPushService PushService, ISMSService SmsService, IUnitOfWork _unitOfWork, IMailerService _MailerService)
        {
            this._PushService = PushService;
            this._SmsService = SmsService;
            this._unitOfWork = _unitOfWork;
            this._MailerService = _MailerService;
        }
        public async Task<List<MSG_Message>> GetMessages(Guid authUserID, int? limit)
        {
            if (limit != null)
            {
                return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == authUserID && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).Take(limit.Value).ToListAsync();
            }

            return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == authUserID && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).ToListAsync();
        }
        public async Task<List<MSG_Message>> GetMessagesToRead(Guid authUserID, int? limit)
        {
            if (limit != null)
            {
                return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == authUserID && p.FirstReadDate == null && p.ShowInList == true).Take(limit.Value).AsNoTracking().OrderByDescending(p => p.CreationDate).ToListAsync();
            }

            return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == authUserID && p.FirstReadDate == null && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).ToListAsync();
        }

        public bool UserHasUnreadMessages(Guid authUserID)
        {
            var unreadMessage = _unitOfWork.Repository<MSG_Message>().FirstOrDefault(p => p.AUTH_User_ID == authUserID && p.FirstReadDate == null && p.ShowInList == true);

            return unreadMessage != null;
        }
        public async Task<int> GetMessagesToReadCount(Guid authUserID)
        {
            return await _unitOfWork.Repository<MSG_Message>().Where(p => p.AUTH_User_ID == authUserID && p.FirstReadDate == null && p.ShowInList == true).AsNoTracking().OrderByDescending(p => p.CreationDate).CountAsync();
        }
        public async Task<MSG_Message?> GetMessage(Guid AUTH_UsersID, Guid AUTH_Municipality_ID, string? BodyTextCode, string? ShortTextCode, string SubjectTextCode,
            Guid MSG_MessageType_ID, bool GetTextFromDb = true, List<MSG_Message_Parameters>? Parameters = null, string? FirstName = null, string? LastName = null)
        {
            var userSett = await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(p => p.AUTH_UsersID == AUTH_UsersID);
            var user = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == AUTH_UsersID && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
            var municipality = await _unitOfWork.Repository<AUTH_Municipality>().GetByIDAsync(AUTH_Municipality_ID);

            if (user == null || municipality == null)
                return null;

            if (userSett == null)
            {
                userSett = new AUTH_UserSettings();
                userSett.ID = Guid.NewGuid();
                userSett.AUTH_UsersID = AUTH_UsersID;

                userSett.LANG_Languages_ID = LanguageSettings.German;

                await _unitOfWork.Repository<AUTH_UserSettings>().InsertOrUpdateAsync(userSett);
            }

            MSG_Message msg = new MSG_Message();

            msg.AUTH_User_ID = AUTH_UsersID;
            msg.AUTH_Municipality_ID = AUTH_Municipality_ID;
            msg.CreationDate = DateTime.Now;
            msg.MessageTypeID = MSG_MessageType_ID;
            msg.ToAddress = user.Email;
            msg.PhoneNumber = user.PhoneNumber;
            msg.LANG_LanguageID = userSett.LANG_Languages_ID.Value;

            var municipalName = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == municipality.Name_Text_SystemTexts_Code && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

            if (GetTextFromDb && userSett.LANG_Languages_ID != null)
            {
                if (BodyTextCode != null)
                {
                    var bodytext = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == BodyTextCode && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

                    if (bodytext != null && bodytext.Text != null)
                    {
                        bodytext.Text = bodytext.Text.Replace("{FirstName}", string.IsNullOrEmpty(FirstName) ? user.Firstname : FirstName);
                        bodytext.Text = bodytext.Text.Replace("{LastName}", string.IsNullOrEmpty(LastName) ? user.Lastname : LastName);

                        if (Parameters != null && Parameters.Count() > 0) 
                        {
                            foreach (var par in Parameters)
                            {
                                bodytext.Text = bodytext.Text.Replace(par.Code, par.Message);
                            }
                        }

                        if (municipalName != null)
                        {
                            bodytext.Text = bodytext.Text.Replace("{Municipality}", municipalName.Text);
                        }

                        msg.Messagetext = bodytext.Text;
                    }
                }
                if (SubjectTextCode != null)
                {
                    var subjecttext = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == SubjectTextCode && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

                    if (subjecttext != null && subjecttext.Text != null)
                    {
                        subjecttext.Text = subjecttext.Text.Replace("{FirstName}", user.Firstname);
                        subjecttext.Text = subjecttext.Text.Replace("{LastName}", user.Lastname);

                        if (Parameters != null && Parameters.Count() > 0)
                        {
                            foreach (var par in Parameters)
                            {
                                subjecttext.Text = subjecttext.Text.Replace(par.Code, par.Message);
                            }
                        }

                        if (municipalName != null)
                        {
                            subjecttext.Text = subjecttext.Text.Replace("{Municipality}", municipalName.Text);
                        }

                        msg.Subject = subjecttext.Text;
                    }
                }

                if (ShortTextCode != null)
                {
                    var shorttext = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == ShortTextCode && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

                    if (shorttext != null && shorttext.Text != null)
                    {
                        shorttext.Text = shorttext.Text.Replace("{FirstName}", user.Firstname);
                        shorttext.Text = shorttext.Text.Replace("{LastName}", user.Lastname);

                        if (Parameters != null && Parameters.Count() > 0)
                        {
                            foreach (var par in Parameters)
                            {
                                shorttext.Text = shorttext.Text.Replace(par.Code, par.Message);
                            }
                        }

                        if (municipalName != null)
                        {
                            shorttext.Text = shorttext.Text.Replace("{Municipality}", municipalName.Text);
                        }

                        msg.ShortText = shorttext.Text;
                    }
                }
            }
            else
            {
                msg.Messagetext = BodyTextCode;
                msg.Subject = SubjectTextCode;
                msg.ShortText = ShortTextCode;
            }

            var type = await _unitOfWork.Repository<MSG_MessageType>().FirstOrDefaultAsync(p => p.id == MSG_MessageType_ID);

            if(type != null && type.ShowInList == true)
            {
                msg.ShowInList = true;
            }

            return msg;
        }
        
        public async Task<MSG_Message?> GetMessageForMunicipalUser(Guid AUTH_Municipal_UsersID, Guid AUTH_Municipality_ID, string? BodyTextCode, string? ShortTextCode, string SubjectTextCode,
            Guid MSG_MessageType_ID, bool GetTextFromDb = true, List<MSG_Message_Parameters>? Parameters = null, string? FirstName = null, string? LastName = null)
        {
            var userSett = await _unitOfWork.Repository<AUTH_Municipal_Users_Settings>().FirstOrDefaultAsync(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_UsersID);
            var user = await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.ID == AUTH_Municipal_UsersID && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
            var municipality = await _unitOfWork.Repository<AUTH_Municipality>().GetByIDAsync(AUTH_Municipality_ID);

            if (user == null || municipality == null)
                return null;

            if (userSett == null)
            {
                userSett = new AUTH_Municipal_Users_Settings();
                userSett.ID = Guid.NewGuid();
                userSett.AUTH_Municipal_Users_ID = AUTH_Municipal_UsersID;

                userSett.LANG_Languages_ID = LanguageSettings.German;

                await _unitOfWork.Repository<AUTH_Municipal_Users_Settings>().InsertOrUpdateAsync(userSett);
            }

            MSG_Message msg = new MSG_Message();

            msg.AUTH_User_ID = AUTH_Municipal_UsersID;
            msg.AUTH_Municipality_ID = AUTH_Municipality_ID;
            msg.CreationDate = DateTime.Now;
            msg.MessageTypeID = MSG_MessageType_ID;
            msg.ToAddress = user.Email;
            msg.PhoneNumber = user.PhoneNumber;

            var municipalName = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == municipality.Name_Text_SystemTexts_Code && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

            if (GetTextFromDb && userSett.LANG_Languages_ID != null)
            {
                if (BodyTextCode != null)
                {
                    var bodytext = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == BodyTextCode && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

                    if (bodytext != null && bodytext.Text != null)
                    {
                        bodytext.Text = bodytext.Text.Replace("{FirstName}", string.IsNullOrEmpty(FirstName) ? user.Firstname : FirstName);
                        bodytext.Text = bodytext.Text.Replace("{LastName}", string.IsNullOrEmpty(LastName) ? user.Lastname : LastName);

                        if (Parameters != null && Parameters.Count() > 0) 
                        {
                            foreach (var par in Parameters)
                            {
                                bodytext.Text = bodytext.Text.Replace(par.Code, par.Message);
                            }
                        }

                        if (municipalName != null)
                        {
                            bodytext.Text = bodytext.Text.Replace("{Municipality}", municipalName.Text);
                        }

                        msg.Messagetext = bodytext.Text;
                    }
                }
                if (SubjectTextCode != null)
                {
                    var subjecttext = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == SubjectTextCode && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

                    if (subjecttext != null && subjecttext.Text != null)
                    {
                        subjecttext.Text = subjecttext.Text.Replace("{FirstName}", user.Firstname);
                        subjecttext.Text = subjecttext.Text.Replace("{LastName}", user.Lastname);

                        if (Parameters != null && Parameters.Count() > 0)
                        {
                            foreach (var par in Parameters)
                            {
                                subjecttext.Text = subjecttext.Text.Replace(par.Code, par.Message);
                            }
                        }

                        if (municipalName != null)
                        {
                            subjecttext.Text = subjecttext.Text.Replace("{Municipality}", municipalName.Text);
                        }

                        msg.Subject = subjecttext.Text;
                    }
                }

                if (ShortTextCode != null)
                {
                    var shorttext = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == ShortTextCode && p.LANG_LanguagesID == userSett.LANG_Languages_ID.Value);

                    if (shorttext != null && shorttext.Text != null)
                    {
                        shorttext.Text = shorttext.Text.Replace("{FirstName}", user.Firstname);
                        shorttext.Text = shorttext.Text.Replace("{LastName}", user.Lastname);

                        if (Parameters != null && Parameters.Count() > 0)
                        {
                            foreach (var par in Parameters)
                            {
                                shorttext.Text = shorttext.Text.Replace(par.Code, par.Message);
                            }
                        }

                        if (municipalName != null)
                        {
                            shorttext.Text = shorttext.Text.Replace("{Municipality}", municipalName.Text);
                        }

                        msg.ShortText = shorttext.Text;
                    }
                }
            }
            else
            {
                msg.Messagetext = BodyTextCode;
                msg.Subject = SubjectTextCode;
                msg.ShortText = ShortTextCode;
            }

            var type = await _unitOfWork.Repository<MSG_MessageType>().FirstOrDefaultAsync(p => p.id == MSG_MessageType_ID);

            if(type != null && type.ShowInList == true)
            {
                msg.ShowInList = true;
            }

            return msg;
        }
        public async Task<bool> SendMessage(MSG_Message messageToSend, string? Link = null, List<FILE_FileInfo>? Attachments = null)
        {
            if (messageToSend == null)
            {
                return false;
            }
            var msgTypes = await _unitOfWork.Repository<MSG_MessageType>().ToListAsync();

            var msgtype = msgTypes.FirstOrDefault(p => p.id == messageToSend.MessageTypeID);

            messageToSend.Link = Link;

            if (msgtype != null)
            {
                if (msgtype.SendMail == true && messageToSend.MailID == null && (messageToSend.PlannedSendDate == null || messageToSend.PlannedSendDate >= DateTime.Now))
                {
                    messageToSend = await SendMail(messageToSend, Attachments);
                }

                if (msgtype.SendSms == true && messageToSend.SmsID == null && (messageToSend.PlannedSendDate == null ||  messageToSend.PlannedSendDate >= DateTime.Now))
                {
                    messageToSend = await SendSMS(messageToSend);
                }

                if (msgtype.SendPush == true && messageToSend.PushID == null && (messageToSend.PlannedSendDate == null || messageToSend.PlannedSendDate >= DateTime.Now))
                {
                    messageToSend = await SendPush(messageToSend);
                }
                if (msgtype.ShowInList == true && msgtype.SendMail != true && msgtype.SendSms != true && msgtype.SendPush != true)
                {
                    messageToSend = await OnlyCreateListEntry(messageToSend);
                }
            }

            CallRequestRefresh();
            return true;
        }
        public async Task<bool> SendMessage(List<Guid?> AUTH_Users_ID, MSG_Message messageToSend, string? Link = null, List<FILE_FileInfo>? Attachments = null)
        {
            foreach (var users in AUTH_Users_ID)
            {
                messageToSend.Link = Link;
                messageToSend.AUTH_User_ID = users;

                await SendMail(messageToSend);
            }

            CallRequestRefresh();
            return true;
        }
        public async Task<bool> SendMessageToAuthority(Guid AUTH_Authority_ID, MSG_Message messageToSend, string? Link = null)
        {
            var authority = await _unitOfWork.Repository<AUTH_Authority>().GetByIDAsync(AUTH_Authority_ID);

            new MSG_Message_Parameters()
            {
                Code = "{Authority}",
                Message = ""
            };

            var AuthorityUsers = await _unitOfWork.Repository<V_AUTH_Authority_Users>().ToListAsync(p => p.AUTH_Authority_ID == AUTH_Authority_ID || p.AUTH_Authority_ID == null);

            string? StartText = messageToSend.Messagetext;
            string? StartShortText = messageToSend.ShortText;
            string? StartTitle = messageToSend.Subject;

            foreach (var AuthorityUser in AuthorityUsers)
            {
                var locMessage = new MSG_Message();

                locMessage.ID = Guid.NewGuid();

                locMessage.AUTH_User_ID = messageToSend.AUTH_User_ID;
                locMessage.MessageTypeID = messageToSend.MessageTypeID;
                locMessage.CreationDate = DateTime.Now;
                locMessage.PlannedSendDate = messageToSend.PlannedSendDate;
                locMessage.Subject = messageToSend.Subject;
                locMessage.Messagetext = messageToSend.Messagetext;
                locMessage.ShortText = messageToSend.ShortText;
                locMessage.LANG_LanguageID = messageToSend.LANG_LanguageID;
                locMessage.SenderAuthorityID = messageToSend.SenderAuthorityID;
                locMessage.SenderUserID = messageToSend.SenderUserID;
                locMessage.AUTH_Municipality_ID = messageToSend.AUTH_Municipality_ID;
                locMessage.ExternalObjectID = messageToSend.ExternalObjectID;
                locMessage.ExternalObjectType = messageToSend.ExternalObjectType;
                locMessage.FromAdress = messageToSend.FromAdress;
                locMessage.DisplayName = messageToSend.DisplayName;
                locMessage.Link = messageToSend.Link;

                locMessage.ToAddress = AuthorityUser.Email;

                locMessage.PhoneNumber = AuthorityUser.PhoneNumber;

                locMessage.ShowInList = messageToSend.ShowInList;

                if (authority != null)
                {
                    var authorityText = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == authority.TEXT_SystemText_Code && p.LANG_LanguagesID == AuthorityUser.LANG_Languages_ID.Value);

                    if (authorityText != null) 
                    {
                        if (StartText != null)
                        {
                            locMessage.Messagetext = StartText.Replace("{Authority}", authorityText.Text);
                        }
                        if (StartShortText != null)
                        {
                            locMessage.ShortText = StartShortText.Replace("{Authority}", authorityText.Text);
                        }
                        if (StartTitle != null)
                        {
                            locMessage.Subject = StartTitle.Replace("{Authority}", authorityText.Text);
                        }
                    }
                }

                locMessage.Link = Link;
                locMessage.AUTH_User_ID = AuthorityUser.ID;

                await SendMail(locMessage);
            }

            return true;
        }
        public async Task<bool> RemoveMessage(MSG_Message messageToSend)
        {
            if (messageToSend == null)
            {
                return false;
            }

            await _unitOfWork.Repository<MSG_Message>().DeleteAsync(messageToSend.ID);

            return true;
        }
        public async Task<bool> UpdateMessage(MSG_Message messageToSend)
        {
            if (messageToSend == null)
            {
                return false;
            }

            await _unitOfWork.Repository<MSG_Message>().InsertOrUpdateAsync(messageToSend);

            return true;
        }
        public async Task<bool> SendAllMessagesToSend(Guid? AuthMunicipalityID)
        {
            var msglist = await _unitOfWork.Repository<V_MSG_Messages_ToReSend>().ToListAsync();

            if (msglist != null)
            {
                foreach (var msg in msglist)
                {
                    if (msg.ID != null)
                    {
                        if (msg.SendMail == true && msg.RealSendDateMail == null && (msg.PlannedSendDate == null || msg.PlannedSendDate >= DateTime.Now))
                        {
                            var locMsg = await _unitOfWork.Repository<MSG_Message>().FirstOrDefaultAsync(p => p.ID == msg.ID);

                            if (locMsg != null)
                            {
                                await SendMail(locMsg);
                            }
                        }
                        if (msg.SendSms == true && msg.RealSendDateSms == null && (msg.PlannedSendDate == null || msg.PlannedSendDate >= DateTime.Now))
                        {
                            var locMsg = await _unitOfWork.Repository<MSG_Message>().FirstOrDefaultAsync(p => p.ID == msg.ID);

                            if (locMsg != null)
                            {
                                await SendSMS(locMsg);
                            }
                        }
                        if (msg.SendPush == true && msg.RealSendDatePush == null && (msg.PlannedSendDate == null || msg.PlannedSendDate >= DateTime.Now))
                        {
                            var locMsg = await _unitOfWork.Repository<MSG_Message>().FirstOrDefaultAsync(p => p.ID == msg.ID);

                            if (locMsg != null)
                            {
                                await SendPush(locMsg);
                            }
                        }
                    }
                }
            }

            return true;
        }
        public async Task<bool> SetMessagesRead(List<MSG_Message> messages)
        {
            foreach (var msg in messages)
            {
                msg.FirstReadDate = DateTime.Now;
            }
            await _unitOfWork.Repository<MSG_Message>().BulkUpdateAsync(messages);
            CallRequestRefresh();
            return true;
        }
        public async Task<bool> SetMessageRead(MSG_Message msg)
        {
            msg.FirstReadDate = DateTime.Now;
            await _unitOfWork.Repository<MSG_Message>().UpdateAsync(msg);
            CallRequestRefresh();
            return true;
        }
        public async Task<bool> SetAllMessageRead(List<MSG_Message> msgs)
        {
            foreach (MSG_Message _msg in  msgs)
            {
                _msg.FirstReadDate = DateTime.Now;
                await _unitOfWork.Repository<MSG_Message>().UpdateAsync(_msg);
            }
            CallRequestRefresh();
            return true;
        }
        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }
        private async Task<MSG_Message> SendMail(MSG_Message messageToSend, List<FILE_FileInfo>? Attachments = null)
        {
            var mail = new MSG_Mailer();
            List<MSG_Mailer_Attachment>? MailAttachments = null;

            mail.ToAdress = messageToSend.ToAddress;
            mail.FromAdress = messageToSend.FromAdress;
            mail.DisplayName = messageToSend.DisplayName;
            mail.Subject = messageToSend.Subject;
            mail.Body = messageToSend.Messagetext;
            mail.PlannedSendDate = messageToSend.PlannedSendDate;
            mail.MessageID = messageToSend.ID;

            if (Attachments != null && Attachments.Count() > 0)
            {
                foreach (var attachment in Attachments)
                {
                    var blob = await _unitOfWork.Repository<FILE_FileStorage>().FirstOrDefaultAsync(e => e.FILE_FileInfo_ID == attachment.ID);

                    if (blob != null)
                    {
                        var mailAttachment = new MSG_Mailer_Attachment();

                        mailAttachment.FileName = attachment.FileName + attachment.FileExtension;
                        mailAttachment.FileData = blob.FileImage;
                        mailAttachment.MSG_MailerID = mail.ID;

                        if (MailAttachments == null)
                        {
                            MailAttachments = new List<MSG_Mailer_Attachment>();
                        }

                        MailAttachments.Add(mailAttachment);
                    }
                }
            }

            if (messageToSend.AUTH_Municipality_ID != null && messageToSend.RealSendDateMail == null)
            {
                await _MailerService.SendMail(mail, MailAttachments, messageToSend.AUTH_Municipality_ID.Value);
            }

            messageToSend.MailID = mail.ID;
            messageToSend.RealSendDateMail = DateTime.Now;

            await _unitOfWork.Repository<MSG_Message>().InsertOrUpdateAsync(messageToSend);

            return messageToSend;
        }
        private async Task<MSG_Message> SendSMS(MSG_Message messageToSend)
        {
            MSG_SMS sms = new MSG_SMS();

            sms.ID = Guid.NewGuid();
            sms.PhoneNumber = messageToSend.PhoneNumber;
            sms.DisplayName = messageToSend.DisplayName;
            sms.Message = messageToSend.ShortText;
            sms.MessageID = messageToSend.ID;
            sms.AUTH_Municipality_ID = messageToSend.AUTH_Municipality_ID;
            if (messageToSend.AUTH_Municipality_ID != null && messageToSend.RealSendDateSms == null)
            {
                await _SmsService.SendSMS(sms, messageToSend.AUTH_Municipality_ID.Value);
            }

            messageToSend.SmsID = sms.ID;
            messageToSend.RealSendDateSms = DateTime.Now;

            await _unitOfWork.Repository<MSG_Message>().InsertOrUpdateAsync(messageToSend);

            return messageToSend;
        }
        private async Task<MSG_Message> SendPush(MSG_Message messageToSend)
        {
            MSG_Push push = new MSG_Push();

            push.MunicipalityName = messageToSend.DisplayName;
            push.UserID = messageToSend.AUTH_User_ID;
            push.MessageDE = messageToSend.ShortText;

            push.MessageEN = messageToSend.ShortText;

            push.MessageIT = messageToSend.ShortText;

            push.MessageID = messageToSend.ID;

            if (messageToSend.AUTH_Municipality_ID != null && messageToSend.RealSendDatePush == null)
            {
                await _PushService.SendPush(push, messageToSend.AUTH_Municipality_ID.Value);                
            }

            messageToSend.PushID = push.ID;
            messageToSend.RealSendDatePush = DateTime.Now;

            await _unitOfWork.Repository<MSG_Message>().InsertOrUpdateAsync(messageToSend);

            return messageToSend;
        }
        private async Task<MSG_Message> OnlyCreateListEntry(MSG_Message messageToSend)
        {
            await _unitOfWork.Repository<MSG_Message>().InsertOrUpdateAsync(messageToSend);

            return messageToSend;
        }
    }
}
