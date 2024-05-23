using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Cache;
using System.Runtime.CompilerServices;
using ICWebApp.Application.Cache;
using ICWebApp.Application.Interface.Helper;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Application.Provider
{
    public class LANGProvider : ILANGProvider
    {
        private IUnitOfWork _unitOfWork;
        private ISessionWrapper _sessionWrapper;
        private NavigationManager NavManager;
        private IBusyIndicatorService _busyIndicatorService;
        private ILocalStorageService _localStorageService;
        private IAUTHSettingsProvider _authSettingsProvider;
        private ILANGProviderCache _langProviderCache;
        private ICookieHelper _cookieHelper;

        public LANGProvider(IUnitOfWork _unitOfWork, ISessionWrapper sessionWrapper, NavigationManager NavManager, 
                            IBusyIndicatorService _busyIndicatorService, ILocalStorageService localStorageService, IAUTHSettingsProvider _authSettingsProvider,
                            ILANGProviderCache _langProviderCache, ICookieHelper _cookieHelper)
        {
            this._unitOfWork = _unitOfWork;
            this._sessionWrapper = sessionWrapper;
            this._authSettingsProvider = _authSettingsProvider;
            this.NavManager = NavManager;
            this._busyIndicatorService = _busyIndicatorService;
            this._localStorageService = localStorageService;
            this._langProviderCache = _langProviderCache;
            this._cookieHelper = _cookieHelper;
        }
        public async Task<List<LANG_Languages>?> GetAll()
        {
            var lang = _langProviderCache.Get();

            if (lang != null)
            {
                return lang.OrderBy(p => p.SortingOrder).ToList();
            }

            if(await HasLadinisch())
            {
                lang = await _unitOfWork.Repository<LANG_Languages>().ToListAsync();
            }
            else
            {
                lang = await _unitOfWork.Repository<LANG_Languages>().Where(p => p.ID != LanguageSettings.Ladinisch).ToListAsync();
            }

            foreach (var l in lang)
            {
                if (_langProviderCache.Get(l.ID) == null)
                {
                    _langProviderCache.Save(l);
                }
            }

            return lang.OrderBy(p => p.SortingOrder).ToList();
        }
        public async Task<bool> LanguageInitialized()
        {
            try
            {
                var langInitalized = await _cookieHelper.ReadCookies("Comunix.Localization.Initialized");

                return langInitalized == "True";
            }
            catch
            {
                return true;
            }
        }
        public LANG_Languages GetLanguageByCode(string LanguageCode)
        {
            var lang = _langProviderCache.Get(LanguageCode);

            if (lang != null)
                return lang;

            lang = _unitOfWork.Repository<LANG_Languages>().First(p => p.Code == LanguageCode);

            _langProviderCache.Save(lang);            

            return lang;
        }
        public string GetCodeByLanguage(Guid LANG_Language_ID)
        {
            var lang = _langProviderCache.Get(LANG_Language_ID);

            if (lang != null)
            {
                if (lang.ID == LanguageSettings.German)
                    return "de";
                else if (lang.ID == LanguageSettings.Italian)
                    return "it";
                else if (lang.ID == LanguageSettings.Ladinisch)
                    return "de";
            }

            lang = _unitOfWork.Repository<LANG_Languages>().First(p => p.ID == LANG_Language_ID);

            _langProviderCache.Save(lang);

            if (lang.ID == LanguageSettings.German)
                return "de";
            else if (lang.ID == LanguageSettings.Italian)
                return "it";
            else if (lang.ID == LanguageSettings.Ladinisch)
                return "de";

            return lang.Code;
        }
        public async Task<bool> SetLanguage(string LanguageCode)
        {
            if (!string.IsNullOrEmpty(LanguageCode))
            {
				if (!await LanguageInitialized())
				{
					await _cookieHelper.Write("Comunix.Localization.Initialized", "True");
				}

                var LanguageList = _langProviderCache.Get();

				if (LanguageList == null || !LanguageList.Any())
                {
                    if (await HasLadinisch())
                    {
                        LanguageList = await _unitOfWork.Repository<LANG_Languages>().ToListAsync();
                    }
                    else
                    {
                        LanguageList = await _unitOfWork.Repository<LANG_Languages>().Where(p => p.ID != LanguageSettings.Ladinisch).ToListAsync();
                    }

                    foreach (var lang in LanguageList)
                    {
                        _langProviderCache.Save(lang);
                    }
                }


                if (LanguageList != null)
                {
                    var language = LanguageList.FirstOrDefault(p => p.Code != null && p.Code.Contains(LanguageCode));

                    if (language != null && _sessionWrapper.CurrentUser != null)
                    {
                        var settings = await _authSettingsProvider.GetSettings(_sessionWrapper.CurrentUser.ID);

                        settings.LANG_Languages_ID = language.ID;

                        await _authSettingsProvider.SetSettings(settings);
                    }
                    else if (language != null && _sessionWrapper.CurrentMunicipalUser != null)
                    {
                        var settings = await _authSettingsProvider.GetMunicipalSettings(_sessionWrapper.CurrentMunicipalUser.ID);

                        settings.LANG_Languages_ID = language.ID;

                        await _authSettingsProvider.SetMunicipalSettings(settings);
                    }
                }

                if (!CultureInfo.CurrentCulture.Name.Contains(LanguageCode))
                {
                    _busyIndicatorService.IsBusy = true;
                    var uri = new Uri(NavManager.Uri).GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
                    var uriEscaped = uri.Replace("^", "_").Replace("/", "^");
                    NavManager.NavigateTo($"Culture/Set?culture={LanguageCode}&redirectUri={uriEscaped}", forceLoad: true);
                }
            }

            return false;
        }
        public Guid GetCurrentLanguageID()
        {
            Guid Lang = LanguageSettings.Italian;

            var Language = GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {
                Lang = Language.ID;
            }

            return Lang;
        }
        public async Task<bool> HasLadinisch()
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null) 
            { 
                var mun = await _unitOfWork.Repository<AUTH_Municipality>().FirstOrDefaultAsync(p => p.ID == _sessionWrapper.AUTH_Municipality_ID.Value);

                if (mun != null && mun.HasLadinisch == true)
                {
                    return true;
                } 
            }

            return false;
        }
        public bool HasLadinischSync()
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var mun = _unitOfWork.Repository<AUTH_Municipality>().FirstOrDefault(p => p.ID == _sessionWrapper.AUTH_Municipality_ID.Value);

                if (mun != null && mun.HasLadinisch == true)
                {
                    return true;
                }
            }

            return false;
        }
        public string GetLanguage2DigitCode()
        {
            var lang = GetCurrentLanguageID();

            if (lang == LanguageSettings.German)
                return "DE";
            else if (lang == LanguageSettings.Italian)
                return "IT";
            else if (lang == LanguageSettings.Ladinisch)
                return "DE";

            return "DE";
        }
    }
}
