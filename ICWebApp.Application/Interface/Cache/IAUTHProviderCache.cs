using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Cache
{
    public interface IAUTHProviderCache
    {
        public void ClearAll();
        public void ClearCache();
        public void ClearCacheUrls();
        public void ClearCacheApps();
        public void ClearCachePrefixes();
        public List<MunicipalityDomainSelectableItem> Get();
        public void Set(List<MunicipalityDomainSelectableItem> Data);
        public List<AUTH_Comunix_Urls> GetUrls();
        public void SetUrls(List<AUTH_Comunix_Urls> Data);
        public List<AUTH_MunicipalityApps> GetApps();
        public void SetApps(List<AUTH_MunicipalityApps> Data);
        public List<AUTH_Municipality> GetPrefixes();
        public void SetPrefixes(List<AUTH_Municipality> Data);
    }
}
