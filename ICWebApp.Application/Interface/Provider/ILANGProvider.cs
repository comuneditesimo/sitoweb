using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ILANGProvider
    {
        public Task<List<LANG_Languages>?> GetAll();
        public LANG_Languages GetLanguageByCode(string LanguageCode);
        public string GetCodeByLanguage(Guid LANG_Language_ID);
        public Task<bool> SetLanguage(string LanguageCode);
        public Task<bool> LanguageInitialized(); 
        public Guid GetCurrentLanguageID();
        public Task<bool> HasLadinisch();
        public bool HasLadinischSync();
        public string GetLanguage2DigitCode();
    }
}
