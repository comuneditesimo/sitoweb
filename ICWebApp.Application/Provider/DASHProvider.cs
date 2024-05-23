using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Provider
{
    public class DASHProvider : IDASHProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionWrapper _sessionWrapper;
        private readonly ILANGProvider _langProvider;

        public DASHProvider(IUnitOfWork _unitOfWork, ISessionWrapper _sessionWrapper, ILANGProvider _langProvider)
        {
            this._sessionWrapper = _sessionWrapper;
            this._unitOfWork = _unitOfWork;
            this._langProvider = _langProvider;
        }
        public List<V_DASH_UserChat> GetDashUserChat(Guid AUTH_Users_ID)
        {            
            return _unitOfWork.Repository<V_DASH_UserChat>().Where(p => p.Responsible_AUTH_Users_ID == AUTH_Users_ID && p.LANG_Language_ID == _langProvider.GetCurrentLanguageID()).ToList();
        }
    }
}
