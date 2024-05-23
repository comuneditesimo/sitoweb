using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ITEXTProvider
    {
        public string Get(string Code, Guid? LanguageID = null);
        public string GetOrReturnCode(string Code);
        public string SelectLanguage(string? GermanText, string? ItalianText);
        public string GetOrCreate(string Code, Guid? LanguageID = null);
        public TEXT_SystemTexts? Get(Guid? TextID);
        public TEXT_SystemTexts? Set(string Code, string Text, Guid LanguageID);
        public Task<TEXT_SystemTexts?> Set(TEXT_SystemTexts Data);
        public Task<List<V_Translations>> GetTexts();
        public Task<List<V_TEXT_Template>> GetTemplates(Guid AUTH_Municipality_ID, string ExternalContext, string ExternalID, bool? IsDocument = null);
        public Task<TEXT_Template?> GetTemplate(Guid ID);
        public Task<V_TEXT_Template?> GetTemplate(Guid AUTH_Municipality_ID, Guid LanguageID, string ExternalContext, string ExternalID);
        public Task<TEXT_Template?> SetTemplate(TEXT_Template Data);
        public Task<List<TEXT_Template_Extended>> GetTemplateExtended(Guid TEXT_Template_ID);
        public Task<TEXT_Template_Extended?> SetTemplateExtended(TEXT_Template_Extended Data);
        public Task<TEXT_Template_Default?> GetDefaultTemplate(Guid LANG_Language_ID, bool? IsDocument = null);
        public Task<bool> RemoveTemplate(Guid TEXT_Template_ID);
        public Task<List<TEXT_Template_Keyword>> GetTemplateKeywords(string ExternalContext, Guid LANG_Language_ID);
        public Task<List<TEXT_Template_Keyword>> GetTemplateKeywords(string ExternalContext);
        public Task<string> ReplaceGeneralKeyWords(Guid AUTH_Users_ID, string Text);
    }
}
