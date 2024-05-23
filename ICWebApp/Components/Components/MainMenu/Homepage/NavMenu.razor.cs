using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using Microsoft.JSInterop;
using ICWebApp.Domain.Models.Homepage.MainMenu;

namespace ICWebApp.Components.Components.MainMenu.Homepage
{
    public partial class NavMenu
    {
        [Inject] ICONFProvider ConfProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IAUTHProvider AUTHProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IJSRuntime JsRuntime { get; set; }
        public List<MainMenuItem> MainMenuList { get; set; }
        public List<MainMenuItem> SecondaryMenuList { get; set; }
        private AUTH_Municipality? Municipality { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                MainMenuList = new List<MainMenuItem>();

                MainMenuList.Add(new MainMenuItem()
                {
                    TEXT_SystemTextsCode = "HP_MAINMENU_VERWALTUNG",
                    DataElement = "management",
                    Url = "/Hp/Administration"
                });
                MainMenuList.Add(new MainMenuItem()
                {
                    TEXT_SystemTextsCode = "HP_MAINMENU_NEWS",
                    DataElement = "news",
                    Url = "/Hp/News"
                });
                MainMenuList.Add(new MainMenuItem()
                {
                    TEXT_SystemTextsCode = "HP_MAINMENU_DIENSTE",
                    DataElement = "all-services",
                    Url = "/Hp/Services"
                });
                MainMenuList.Add(new MainMenuItem()
                {
                    TEXT_SystemTextsCode = "HP_MAINMENU_DORFLEBEN",
                    DataElement = "live",
                    Url = "/Hp/Villagelife"
                });

                SecondaryMenuList = new List<MainMenuItem>();

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var themes = HomeProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), true);

                    foreach(var t in themes.OrderBy(p => p.Title).ToList())
                    {
                        SecondaryMenuList.Add(new MainMenuItem()
                        {
                            Text = t.Title,
                            Url = "/Hp/Theme/" + t.ID,
                            DataElement = "topic-element"
                        });
                    }
                }

                SecondaryMenuList.Add(new MainMenuItem(){
                    TEXT_SystemTextsCode = "HP_SECONDMENU_ALL_TOPICS",
                    Url = "/Hp/Theme",
                    Class = "menu-bold",
                    DataElement = "all-topics"
                });
            }

            StateHasChanged();

            await base.OnInitializedAsync();
        }

        private async void CloseNavMenu()
        {
            await JsRuntime.InvokeVoidAsync("navmenu_ToggleVisibility");
        }
    }
}
