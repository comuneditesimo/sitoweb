using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Spid;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface ISPIDService
    {
        public Task<string?> AuthenticateSpid(Guid Auth_Municipality_ID, string ReturnBaseUrl);
        public Task<ProfileData?> GetUserData(Guid Auth_Municipality_ID, string Token);
        public void SetLog(Guid Auth_Municipality_ID, string Header, string Content);
    }
}
