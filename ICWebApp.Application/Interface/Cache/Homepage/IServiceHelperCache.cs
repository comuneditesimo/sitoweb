using ICWebApp.Domain.Models.Homepage.Services;

namespace ICWebApp.Application.Interface.Cache.Homepage
{
    public interface IServiceHelperCache
    {
        public void ClearAll();
        public void ClearServicesCache();
        public void ClearKategoriesCache();
        public List<ServiceItem> GetServices(Guid? AUTH_Municipality_ID, Guid LANG_Language_ID);
        public bool ClearServices(Guid? AUTH_Municipality_ID);
        public void SetServices(List<ServiceItem> Data);
        public List<ServiceKategorieItems> GetKategories(Guid? AUTH_Municipality_ID, Guid LANG_Language_ID);
        public bool ClearKategories(Guid? AUTH_Municipality_ID);
        public void SetKategories(List<ServiceKategorieItems> Data);
    }
}
