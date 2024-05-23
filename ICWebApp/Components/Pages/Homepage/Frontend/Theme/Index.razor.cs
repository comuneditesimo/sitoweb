using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Theme
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider {  get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<V_HOME_Theme> Themes = new List<V_HOME_Theme>();
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("FRONTEND_HOMEPAGE_THEME_TITLE");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("FRONTEND_HOMEPAGE_THEME_DESCRIPTION");
            SessionWrapper.ShowHelpSection = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Theme", "HP_SECONDMENU_ALL_TOPICS", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                Themes = HomeProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Themes = HomeProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnParametersSetAsync();
        }
        private void OnClick(V_HOME_Theme Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Theme/" + Item.ID);
            StateHasChanged();
        }
    }
}
