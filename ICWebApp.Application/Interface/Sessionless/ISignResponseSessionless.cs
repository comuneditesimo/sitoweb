using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.AdobeSign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Sessionless
{
    public interface ISignResponseSessionless
    {
        public Task<Token?> SetAccessToken(Guid AUTH_Municipality_ID, string code);
        public Task<Token?> RefreshAccessToken(Guid CONF_ID, Guid AUTH_Municipality_ID);
        public Task<Token?> RefreshAccessToken(Guid AUTH_Municipality_ID);
        public Task<bool> RevokeAccessToken(Guid AUTH_Municipality_ID);
    }
}
