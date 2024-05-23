using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Components;
using Stripe;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Mediagallery
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

        private List<V_HOME_Media>? Media = new List<V_HOME_Media>();
        private bool ShowFullImage = false;
        private V_HOME_Media? FullScreenImage = null;
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FOOTER_MEDIA");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FOOTER_MEDIA_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = true;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Media", "HOMEPAGE_FOOTER_MEDIA", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Media = await HomeProvider.GetMediaList(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void ShowFullView(V_HOME_Media item)
        {
            FullScreenImage = item;
            ShowFullImage = true;
            StateHasChanged();
        }
        private void HideFullView()
        {
            FullScreenImage = null;
            ShowFullImage = false;
            StateHasChanged();
        }
    }
}
