using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IDASHProvider
    {
        public List<V_DASH_UserChat> GetDashUserChat(Guid AUTH_Users_ID);
    }
}
