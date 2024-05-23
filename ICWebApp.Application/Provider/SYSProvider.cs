using ICWebApp.Application.Interface.Provider;
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
    public class SYSProvider : ISYSProvider
    {
        private IUnitOfWork _unitOfWork;
        public SYSProvider(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        public async Task<bool> SetLog(SYS_Log Data)
        {
            await _unitOfWork.Repository<SYS_Log>().InsertOrUpdateAsync(Data);

            return true;
        }
    }
}
