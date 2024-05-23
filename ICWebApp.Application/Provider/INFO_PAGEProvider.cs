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
using ICWebApp.DataStore.MSSQL.Repositories;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Application.Provider
{
    public class INFO_PAGEProvider : IINFO_PAGEProvider
    {
        private IUnitOfWork _unitOfWork;
        private ILANGProvider _langProvider;
        private ISessionWrapper _sessionWrapper;

        public INFO_PAGEProvider(IUnitOfWork _unitOfWork, ILANGProvider langProvider, ISessionWrapper sessionWrapper)
        {
            this._unitOfWork = _unitOfWork;
            this._langProvider = langProvider;
            this._sessionWrapper = sessionWrapper;
        }

        public async Task<List<INFO_Page>> GetPages(Guid menuID, Guid? parentID, String? languageCode , string pageUrl)
        {            
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (languageCode != null)
            {
                Language = _langProvider.GetLanguageByCode(languageCode);
            }

            Guid municipalityID = Guid.NewGuid();

            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                municipalityID = _sessionWrapper.AUTH_Municipality_ID ?? Guid.NewGuid();
            }

            var pages = await _unitOfWork.Repository<INFO_Page>().Where(p => p.MenuID == menuID && p.Language == Language.Code && p.ParentID == parentID && p.SubPageUrl == pageUrl && p.MunicipalityID == municipalityID).OrderByDescending(o => o.SortOrder)
                                               .ThenByDescending(o => o.UpdateDate)
                                               .ToListAsync();

            return pages;
        }
        public async Task<List<INFO_Page>> GetInfoPagesList(Guid menuID, Guid? parentID)
        {
            Guid municipalityID = Guid.NewGuid();

            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                municipalityID = _sessionWrapper.AUTH_Municipality_ID ?? Guid.NewGuid();
            }

            var pages = await _unitOfWork.Repository<INFO_Page>().Where(p => p.MenuID == menuID && p.ParentID == parentID && p.MunicipalityID == municipalityID).OrderByDescending(o => o.SortOrder).ThenByDescending(o => o.UpdateDate).ToListAsync();

            return pages;
        }
        public async Task<List<INFO_Template>> GetTemplateList(Guid? externalKey)
        {
            Guid municipalityID = Guid.NewGuid();
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                municipalityID = _sessionWrapper.AUTH_Municipality_ID ?? Guid.NewGuid();
            }

            var pages = await _unitOfWork.Repository<INFO_Template>().Where(p => (externalKey == null || (externalKey != null && p.ExternalKey == externalKey)) && p.MunicipalityID == municipalityID)
                                                                     .OrderByDescending(o => o.SortOrder).ThenByDescending(o => o.UpdateDate).ToListAsync();

            return pages;
        }
        public async Task<List<INFO_Template_Type>> GetTemplateTypeList()
        {
            var pages = await _unitOfWork.Repository<INFO_Template_Type>().ToListAsync();

            return pages;
        }
        public async Task<INFO_Page> GetDetailPage(Guid newspageID)
        {
            Guid Lang = LanguageSettings.German;

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {

                Lang = Language.ID;
            }

            var pages = await _unitOfWork.Repository<INFO_Page>().Where(p => p.ID == newspageID).FirstOrDefaultAsync();

            return pages;
        }
        public async Task<INFO_Template> GetDetailTemplate(Guid newspageID)
        {
            Guid Lang = LanguageSettings.German;

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {

                Lang = Language.ID;
            }

            var pages = await _unitOfWork.Repository<INFO_Template>().Where(p => p.id == newspageID).FirstOrDefaultAsync();

            return pages;
        }
        public async Task<INFO_Message> GetDetailMessage(Guid newspageID)
        {
            Guid Lang = LanguageSettings.German;

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {

                Lang = Language.ID;
            }

            var pages = await _unitOfWork.Repository<INFO_Message>().Where(p => p.id == newspageID).FirstOrDefaultAsync();

            return pages;
        }
        public async Task<bool> UpdateTemplate(INFO_Template item)
        {
            await _unitOfWork.Repository<INFO_Template>().InsertOrUpdateAsync(item);

            return true;
        }
        public async Task<bool> UpdateMessage(INFO_Message item)
        {
            await _unitOfWork.Repository<INFO_Message>().InsertOrUpdateAsync(item);

            return true;
        }
        public async Task<bool> UpdatePage(INFO_Page item)
        {
            await _unitOfWork.Repository<INFO_Page>().InsertOrUpdateAsync(item);

            return true;
        }
        public async Task<bool> RemoveTemplate(Guid ID)
        {
            return await _unitOfWork.Repository<INFO_Template>().DeleteAsync(ID);
        }
    }
}
