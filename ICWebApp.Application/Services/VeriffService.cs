using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.JSInterop;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class VeriffService : IVeriffService
    {
        private readonly IMSGProvider _MSGProvider;
        private readonly ICONFProvider _CONFProvider;
        private readonly IJSRuntime _jsRuntime;
        private readonly IAUTHProvider _authProvider;

        public VeriffService(IMSGProvider MSGProvider, ICONFProvider CONFProvider, IJSRuntime _jsRuntime, IAUTHProvider _authProvider)
        {
            this._MSGProvider = MSGProvider;
            this._CONFProvider = CONFProvider;
            this._authProvider = _authProvider;
            this._jsRuntime = _jsRuntime;
        }

        public async Task<bool> InitializeVeriff(Guid AUTH_Users_ID, string DivID)
        {
            var dbUser = await _authProvider.GetUser(AUTH_Users_ID);
            var conf = await _CONFProvider.GetVeriffConfiguration(null);

            if(dbUser != null && !string.IsNullOrEmpty(DivID) && conf != null && conf.ApiKey != null)
            {
                dbUser.VeriffStartDate = DateTime.Now;

                await _authProvider.UpdateUser(dbUser);

                string apikey = conf.ApiKey;
                string divID = DivID;

                await _jsRuntime.InvokeVoidAsync("Veriff_Initialize", divID, apikey, dbUser.Firstname, dbUser.Lastname, dbUser.ID.ToString());
            }

            return true;
        }
    }
}
