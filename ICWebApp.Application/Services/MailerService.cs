using Chilkat;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class MailerService : IMailerService
    {
        private IMSGProvider _MSGProvider;
        private ICONFProviderSessionless _CONFProvider;
        private IUnitOfWork _unitOfWork;

        public MailerService(IMSGProvider _MSGProvider, ICONFProviderSessionless _CONFProvider, IUnitOfWork _unitOfWork)
        {
            this._MSGProvider = _MSGProvider;
            this._CONFProvider = _CONFProvider;
            this._unitOfWork = _unitOfWork;
        }
        public async Task<bool> SendMail(MSG_Mailer Mail, List<MSG_Mailer_Attachment>? Mail_Attachments, Guid AUTH_Municipality_ID, Guid? CONF_Mailer_ID = null)
        {
            if (Mail != null)
            {
                try
                {
                    var config = await _CONFProvider.GetMailerConfiguration(CONF_Mailer_ID, AUTH_Municipality_ID);

                    if (config == null || config.CONF_Mailer_TypeID == Guid.Empty)
                        return false;

                    var mailerType = await _CONFProvider.GetMailerType(config.CONF_Mailer_TypeID);

                    if (mailerType == null)
                        return false;

                    Mail.FromAdress = config.MailFrom;
                    Mail.DisplayName = config.DisplayName;
                    

                    await SaveMail(Mail, Mail_Attachments);

                    bool result = false;

                    if (mailerType.Code == "SMTP")
                    {
                        result = SendWithSmtp(Mail, Mail_Attachments, config);
                    }
                    else if(mailerType.Code == "SENDGRID")
                    {
                        result = await SendWithSendGrid(Mail, Mail_Attachments, config);
                    }
                    else if (mailerType.Code == "SENDINBLUE")
                    {
                        result = await SendWithSendinBlue(Mail, Mail_Attachments, config);
                    }

                    await FlagMailAsSend(Mail);

                    return result;
                }
                catch { }
            }

            return false;
        }
        private bool SendWithSmtp(MSG_Mailer Mail, List<MSG_Mailer_Attachment>? Mail_Attachments, CONF_Mailer config)
        {
            if (Mail != null)
            {
                try
                {
                    Mail.DisplayName = config.DisplayName;
                    Mail.FromAdress = config.MailFrom;

                    SmtpClient client = new SmtpClient(config.Client);
                    client.Credentials = new NetworkCredential(config.UserName, config.Password);
                    if (config.ClientPort != null)
                    {
                        client.Port = int.Parse(config.ClientPort);
                    }
                    //client.EnableSsl = true;
                    
                    var message = ParseDBMailToSmtpMail(Mail, Mail_Attachments, config);
                    client.Send(message);

                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }

            return false;
        }
        private async Task<bool> SendWithSendinBlue(MSG_Mailer Mail, List<MSG_Mailer_Attachment>? Mail_Attachments, CONF_Mailer config)
        {
            if (!Configuration.Default.ApiKey.ContainsKey("api-key"))
            {
                Configuration.Default.ApiKey.Add("api-key", config.ApiKey);
            }
            else
            {
                Configuration.Default.ApiKey["api-key"] = config.ApiKey;
            }

            var apiInstance = new TransactionalEmailsApi();

            SendSmtpEmail email = new SendSmtpEmail();

            email.To = new List<SendSmtpEmailTo>() { new SendSmtpEmailTo(Mail.ToAdress) };
            email.Sender = new SendSmtpEmailSender(config.DisplayName, config.MailFrom);
            email.Subject = Mail.Subject;
            email.Tags = new List<string>() { config.Tag };
            if (config.ReplyTo != null)
            {
                email.ReplyTo = new SendSmtpEmailReplyTo(config.ReplyTo);
            }

            if (config.TemplateID != null)
            {
                email.TemplateId = long.Parse(config.TemplateID);
                email.Params = new Dictionary<string, object>
                {
                    { "Content",  Mail.Body },
                    { "Subject", Mail.Subject }
                };
            }
            else
            {
                if (config.AUTH_Municipality_ID != null)
                {
                    var html = config.TemplateHtml;
                    var municipality = await _unitOfWork.Repository<AUTH_Municipality>().GetByIDAsync(config.AUTH_Municipality_ID.Value);

                    if (municipality != null)
                    {
                        var municipalityDE = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == municipality.Name_Text_SystemTexts_Code && p.LANG_LanguagesID == LanguageSettings.German);
                        var municipalityIT = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == municipality.Name_Text_SystemTexts_Code && p.LANG_LanguagesID == LanguageSettings.Italian);

                        if (municipalityDE != null)
                        {
                            html = html.Replace("##MunicipalityDE##", municipalityDE.Text);
                        }

                        if (municipalityIT != null)
                        {
                            html = html.Replace("##MunicipalityIT##", municipalityIT.Text);
                        }

                        var onlineUrl = GetImageUrl(municipality.Logo, municipality.InternalName, config.BaseUrl);

                        html = html.Replace("##LogoURL##", onlineUrl);

                        if (!string.IsNullOrEmpty(Mail.MailTitle))
                        {
                            html = html.Replace("##Subject##", Mail.MailTitle);
                        }
                        else
                        {
                            html = html.Replace("##Subject##", Mail.Subject);
                        }

                        html = html.Replace("##Content##", Mail.Body);

                        email.HtmlContent = html;
                    }
                    else
                    {
                        email.HtmlContent = Mail.Body;
                    }
                }
                else
                {
                    email.HtmlContent = Mail.Body;
                }
            }

            if (Mail_Attachments != null && Mail_Attachments.Any())
            {
                email.Attachment = new List<SendSmtpEmailAttachment>();

                foreach (var attachment in Mail_Attachments)
                {
                    email.Attachment.Add(new SendSmtpEmailAttachment()
                    {
                        Content = attachment.FileData,
                        Name = attachment.FileName
                    });
                }
            }


            var response = apiInstance.SendTransacEmail(email);

            return true;
        }
        private async Task<bool> SendWithSendGrid(MSG_Mailer Mail, List<MSG_Mailer_Attachment>? Mail_Attachments, CONF_Mailer config)
        {
            var client = new SendGridClient(config.ApiKey);
            var from = new EmailAddress(config.MailFrom, config.DisplayName);

            var to = new EmailAddress(Mail.ToAdress);
            var plainTextContent = Mail.Body;
            var htmlContent = Mail.Body;

            var msg = MailHelper.CreateSingleEmail(from, to, Mail.Subject, plainTextContent, htmlContent);

            if (Mail_Attachments != null)
            {

                foreach (var attachment in Mail_Attachments)
                {
                    var insert = GetSendGridAttachment(attachment);
                    msg.Attachments = new List<SendGrid.Helpers.Mail.Attachment>();
                    msg.Attachments.Add(insert);
                }

            }

            if (config.ReplyTo != null)
            {
                msg.ReplyTo = new EmailAddress(config.ReplyTo);
            }
            
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);


            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        private SendGrid.Helpers.Mail.Attachment GetSendGridAttachment(MSG_Mailer_Attachment attachment)
        {
            using (var stream = new MemoryStream(attachment.FileData))
            {
                try
                {
                    return new SendGrid.Helpers.Mail.Attachment()
                    {
                        Disposition = "attachment",
                        Type = "/" + Path.GetExtension(attachment.FileName).Replace(".", ""),
                        Filename = Path.GetFileNameWithoutExtension(attachment.FileName),
                        ContentId = attachment.ID.ToString(),
                        Content = Convert.ToBase64String(stream.ToArray())
                    };
                }
                finally
                {
                    stream.Close();
                }
            }
        }
        private MailMessage ParseDBMailToSmtpMail(MSG_Mailer Mail, List<MSG_Mailer_Attachment> Mail_Attachments, CONF_Mailer config)
        {
            MailMessage message = new MailMessage();

            message.To.Add(Mail.ToAdress);
            message.Subject = Mail.Subject;
            message.Body = Mail.Body;
            message.IsBodyHtml = true;
            message.From = new MailAddress(Mail.FromAdress, Mail.DisplayName);
            if (config.ReplyTo != null)
            {
                message.ReplyTo = new MailAddress(config.ReplyTo);
            }
            

            if (Mail_Attachments != null)
            {
                foreach (var dbattachment in Mail_Attachments)
                {
                    MemoryStream ms = new MemoryStream(dbattachment.FileData);

                    var attachment = new System.Net.Mail.Attachment(ms, dbattachment.FileName);
                    
                    message.Attachments.Add(attachment);
                }
            }

            return message;
        }
        private async Task<bool> SaveMail(MSG_Mailer Mail, List<MSG_Mailer_Attachment>? Mail_Attachments)
        {
            Mail.ID = Guid.NewGuid();
            await _MSGProvider.SetMail(Mail);

            if (Mail_Attachments != null && Mail_Attachments.Count() > 0)
            {
                foreach(var att in Mail_Attachments)
                {
                    if (att.ID == Guid.Empty)
                    {
                        att.ID = Guid.NewGuid();
                    }
                    att.MSG_MailerID = Mail.ID;
                }

                await _MSGProvider.SetMailAttachment(Mail, Mail_Attachments);
            }

            return true;
        }
        private async Task<bool> FlagMailAsSend(MSG_Mailer Mail)
        {
            Mail.RealSendDate = DateTime.Now;

            return await _MSGProvider.SetMail(Mail);
        }
        private string GetImageUrl(byte[] Data, string FileName, string BaseUrl)
        {
            if (!Directory.Exists("D:/Comunix/MailContent"))
            {
                Directory.CreateDirectory("D:/Comunix/MailContent");
            }

            var filepath = "D:/Comunix/MailContent/" + FileName + ".png";

            if (!File.Exists(filepath))
            {
                System.IO.File.WriteAllBytes(filepath, Data);
            }

            var onlinePath = BaseUrl + "Content/" + FileName + ".png";

            return onlinePath;
        }
    }
}
