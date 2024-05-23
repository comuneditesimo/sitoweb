using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.DataStore.MSSQL.Repositories;
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
    public class METAProvider : IMETAProvider
    {
        private IUnitOfWork _unitOfWork;
        private ILANGProvider _langProvider;

        public METAProvider(IUnitOfWork _unitOfWork, ILANGProvider _langProvider)
        {
            this._unitOfWork = _unitOfWork;
            this._langProvider = _langProvider;
        }

        public async Task<List<META_IstatPaesi>> GetCountries(string SearchTherm)
        {
            return await _unitOfWork.Repository<META_IstatPaesi>().Where(p => (p.NameIT != null && p.NameIT.Contains(SearchTherm)) || (p.NameEN != null && p.NameEN.Contains(SearchTherm))).ToListAsync();
        }
        public async Task<List<META_IstatComuni>> GetMunicipalities()
        {
            List<META_IstatComuni> _municipalities = await _unitOfWork.Repository<META_IstatComuni>().ToListAsync();
            return await FillExtendedFields(_municipalities);
        }
        public async Task<META_IstatComuni?> GetMunicipality(Guid ID)
        {
            List<META_IstatComuni> _municipalities = await _unitOfWork.Repository<META_IstatComuni>().Where(p => p.ID == ID).ToListAsync();
            return (await FillExtendedFields(_municipalities)).FirstOrDefault();
        }
        public async Task<META_IstatComuni?> GetMunicipality(string ISTAT)
        {
            List<META_IstatComuni> _municipalities = await _unitOfWork.Repository<META_IstatComuni>().Where(p => p.CodeCatastale == ISTAT).ToListAsync();
            return (await FillExtendedFields(_municipalities)).FirstOrDefault();
        }
        public async Task<META_IstatComuni?> GetMunicipalityByName(string Name)
        {
            List<META_IstatComuni> _municipalities = await _unitOfWork.Repository<META_IstatComuni>().Where(p => p.NameDE != null && p.NameDE.ToLower().Contains(Name.ToLower())).ToListAsync();
            return (await FillExtendedFields(_municipalities)).FirstOrDefault();
        }
        public async Task<META_IstatPaesi?> GetCountry(string ISTAT)
        {
            return await _unitOfWork.Repository<META_IstatPaesi>().FirstOrDefaultAsync(p => p.CodeAT == ISTAT);
        }
        public async Task<List<META_PhonePrefix>> GetPhonePrefixes()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<META_PhonePrefix>().Where(p => p.LANG_Language_ID == Language.ID).ToListAsync();
        }
        public async Task<List<META_Icons>> GetIconList()
        {
            return await _unitOfWork.Repository<META_Icons>().Where().OrderBy(p => p.Description).ToListAsync();
        }
        public async Task<List<META_IstatComuni>> FillExtendedFields(List<META_IstatComuni> Municipalities)
        {
            if (Municipalities.Any())
            {
                Guid _language = LanguageSettings.German;

                LANG_Languages? _actualLanguage = await _unitOfWork.Repository<LANG_Languages>().FirstOrDefaultAsync(p => p.Code == CultureInfo.CurrentCulture.Name);
                if (_actualLanguage != null)
                {
                    _language = _actualLanguage.ID;
                }

                foreach (META_IstatComuni _municipality in Municipalities)
                {
                    if (_language == LanguageSettings.Italian && !string.IsNullOrEmpty(_municipality.NameIT))
                    {
                        _municipality.Name = _municipality.NameIT;
                    }
                    else
                    {
                        _municipality.Name = _municipality.NameDE;
                    }
                }
            }
            return Municipalities;
        }
    }
}
