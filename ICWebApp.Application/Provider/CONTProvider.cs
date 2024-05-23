using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
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
    public class CONTProvider : ICONTProvider
    {
        private IUnitOfWork _unitOfWork;
        public CONTProvider(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }
        public async Task<CONT_Contact?> GetContact(Guid ID)
        {
            return await _unitOfWork.Repository<CONT_Contact>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<V_CONT_Contact>> GetContacts(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<V_CONT_Contact>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
        }
        public async Task<bool> RemoveContact(Guid ID)
        {
            return await _unitOfWork.Repository<CONT_Contact>().DeleteAsync(ID);
        }
        public async Task<CONT_Contact?> SetContact(CONT_Contact Data)
        {
            return await _unitOfWork.Repository<CONT_Contact>().InsertOrUpdateAsync(Data);
        }
    }
}
