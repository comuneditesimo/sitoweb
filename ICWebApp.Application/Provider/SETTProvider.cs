using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.DataStore.MSSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.DataStore;
using System.Globalization;
using System.Collections;
using ICWebApp.Domain.DBModels;
using ICWebApp.DataStore.MSSQL.Repositories;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Application.Provider
{
    public class SETTProvider : ISETTProvider
    {
        private IUnitOfWork _unitOfWork;
        public SETTProvider(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }
        public async Task<SETT_Municipal_Dashboard?> GetDashboard(Guid AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<SETT_Municipal_Dashboard>().FirstOrDefaultAsync(p => p.AUTH_Users_ID == AUTH_Users_ID);
        }
        public async Task<SETT_User_States?> GetUserState(Guid AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<SETT_User_States>().FirstOrDefaultAsync(p => p.AUTH_Users_ID == AUTH_Users_ID);
        }
        public async Task<SETT_Municipal_Dashboard?> SetDashboard(SETT_Municipal_Dashboard? Data)
        {
            if(Data != null)
                return await _unitOfWork.Repository<SETT_Municipal_Dashboard>().InsertOrUpdateAsync(Data);

            return null;
        }
        public async Task<SETT_User_States?> SetUserState(SETT_User_States? Data)
        {
            if (Data != null)
                return await _unitOfWork.Repository<SETT_User_States>().InsertOrUpdateAsync(Data);

            return null;
        }
    }
}
