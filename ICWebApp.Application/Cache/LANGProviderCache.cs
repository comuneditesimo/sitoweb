using ICWebApp.Application.Interface.Cache;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Cache
{
    public class LANGProviderCache : ILANGProviderCache
    {
        public HashSet<LANG_Languages>? Cache { get; set; }

        public void Clear()
        {
            if (Cache != null)            
                Cache.Clear();     
        }
        public List<LANG_Languages>? Get()
        {
            if (Cache == null)
                return null;

            return Cache.ToList();
        }
        public LANG_Languages? Get(string LanguageCode)
        {
            if (Cache == null)
                return null;

            return Cache.FirstOrDefault(p => p.Code == LanguageCode);
        }
        public LANG_Languages? Get(Guid ID)
        {
            if (Cache == null)
                return null;

            return Cache.FirstOrDefault(p => p.ID == ID);
        }

        public void Save(LANG_Languages Data)
        {
            if(Cache == null)
            {
                Cache = new HashSet<LANG_Languages>();
            }

            var existing = Get(Data.ID);

            if(existing == null)
                Cache.Add(Data);
        }
    }
}
