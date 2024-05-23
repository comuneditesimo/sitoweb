using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IPushService
    {
        public Task<bool> SendPush(MSG_Push Push, Guid AUTH_Municipality_ID, Guid? CONF_Push_ID = null);
    }
}
