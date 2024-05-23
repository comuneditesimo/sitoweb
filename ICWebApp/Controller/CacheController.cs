using ICWebApp.Application.Interface.Cache;
using ICWebApp.Application.Interface.Cache.Homepage;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;

namespace ICWebApp.Controller
{
    [Route("[controller]/[action]")]
    public class CacheController : ControllerBase
    {
        private readonly ITEXTProviderCache _textProviderCache;
        private readonly ILANGProviderCache _langProviderCache;
        private readonly IAUTHProviderCache _authProviderCache;
        private readonly IAPPProviderCache _appProviderCache;
        private readonly IServiceHelperCache _serviceHelperCache;

        public CacheController(ITEXTProviderCache TextProviderCache,
                                ILANGProviderCache LangProviderCache,
                                IAUTHProviderCache AuthProviderCache,
                                IAPPProviderCache AppProviderCache,
                                IServiceHelperCache ServiceHelperCache)
        {
            _textProviderCache = TextProviderCache;
            _langProviderCache = LangProviderCache;
            _authProviderCache = AuthProviderCache;
            _appProviderCache = AppProviderCache;
            _serviceHelperCache = ServiceHelperCache;
        }
        public bool ClearAllCaches()
        {
            this.ClearTextCache();
            this.ClearLanguageCache();
            this.ClearAuthenticationCache();
            this.ClearApplicationCache();
            this.ClearServiceHelperCache();

            return true;
        }
        public bool ClearTextCache()
        {
            _textProviderCache.Clear();

            return true;
        }
        public bool ClearLanguageCache()
        {
            _langProviderCache.Clear();

            return true;
        }
        public bool ClearAuthenticationCache()
        {
            _authProviderCache.ClearAll();

            return true;
        }
        public bool ClearApplicationCache()
        {
            _appProviderCache.Clear();

            return true;
        }
        public bool ClearServiceHelperCache()
        {
            _serviceHelperCache.ClearAll();

            return true;
        }
    }
}
