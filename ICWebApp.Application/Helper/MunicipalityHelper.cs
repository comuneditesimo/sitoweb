using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.Application.Services;

namespace ICWebApp.Application.Helper
{
    public class MunicipalityHelper : IMunicipalityHelper
    {
        private IUnitOfWork _unitOfWork;
        public MunicipalityHelper(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }
        public async Task<string> GetMunicipalBasePath(Guid AUTH_Municipality_ID)
        {
            //URLUPDATE --OK?
            string link = "";

            var municipality = await _unitOfWork.Repository<AUTH_Municipality>().GetByIDAsync(AUTH_Municipality_ID);

            if (municipality != null)
            {
                //var prefix = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == municipality.Prefix_Text_SystemTexts_Code && p.LANG_LanguagesID == LanguageSettings.German);
                var baseUrl = await UrlService.GetDefaultUrlByMunicipalityStatic(_unitOfWork, municipality.ID);
                link = $"https://{baseUrl}";
                
            }

            return link;
        }
    }
}
