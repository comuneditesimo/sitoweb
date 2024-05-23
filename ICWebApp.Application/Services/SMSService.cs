using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Domain.DBModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class SMSService : ISMSService
    {
        private readonly IMSGProvider _MSGProvider;
        private readonly ICONFProviderSessionless _CONFProvider;

        public SMSService(IMSGProvider MSGProvider, ICONFProviderSessionless CONFProvider)
        {
            this._MSGProvider = MSGProvider;
            this._CONFProvider = CONFProvider;
        }

        public async Task<bool> SendSMS(MSG_SMS SMS, Guid AUTH_Municipality_ID, Guid? CONF_SMS_ID = null)
        {
            if(SMS != null)
            {
                var config = await _CONFProvider.GetSMSConfiguration(CONF_SMS_ID, AUTH_Municipality_ID);

                if (config == null)
                {
                    return false;
                }

                if(SMS.PhoneNumber == null)
                {
                    return false;
                }

                if (SMS.DisplayName == null)
                {
                    SMS.DisplayName = config.DefaultSenderName;
                }

                if (!SMS.PhoneNumber.Contains("+"))
                {
                    SMS.PhoneNumber = "+" + SMS.PhoneNumber;
                }

                SMS.SendDate = DateTime.Now;

                var result = await _MSGProvider.SetSMS(SMS);

                if (result)
                {
                    if (!Configuration.Default.ApiKey.ContainsKey("api-key"))
                    {
                        Configuration.Default.ApiKey.Add("api-key", config.APIKey);
                    }
                    else
                    {
                        Configuration.Default.ApiKey["api-key"] = config.APIKey;
                    }

                    var apiInstance = new TransactionalSMSApi();
                    string sender = SMS.DisplayName;
                    string recipient = SMS.PhoneNumber;
                    string content = SMS.Message;
                    SendTransacSms.TypeEnum type = SendTransacSms.TypeEnum.Transactional;
                    string tag = config.Tag;


                    var isNumericSender = int.TryParse(sender, out _);
                    if (sender.Length > 11 && isNumericSender == false)
                    {
                        sender = sender.Replace("Gemeinde ", "Gem ");
                        sender = sender.Replace("Comune ", "Com ");
                        sender = sender.Replace("/ ", " ");
                        if (sender.Length > 11)
                        {
                            sender = sender.Substring(0, 11);
                        }
                    }

                    try
                    {
                        var sendTransacSms = new SendTransacSms(sender, recipient, content, type, tag);
                        SendSms resultSMS = apiInstance.SendTransacSms(sendTransacSms);
                    }
                    catch (Exception e)
                    {
                        Debugger.Break();
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
