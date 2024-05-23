using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Sessionless
{
    public interface ICONFProviderSessionless 
    { 
        public Task<CONF_Mailer?> GetMailerConfiguration(Guid? ID, Guid AUTH_Municipality_ID);
        public Task<CONF_SMS?> GetSMSConfiguration(Guid? ID, Guid AUTH_Municipality_ID);
        public Task<CONF_Push?> GetPushConfiguration(Guid? ID, Guid AUTH_Municipality_ID);
        public Task<CONF_Mailer_Type?> GetMailerType(Guid? ID);
    }
}
