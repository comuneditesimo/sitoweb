using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;

namespace ICWebApp.Application.Provider
{
    public class PRIVProvider : IPRIVProvider
    {
        private IUnitOfWork _unitOfWork;
        private ISessionWrapper _sessionWrapper;
        private ILANGProvider _langProvider;
        private ITEXTProvider _textProvider;

        public PRIVProvider(IUnitOfWork _unitOfWork, ISessionWrapper _sessionWrapper, ILANGProvider _langProvider, ITEXTProvider _textProvider)
        {
            this._unitOfWork = _unitOfWork;
            this._sessionWrapper = _sessionWrapper;
            this._langProvider = _langProvider;
            this._textProvider = _textProvider;
        }
        public async Task<PRIV_Privacy?> GetPrivacy(Guid AUTH_Municipality_ID)
        {
            var r = await _unitOfWork.Repository<PRIV_Privacy>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.PRIV_Privacy_Extended).FirstOrDefaultAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (r != null && Language != null)
            {
                var extended = r.PRIV_Privacy_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null && extended.Description != null && extended.Title != null)
                {
                    r.Title = extended.Title;
                    r.Description = extended.Description;
                }
            }

            return r;
        }
        public async Task<PRIV_Privacy?> SetPrivacy(PRIV_Privacy Data)
        {
            return await _unitOfWork.Repository<PRIV_Privacy>().InsertOrUpdateAsync(Data);
        }
        public async Task<PRIV_Privacy_Extended?> GetPrivacyExtended(Guid ID)
        {
            return await _unitOfWork.Repository<PRIV_Privacy_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<PRIV_Privacy_Extended?> SetPrivacyExtended(PRIV_Privacy_Extended Data)
        {
            return await _unitOfWork.Repository<PRIV_Privacy_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<PRIV_Backend_Privacy>> GetPrivacyDocuments()
        {
            return await _unitOfWork.Repository<PRIV_Backend_Privacy>().ToListAsync();
        }
        public async Task<PRIV_Backend_Privacy?> SetPrivacyDocuments(PRIV_Backend_Privacy Data)
        {
            return await _unitOfWork.Repository<PRIV_Backend_Privacy>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemovePrivacyDocuments(Guid ID)
        {
            return await _unitOfWork.Repository<PRIV_Backend_Privacy>().DeleteAsync(ID);
        }
    }
}
