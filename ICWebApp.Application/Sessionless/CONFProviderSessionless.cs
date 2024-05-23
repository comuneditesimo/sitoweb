using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;

namespace ICWebApp.Application.Sessionless
{
    public class CONFProviderSessionless : ICONFProviderSessionless
    {
        private IUnitOfWork _unitOfWork;

        public CONFProviderSessionless(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        public async Task<CONF_Mailer?> GetMailerConfiguration(Guid? ID, Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<CONF_Mailer>().FirstOrDefaultAsync(p => (p.ID == ID || ID == null) && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<CONF_SMS?> GetSMSConfiguration(Guid? ID, Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<CONF_SMS>().FirstOrDefaultAsync(p => (p.ID == ID || ID == null) && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<CONF_Push?> GetPushConfiguration(Guid? ID, Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<CONF_Push>().FirstOrDefaultAsync(p => (p.ID == ID || ID == null) && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<CONF_Mailer_Type?> GetMailerType(Guid? ID)
        {
            return await _unitOfWork.Repository<CONF_Mailer_Type>().FirstOrDefaultAsync(p => (p.ID == ID || ID == null));
        }
    }
}
