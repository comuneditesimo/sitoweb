using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Provider
{
    public class AUTHSettingsProvider : IAUTHSettingsProvider
    {
        private IUnitOfWork _unitOfWork;
        private ISessionWrapper _sessionWrapper;

        public AUTHSettingsProvider(IUnitOfWork _unitOfWork, ISessionWrapper _sessionWrapper)
        {
            this._unitOfWork = _unitOfWork;
            this._sessionWrapper = _sessionWrapper;
        }
        public async Task<AUTH_UserSettings> GetSettings(Guid UserID)
        {
            var settings = await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(p => p.AUTH_UsersID == UserID);

            if (settings == null)
            {
                settings = new AUTH_UserSettings();
                settings.ID = Guid.NewGuid();

                if (_sessionWrapper.CurrentUser != null)
                {
                    settings.AUTH_UsersID = _sessionWrapper.CurrentUser.ID;
                }
                if (_sessionWrapper.CurrentMunicipalUser != null)
                {
                    settings.AUTH_UsersID = _sessionWrapper.CurrentMunicipalUser.ID;
                }
            }

            return settings;
        }
        public async Task<bool> SetSettings(AUTH_UserSettings UserSettings)
        {
            await _unitOfWork.Repository<AUTH_UserSettings>().InsertOrUpdateAsync(UserSettings);

            return true;
        }
        public async Task<AUTH_Municipal_Users_Settings> GetMunicipalSettings(Guid AUTH_Municipal_Users_ID)
        {
            var settings = await _unitOfWork.Repository<AUTH_Municipal_Users_Settings>().FirstOrDefaultAsync(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID);

            if (settings == null)
            {
                settings = new AUTH_Municipal_Users_Settings();
                settings.ID = Guid.NewGuid();
                settings.AUTH_Municipal_Users_ID = _sessionWrapper.CurrentMunicipalUser.ID;
            }

            return settings;
        }
        public async Task<bool> SetMunicipalSettings(AUTH_Municipal_Users_Settings UserSettings)
        {
            await _unitOfWork.Repository<AUTH_Municipal_Users_Settings>().InsertOrUpdateAsync(UserSettings);

            return true;
        }
    }
}
