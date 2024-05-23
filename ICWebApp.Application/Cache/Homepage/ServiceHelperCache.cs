using ICWebApp.Application.Interface.Cache.Homepage;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Cache.Homepage
{
    public class ServiceHelperCache : IServiceHelperCache
    {
        public HashSet<ServiceItem> Services = new HashSet<ServiceItem>();
        public HashSet<ServiceKategorieItems> Kategories = new HashSet<ServiceKategorieItems>();

        public void ClearAll()
        {
            this.ClearServicesCache();
            this.ClearKategoriesCache();
        }
        public void ClearServicesCache()
        {
            if (Services != null)
            {
                Services.Clear();
            }
        }
        public void ClearKategoriesCache()
        {
            if (Kategories != null)
            {
                Kategories.Clear();
            }
        }
        public bool ClearKategories(Guid? AUTH_Municipality_ID)
        {
            Kategories.Clear();

            return true;
        }
        public bool ClearServices(Guid? AUTH_Municipality_ID)
        {
            Services.Clear();

            return true;
        }
        public List<ServiceKategorieItems> GetKategories(Guid? AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return Kategories.Where(p => p.Auth_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToList();
        }
        public List<ServiceItem> GetServices(Guid? AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return Services.Where(p => p.Auth_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToList();
        }
        public void SetKategories(List<ServiceKategorieItems> Data)
        {
            foreach (var item in Data)
            {
                Kategories.Add(item);
            }
        }
        public void SetServices(List<ServiceItem> Data)
        {
            foreach (var item in Data)
            {
                Services.Add(item);
            }
        }
    }
}
