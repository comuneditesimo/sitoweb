using ICWebApp.Application.Interface.Cache;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Linq;

namespace ICWebApp.Application.Cache
{
    public class APPProviderCache : IAPPProviderCache
    {
        public HashSet<APP_Applications_Url> Cache = new HashSet<APP_Applications_Url>();

        public void Clear()
        {
            if (Cache != null)
            {
                Cache.Clear();
            }
        }
        public List<APP_Applications_Url> Get()
        {
            return Cache.ToList();
        }
        public void Set(List<APP_Applications_Url> Data)
        {
            Cache = Data.ToHashSet();
        }
    }
}
