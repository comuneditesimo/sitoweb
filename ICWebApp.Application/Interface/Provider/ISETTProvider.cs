using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ISETTProvider
    {
        public Task<SETT_Municipal_Dashboard?> GetDashboard(Guid AUTH_Users_ID);
        public Task<SETT_Municipal_Dashboard?> SetDashboard(SETT_Municipal_Dashboard? Data);
        public Task<SETT_User_States?> GetUserState(Guid AUTH_Users_ID);
        public Task<SETT_User_States?> SetUserState(SETT_User_States? Data);
    }
}
