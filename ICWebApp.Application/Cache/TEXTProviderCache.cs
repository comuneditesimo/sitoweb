using ICWebApp.Application.Interface.Cache;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Cache
{
    public class TEXTProviderCache : ITEXTProviderCache
    {
        public HashSet<TEXT_SystemTexts>? Cache { get; set; }

        public void Clear()
        {
            if (Cache != null)
                Cache.Clear();
        }

        public TEXT_SystemTexts? Get(string Code, Guid LANG_Language_ID)
        {
            if (Cache == null)
                return null;

            return Cache.FirstOrDefault(p => p.Code == Code && p.LANG_LanguagesID == LANG_Language_ID);
        }

        public void Save(TEXT_SystemTexts Data)
        {
            if(Cache == null)
            {
                Cache = new HashSet<TEXT_SystemTexts>();
            }

            var existing = Get(Data.Code, Data.LANG_LanguagesID);

            if(existing == null)
                Cache.Add(Data);
        }
    }
}
