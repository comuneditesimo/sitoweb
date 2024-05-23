using ICWebApp.Application.Cache.Homepage;
using ICWebApp.Application.Interface.Cache.Homepage;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class HPServiceHelper : IHPServiceHelper
    {
        private IFORMDefinitionProvider _FormDefinitionProvider;
        private IAUTHProvider _AUTHProvider;
        private ITEXTProvider _TextProvider;
        private IAPPProvider _AppProvider;
        private ICANTEENProvider _CanteenProvider;
        private IRoomProvider _RoomProvider;
        private ILANGProvider _langProvider;
        private IServiceHelperCache _serviceHelperCache;
        private IJSRuntime _jsRuntime;

        private string? _serviceScript;
        public event Action OnServiceScriptChanged;

        public HPServiceHelper(IFORMDefinitionProvider _FormDefinitionProvider, IAUTHProvider _AUTHProvider, ITEXTProvider _TextProvider, 
                               IAPPProvider _AppProvider, ICANTEENProvider _CanteenProvider, IRoomProvider _RoomProvider, 
                               ILANGProvider _langProvider, IServiceHelperCache _serviceHelperCache, IJSRuntime _jsRuntime)
        {
            this._FormDefinitionProvider = _FormDefinitionProvider;
            this._AUTHProvider = _AUTHProvider;
            this._TextProvider = _TextProvider;
            this._AppProvider = _AppProvider;
            this._RoomProvider = _RoomProvider;
            this._CanteenProvider = _CanteenProvider;
            this._langProvider = _langProvider;
            this._serviceHelperCache = _serviceHelperCache;
            this._jsRuntime = _jsRuntime;
        }
        public List<ServiceItem> GetServices(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            var cachedData = _serviceHelperCache.GetServices(AUTH_Municipality_ID, LANG_Language_ID);

            if (cachedData.Any())
            {
                return cachedData;
            }

            var Items = new List<ServiceItem>();
           
            //Anträge
            var data = _FormDefinitionProvider.GetVDefinitionListOnlineSync(AUTH_Municipality_ID, LANG_Language_ID);

            var definitions = data.Where(p => p.FORM_Definition_Category_ID == FORMCategories.Applications && p.FORM_Defintion_Type_ID != null && p.Enabled == true).ToList();

            foreach (var d in definitions)
            {
                if (!string.IsNullOrEmpty(d.Name))
                {
                    var item = new ServiceItem();

                    item.ID = d.ID;
                    item.Title = d.Name;
                    item.Url = "/Form/Detail/" + d.ID;
                    item.Kategorie = d.Type;
                    item.Kategorie_Url = "/Hp/Type/Services/" + d.FORM_Defintion_Type_ID;
                    item.KategorieID = d.FORM_Defintion_Type_ID;
                    item.Auth_Municipality_ID = AUTH_Municipality_ID;
                    item.LANG_Language_ID = LANG_Language_ID;

                    if (d.Highlight != null)
                    {
                        item.Highlight = d.Highlight.Value;
                    }

                    item.ShortText = d.ShortText;
                    Items.Add(item);
                }
            }

            //MELDUNGEN
            if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Mantainences))
            {
                var item = new ServiceItem();

                item.ID = HomepageServices.Maintenance;
                item.Title = _TextProvider.Get("FRONTEND_MANTAINANCE_TITLE");
                item.Url = "/Mantainance/Landing";
                item.Kategorie = _TextProvider.Get("FORM_DEFINTION_TYPE_ENVIROMENT");
                item.Kategorie_Url = "/Hp/Type/Services/" + HomepageCategoryServices.Maintenance;
                item.KategorieID = HomepageCategoryServices.Maintenance;
                item.ShortText = _TextProvider.Get("HOMEPAGE_FRONTEND_MAINTENANCE_DESCRIPTION");
                item.Auth_Municipality_ID = AUTH_Municipality_ID;
                item.LANG_Language_ID = LANG_Language_ID;

                Items.Add(item);
            }

            //MENSA
            if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Canteen))
            {
                var item = new ServiceItem();

                item.ID = HomepageServices.Mensa;
                item.Title = _TextProvider.Get("MAINMENU_CANTEEN");
                item.Url = "/Canteen";
                item.Kategorie = _TextProvider.Get("FORM_DEFINTION_TYPE_EDUCATION");
                item.Kategorie_Url = "/Hp/Type/Services/" + HomepageCategoryServices.Mensa;
                item.KategorieID = HomepageCategoryServices.Mensa;

                var pagesubTitle = _AUTHProvider.GetCurrentPageSubTitle(AUTH_Municipality_ID, LANG_Language_ID, (int)PageSubtitleType.Canteen);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    item.ShortText = pagesubTitle.Description;
                }
                else
                {
                    item.ShortText = _TextProvider.Get("MAINMENU_CANTEEN_SERVICE_DESCRIPTION");
                }                

                item.Auth_Municipality_ID = AUTH_Municipality_ID;
                item.LANG_Language_ID = LANG_Language_ID;

                Items.Add(item);    
            }

            //ROOM
            if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Rooms))
            {
                var item = new ServiceItem();

                item.ID = HomepageCategoryServices.Rooms;
                item.Title = _TextProvider.Get("FRONTEND_BOOKING_TITLE");
                item.Url = "/Rooms";
                item.Kategorie = _TextProvider.Get("FORM_DEFINTION_TYPE_CULTURE");
                item.Kategorie_Url = "/Hp/Type/Services/" + HomepageCategoryServices.Rooms;
                item.KategorieID = HomepageCategoryServices.Rooms;

                var pagesubTitle = _AUTHProvider.GetCurrentPageSubTitle(AUTH_Municipality_ID, LANG_Language_ID, (int)PageSubtitleType.Rooms);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    item.ShortText = pagesubTitle.Description;
                }
                else
                {
                    item.ShortText = _TextProvider.Get("MAINMENU_ROOMS_SERVICE_DESCRIPTION");
                }     

                item.Auth_Municipality_ID = AUTH_Municipality_ID;
                item.LANG_Language_ID = LANG_Language_ID;

                Items.Add(item);
            }

            _serviceHelperCache.SetServices(Items);

            return Items;
        }
        public async Task<List<ServiceItem>> GetServicesByThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID)
        {
            var Items = new List<ServiceItem>();

            //Anträge  
            var data = await _FormDefinitionProvider.GetVDefinitionListOnlineByTheme(AUTH_Municipality_ID, LANG_Language_ID, HOME_Theme_ID);

            var definitions = data.Where(p => p.FORM_Definition_Category_ID == FORMCategories.Applications && p.FORM_Defintion_Type_ID != null && p.Enabled == true).ToList();

            foreach (var d in definitions)
            {
                if (!string.IsNullOrEmpty(d.Name))
                {
                    var item = new ServiceItem();

                    item.ID = d.ID;
                    item.Title = d.Name;
                    item.Url = "/Form/Detail/" + d.ID;
                    item.Kategorie = d.AmtName;
                    item.Kategorie_Url = "/Hp/Type/Services/" + d.AUTH_Authority_ID;
                    item.KategorieID = d.AUTH_Authority_ID;
                    item.Auth_Municipality_ID = AUTH_Municipality_ID;
                    item.LANG_Language_ID = LANG_Language_ID;

                    if (d.Highlight != null)
                    {
                        item.Highlight = d.Highlight.Value;
                    }

                    item.ShortText = d.ShortText;
                    Items.Add(item);
                }
            }

            //MENSA
            var mensaHasTheme = await _CanteenProvider.HasTheme(HOME_Theme_ID);

            if (mensaHasTheme)
            {
                if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Canteen))
                {
                    var item = new ServiceItem();

                    item.ID = HomepageCategoryServices.Mensa;
                    item.Title = _TextProvider.Get("MAINMENU_CANTEEN");
                    item.Url = "/Canteen";
                    item.Kategorie = _TextProvider.Get("FORM_DEFINTION_TYPE_EDUCATION");
                    item.Kategorie_Url = "/Hp/Type/Services/" + HomepageCategoryServices.Mensa;
                    item.KategorieID = HomepageCategoryServices.Mensa;

                    var pagesubTitle = _AUTHProvider.GetCurrentPageSubTitle(AUTH_Municipality_ID, LANG_Language_ID, (int)PageSubtitleType.Canteen);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        item.ShortText = pagesubTitle.Description;
                    }
                    else
                    {
                        item.ShortText = _TextProvider.Get("MAINMENU_CANTEEN_SERVICE_DESCRIPTION");
                    }

                    item.Auth_Municipality_ID = AUTH_Municipality_ID;
                    item.LANG_Language_ID = LANG_Language_ID;

                    Items.Add(item);
                }
            }

            //ROOM
            var roomHasTheme = await _RoomProvider.HasTheme(HOME_Theme_ID);

            if (roomHasTheme)
            {
                if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Rooms))
                {
                    var item = new ServiceItem();

                    item.ID = HomepageCategoryServices.Rooms;
                    item.Title = _TextProvider.Get("FRONTEND_BOOKING_TITLE");
                    item.Url = "/Rooms";
                    item.Kategorie = _TextProvider.Get("FORM_DEFINTION_TYPE_CULTURE");
                    item.Kategorie_Url = "/Hp/Type/Services/" + HomepageCategoryServices.Rooms;
                    item.KategorieID = HomepageCategoryServices.Rooms;

                    var pagesubTitle = _AUTHProvider.GetCurrentPageSubTitle(AUTH_Municipality_ID, LANG_Language_ID, (int)PageSubtitleType.Rooms);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        item.ShortText = pagesubTitle.Description;
                    }
                    else
                    {
                        item.ShortText = _TextProvider.Get("MAINMENU_ROOMS_SERVICE_DESCRIPTION");
                    }

                    item.Auth_Municipality_ID = AUTH_Municipality_ID;
                    item.LANG_Language_ID = LANG_Language_ID;

                    Items.Add(item);
                }
            }

            return Items;
        }
        public List<ServiceKategorieItems> GetKategories(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            var cachedData = _serviceHelperCache.GetKategories(AUTH_Municipality_ID, LANG_Language_ID);

            if (cachedData.Any())
            {
                return cachedData;
            }

            var Kategories = new List<ServiceKategorieItems>();

            //ANTRÄGE
            var types = _FormDefinitionProvider.GetTypesSync(LANG_Language_ID);
            var data = _FormDefinitionProvider.GetVDefinitionListOnlineSync(AUTH_Municipality_ID, LANG_Language_ID);
            var definitions = data.Where(p => p.FORM_Definition_Category_ID == FORMCategories.Applications && p.FORM_Defintion_Type_ID != null && p.Enabled == true).ToList();
            var selectedtypes = types.Where(p => definitions.Select(p => p.FORM_Defintion_Type_ID).Distinct().Contains(p.ID)).ToList();

            foreach (var a in selectedtypes)
            {
                if (!string.IsNullOrEmpty(a.Description))
                {
                    var item = new ServiceKategorieItems();

                    item.ID = a.ID;
                    item.Title = a.Type;
                    item.Url = "/Hp/Type/Services/" + a.ID;
                    item.ShortText = a.Description;
                    item.Auth_Municipality_ID = AUTH_Municipality_ID;
                    item.LANG_Language_ID = LANG_Language_ID;

                    Kategories.Add(item);
                }
            }

            //MELDUNGEN

            if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Mantainences))
            {
                if (!Kategories.Select(p => p.Url).Contains("/Hp/Type/Services/" + HomepageCategoryServices.Maintenance))
                {
                    var itemauth = new ServiceKategorieItems();

                    itemauth.ID = HomepageCategoryServices.Maintenance;
                    itemauth.Title = _TextProvider.Get("FORM_DEFINTION_TYPE_ENVIROMENT");
                    itemauth.Url = "/Hp/Type/Services/" + HomepageCategoryServices.Maintenance;
                    itemauth.ShortText = _TextProvider.Get("FORM_DEFINTION_TYPE_ENVIROMENT_DESCRIPTION");
                    itemauth.Auth_Municipality_ID = AUTH_Municipality_ID;
                    itemauth.LANG_Language_ID = LANG_Language_ID;

                    Kategories.Add(itemauth);
                }
            }
            

            //MENSA
            if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Canteen))
            {
                if (!Kategories.Select(p => p.Url).Contains("/Hp/Type/Services/" + HomepageCategoryServices.Mensa))
                {
                    var itemauth = new ServiceKategorieItems();

                    itemauth.ID = HomepageCategoryServices.Mensa;
                    itemauth.Title = _TextProvider.Get("FORM_DEFINTION_TYPE_EDUCATION");
                    itemauth.Url = "/Hp/Type/Services/" + HomepageCategoryServices.Mensa;
                    itemauth.ShortText = _TextProvider.Get("FORM_DEFINTION_TYPE_EDUCATION_DESCRIPTION");
                    itemauth.Auth_Municipality_ID = AUTH_Municipality_ID;
                    itemauth.LANG_Language_ID = LANG_Language_ID;

                    Kategories.Add(itemauth);
                }
            }
            //ROOMS
            if (_AppProvider.HasApplication(AUTH_Municipality_ID, Applications.Rooms))
            {
                if (!Kategories.Select(p => p.Url).Contains("/Hp/Type/Services/" + HomepageCategoryServices.Rooms))
                {
                    var itemauth = new ServiceKategorieItems();

                    itemauth.ID = HomepageCategoryServices.Rooms;
                    itemauth.Title = _TextProvider.Get("FORM_DEFINTION_TYPE_CULTURE");
                    itemauth.Url = "/Hp/Type/Services/" + HomepageCategoryServices.Rooms;
                    itemauth.ShortText = _TextProvider.Get("FORM_DEFINTION_TYPE_CULTURE_DESCRIPTION");
                    itemauth.Auth_Municipality_ID = AUTH_Municipality_ID;
                    itemauth.LANG_Language_ID = LANG_Language_ID;

                    Kategories.Add(itemauth);
                }
            }

            _serviceHelperCache.SetKategories(Kategories);

            return Kategories;
        }
        public async Task<List<ServiceAuthorityItems>> GetAuthorities(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid FORM_Definition_Type_ID)
        {
            var auth = new List<ServiceAuthorityItems>();

            //ANTRÄGE
            var types = await _AUTHProvider.GetHomeAuthorityList(AUTH_Municipality_ID, LANG_Language_ID);
            var data = await _FormDefinitionProvider.GetVDefinitionListOnline(AUTH_Municipality_ID, LANG_Language_ID);
            var definitions = data.Where(p => p.FORM_Defintion_Type_ID == FORM_Definition_Type_ID && p.FORM_Definition_Category_ID == FORMCategories.Applications && p.Enabled == true).ToList();
            var selectedtypes = types.Where(p => definitions.Select(p => p.AUTH_Authority_ID).Distinct().Contains(p.ID)).ToList();

            foreach (var a in selectedtypes)
            {
                if (!string.IsNullOrEmpty(a.Title))
                {
                    var item = new ServiceAuthorityItems();

                    item.ID = a.ID;
                    item.Title = a.Title;
                    item.Url = "/Hp/Authority/" + a.ID;
                    item.Auth_Municipality_ID = AUTH_Municipality_ID;

                    auth.Add(item);
                }
            }

            return auth;
        }
        public async Task<bool> CreateServiceScript(Guid AUTH_Municipality_ID, MetaItem Item)
        {
            var municipality = await _AUTHProvider.GetMunicipality(AUTH_Municipality_ID);

            if (municipality != null)
            {
                var mun = _TextProvider.Get("APPLICANT_MUNICIPALITY") + " " + municipality.Name;

                var content = new StringBuilder();

                content.AppendLine("{");
                //content.AppendLine("\"@context\": \"https://schema.org\",");
                //content.AppendLine("\"@type\": \"GovernmentService\",");
                content.AppendLine("\"name\": \"" + Item.Name + "\",");
                content.AppendLine("\"serviceType\": \"" + Item.Type + "\",");
                content.AppendLine("\"serviceOperator\": {");
                //content.AppendLine("\"@type\": \"GovernmentOrganization\",");
                content.AppendLine("\"name\": \"" + mun + "\"");
                content.AppendLine("},");
                content.AppendLine("\"areaServed\": {");
                //content.AppendLine("\"@type\": \"AdministrativeArea\",");
                content.AppendLine("\"name\": \"" + mun + "\"");
                content.AppendLine("},");
                content.AppendLine("\"audience\": {");
                //content.AppendLine("\"@type\": \"Audience\",");
                content.AppendLine("\"audienceType\": \"" + Item.AudienceType + "\"");
                content.AppendLine("},");
                content.AppendLine("\"availableChannel\": {");
                //content.AppendLine("\"@type\": \"ServiceChannel\",");
                content.AppendLine("\"name\": \"Dove rivolgersi\",");
                content.AppendLine("\"serviceUrl\": \"" + Item.Url + "\",");
                content.AppendLine("\"availableLanguage\": {");
                //content.AppendLine("\"@type\": \"Language\",");

                if (_langProvider.GetCurrentLanguageID() == LanguageSettings.German)
                {
                    content.AppendLine("\"name\": \"Deutsch\",");
                    content.AppendLine("\"alternateName\": \"de\"");
                }
                else
                {
                    content.AppendLine("\"name\": \"Italiano\",");
                    content.AppendLine("\"alternateName\": \"it\"");
                }
                content.AppendLine("},");

                if (!string.IsNullOrEmpty(Item.Authority))
                {
                    content.AppendLine("\"serviceLocation\": {");
                    //content.AppendLine("\"@type\": \"Place\",");
                    content.AppendLine("\"name\": \"" + Item.Authority + "\",");
                    content.AppendLine("\"address\": {");
                    //content.AppendLine("\"@type\": \"PostalAddress\",");

                    var munFooter = _AUTHProvider.GetMunicipalityFooter(AUTH_Municipality_ID, _langProvider.GetCurrentLanguageID());

                    if (!string.IsNullOrEmpty(Item.Address))
                    {
                        content.AppendLine("\"streetAddress\": \"" + Item.Address + "\",");
                    }
                    else
                    {
                        if (munFooter != null)
                        {
                            content.AppendLine("\"streetAddress\": \"" + munFooter.Address + "\",");
                        }
                    }

                    if (!string.IsNullOrEmpty(Item.Cap))
                    {
                        content.AppendLine("\"postalCode\": \"" + Item.Cap + "\",");
                    }
                    else
                    {
                        if (munFooter != null)
                        {
                            content.AppendLine("\"postalCode\": \"" + munFooter.Cap + "\",");
                        }
                    }

                    content.AppendLine("\"addressLocality\": \"" + municipality.Name + "\",");

                    content.AppendLine("}");
                    content.AppendLine("}");
                }
                content.AppendLine("}");
                content.AppendLine("}");

                ServiceScript = content.ToString();

                return true;
            }

            return false;
        }
        public async Task<bool> InjectServiceScript()
        {
            await _jsRuntime.InvokeVoidAsync("injectMetaTag", this.ServiceScript);

            return true;
        }
        public string? ServiceScript
        {
            get
            {
                return _serviceScript;
            }
            set
            {
                _serviceScript = value;
                NotifyOnServiceScriptChanged();
            }
        }
        private void NotifyOnServiceScriptChanged() => OnServiceScriptChanged?.Invoke();
    }
}
