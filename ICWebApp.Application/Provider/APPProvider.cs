using ICWebApp.Application.Cache;
using ICWebApp.Application.Interface.Cache;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Provider
{
    public class APPProvider : IAPPProvider
    {
        private IUnitOfWork _unitOfWork;
        private IAPPProviderCache _appProviderCache;
        private IAUTHProviderCache _authProviderCache;
        private NavigationManager _NavManager;

        public APPProvider(IUnitOfWork _unitOfWork, IAPPProviderCache _appProviderCache, IAUTHProviderCache authProviderCache, NavigationManager _NavManager)
        {
            this._unitOfWork = _unitOfWork;
            this._appProviderCache = _appProviderCache;
            this._authProviderCache = authProviderCache;
            this._NavManager = _NavManager;
        }
        public async Task<List<APP_Applications_Url>> GetAppUrl()
        {
            var data = _appProviderCache.Get();

            if (!data.Any())
            {
                data = await _unitOfWork.Repository<APP_Applications_Url>().ToListAsync();

                _appProviderCache.Set(data);
            }

            return data;
        }
        public async Task<bool> HasApplicationAsync(Guid MunicipalityID, Guid ApplicationID, bool WithPreparation = false)
        {
            if(ApplicationID == Applications.Homepage && _NavManager.BaseUri.Contains("services.tisens"))   //TISENS 
            {
                return true;
            }

            var data = _authProviderCache.GetApps();

            if (!data.Any())
            {
                data = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync();

                _authProviderCache.SetApps(data);
            }

            if (!WithPreparation)
            {
                var right = data.FirstOrDefault(p => p.AUTH_Municipality_ID == MunicipalityID && p.APP_Application_ID == ApplicationID && p.Deaktivated == null);

                if (right != null)
                {
                    return true;
                }
            }
            else
            {
                var right = data.FirstOrDefault(p => p.AUTH_Municipality_ID == MunicipalityID && p.APP_Application_ID == ApplicationID && (p.Deaktivated == null || p.InPreparation == true));

                if (right != null)
                {
                    return true;
                }
            }

            return false;
        }
        public bool HasApplication(Guid MunicipalityID, Guid ApplicationID, bool WithPreparation = false)
        {
            var data = _authProviderCache.GetApps();

            if (!data.Any())
            {
                data = _unitOfWork.Repository<AUTH_MunicipalityApps>().ToList();

                _authProviderCache.SetApps(data);
            }

            if (!WithPreparation)
            {
                var right = data.FirstOrDefault(p => p.AUTH_Municipality_ID == MunicipalityID && p.APP_Application_ID == ApplicationID && p.Deaktivated == null);

                if (right != null)
                {
                    return true;
                }
            }
            else
            {
                var right = data.FirstOrDefault(p => p.AUTH_Municipality_ID == MunicipalityID && p.APP_Application_ID == ApplicationID && (p.Deaktivated == null || p.InPreparation == true));

                if (right != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
