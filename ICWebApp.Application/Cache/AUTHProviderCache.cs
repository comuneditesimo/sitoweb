using ICWebApp.Application.Interface.Cache;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Linq;

namespace ICWebApp.Application.Cache
{
    public class AUTHProviderCache : IAUTHProviderCache
    {
        public HashSet<MunicipalityDomainSelectableItem> Cache = new HashSet<MunicipalityDomainSelectableItem>();
        public HashSet<AUTH_Comunix_Urls> CacheUrls = new HashSet<AUTH_Comunix_Urls>();
        public HashSet<AUTH_MunicipalityApps> CacheApps = new HashSet<AUTH_MunicipalityApps>();
        public HashSet<AUTH_Municipality> CachePrefixes = new HashSet<AUTH_Municipality>();

        public void ClearAll()
        {
            this.ClearCache();
            this.ClearCacheUrls();
            this.ClearCacheApps();
            this.ClearCachePrefixes();
        }
        public void ClearCache()
        {
            if (Cache != null)
            {
                Cache.Clear();
            }
        }
        public void ClearCacheUrls()
        {
            if (CacheUrls != null)
            {
                CacheUrls.Clear();
            }
        }
        public void ClearCacheApps()
        {
            if (CacheApps != null)
            {
                CacheApps.Clear();
            }
        }
        public void ClearCachePrefixes()
        {
            if (CachePrefixes != null)
            {
                CachePrefixes.Clear();
            }
        }
        public List<MunicipalityDomainSelectableItem> Get()
        {
            return Cache.ToList();
        }
        public void Set(List<MunicipalityDomainSelectableItem> Data)
        {
            Cache = Data.ToHashSet();
        }
        public List<AUTH_Comunix_Urls> GetUrls()
        {
            return CacheUrls.ToList();
        }
        public void SetUrls(List<AUTH_Comunix_Urls> Data)
        {
            CacheUrls = Data.ToHashSet();
        }
        public List<AUTH_MunicipalityApps> GetApps()
        {
            return CacheApps.ToList();
        }
        public void SetApps(List<AUTH_MunicipalityApps> Data)
        {
            CacheApps = Data.ToHashSet();
        }
        public List<AUTH_Municipality> GetPrefixes()
        {
            return CachePrefixes.ToList();
        }
        public void SetPrefixes(List<AUTH_Municipality> Data)
        {
            CachePrefixes = Data.ToHashSet();
        }
    }
}
