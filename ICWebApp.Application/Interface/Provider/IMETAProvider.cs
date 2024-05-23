using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IMETAProvider
    {
        public Task<List<META_IstatPaesi>> GetCountries(string SearchTherm);
        public Task<List<META_IstatComuni>> GetMunicipalities();
        public Task<META_IstatComuni?> GetMunicipality(Guid ID);
        public Task<META_IstatComuni?> GetMunicipality(string ISTAT);
        public Task<META_IstatComuni?> GetMunicipalityByName(string Name);
        public Task<META_IstatPaesi?> GetCountry(string ISTAT);
        public Task<List<META_PhonePrefix>> GetPhonePrefixes(); 
        public Task<List<META_Icons>> GetIconList();
        public Task<List<META_IstatComuni>> FillExtendedFields(List<META_IstatComuni> Municipalities);
    }
}