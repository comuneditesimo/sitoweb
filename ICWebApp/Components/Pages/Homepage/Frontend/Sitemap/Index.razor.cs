using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Components;
using Stripe;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Sitemap
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IHPServiceHelper HPServiceHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<ServiceKategorieItems>? Categories = new List<ServiceKategorieItems>();
        private List<ServiceItem>? Services = new List<ServiceItem>();
        private List<V_HOME_Article_Type>? Newstypes;
        private List<V_HOME_Theme>? Themes = new List<V_HOME_Theme>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FOOTER_SITEMAP");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FOOTER_SITEMAP_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = true;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Sitemap", "HOMEPAGE_FOOTER_SITEMAP", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Categories = HPServiceHelper.GetKategories(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                Services = HPServiceHelper.GetServices(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                Newstypes = await HomeProvider.GetArticle_Types(LANGProvider.GetCurrentLanguageID());
                Themes = HomeProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void GoToUrl(string Url)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Url);

                StateHasChanged();
            }
        }
    }
}
