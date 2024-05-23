using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
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
using System.Text.Json;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class PushService : IPushService
    {
        private readonly IMSGProvider _MSGProvider;
        private readonly ICONFProviderSessionless _CONFProvider;

        public PushService(IMSGProvider MSGProvider, ICONFProviderSessionless CONFProvider)
        {
            this._MSGProvider = MSGProvider;
            this._CONFProvider = CONFProvider;
        }
        public async Task<bool> SendPush(MSG_Push Push, Guid AUTH_Municipality_ID, Guid? CONF_Push_ID = null)
        {
            if(Push != null)
            {
                var config = await _CONFProvider.GetPushConfiguration(CONF_Push_ID, AUTH_Municipality_ID);

                if (config == null)
                {
                    return false;
                }

                if(Push.MunicipalityName == null && Push.UserID == null)
                {
                    return false;
                }

                if (Push.MessageDE == null && Push.MessageEN == null && Push.MessageIT == null)
                {
                    return false;
                }


                Push.SendDate = DateTime.Now;

                var result = await _MSGProvider.SetPush(Push);

                if (result)
                {
                    var pn = new PushNotification();

                    pn.app_id = config.AppID ?? "";
                    pn.filters = new List<Filter>();
                    pn.contents = new Dictionary<string, string>();

                    if (Push.MunicipalityName != null)
                    {
                        pn.filters.Add(new Filter { field = "tag", key = Push.MunicipalityName, relation = "=", value = Push.MunicipalityName });
                    }
                    if (Push.UserID != null)
                    {
                        pn.filters.Add(new Filter { field = "tag", key = "UserID", relation = "=", value = Push.UserID.ToString() ?? "" });
                    }

                    pn.contents.Add("en", Push.MessageEN ?? Push.MessageDE ?? Push.MessageIT ?? "");
                    if (Push.MessageDE != null && Push.MessageIT != null)
                    {
                        pn.contents.Add("de", Push.MessageDE);
                        pn.contents.Add("it", Push.MessageIT);
                    }

                    //Additional payload data for app
                    /*
                     *pn.data.Add("Key1", "Value1");
                     *pn.data.Add("Key2", "Value2");
                    */

                    try
                    {

                        var content = new StringContent(JsonSerializer.Serialize(pn), Encoding.UTF8, "application/json");
                        using (var request = new HttpRequestMessage(HttpMethod.Post, "https://onesignal.com/api/v1/notifications"))
                        {
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", config.ApiKey);
                            request.Content = content;
                            using (var client = new HttpClient())
                            {
                                await client.SendAsync(request);
                            }
                        }

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
