using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface ISMSService
    {
        public Task<bool> SendSMS(MSG_SMS SMS, Guid AUTH_Municipality_ID, Guid? CONF_SMS_ID = null);
    }
}
