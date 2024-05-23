using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IAUTHSettingsProvider
    {
        public Task<AUTH_UserSettings> GetSettings(Guid UserID);
        public Task<bool> SetSettings(AUTH_UserSettings UserSettings);
        public Task<AUTH_Municipal_Users_Settings> GetMunicipalSettings(Guid AUTH_Municipal_Users_ID);
        public Task<bool> SetMunicipalSettings(AUTH_Municipal_Users_Settings UserSettings);
    }
}
