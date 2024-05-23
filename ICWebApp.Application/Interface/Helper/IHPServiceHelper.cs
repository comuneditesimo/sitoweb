using ICWebApp.Domain.Models.Homepage.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IHPServiceHelper
    {
        public string? ServiceScript { get; set; }
        public event Action OnServiceScriptChanged;

        public List<ServiceItem> GetServices(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
        public Task<List<ServiceItem>> GetServicesByThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID);
        public List<ServiceKategorieItems> GetKategories(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
        public Task<List<ServiceAuthorityItems>> GetAuthorities(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid FORM_Definition_Type_ID);
        public Task<bool> CreateServiceScript(Guid AUTH_Municipality_ID, MetaItem Item);
        public Task<bool> InjectServiceScript();
    }
}
