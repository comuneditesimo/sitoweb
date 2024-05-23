using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using Microsoft.AspNetCore.Components;
using Stripe;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Villagelife
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
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<V_HOME_Appointment> Events = new List<V_HOME_Appointment>();
        private List<V_HOME_Location> Locations = new List<V_HOME_Location>();
        private List<V_HOME_Municipal_Newsletter> Newsletter = new List<V_HOME_Municipal_Newsletter>();
        private V_HOME_Municipality? Municipality;
        private List<HOME_Municipality_Images> FileImageList = new List<HOME_Municipality_Images>();
        private List<V_HOME_Thematic_Sites>? ThematicSites;
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HP_MAINMENU_DORFLEBEN");
            SessionWrapper.PageSubTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.VillageLife);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageDescription = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageDescription = TextProvider.Get("HP_MAINMENU_DORFLEBEN_DESCRIPTION");
                }
            }

            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.ShowQuestioneer = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Villagelife", "HP_MAINMENU_DORFLEBEN", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null) 
            {
                var app = await HomeProvider.GetAppointmentsWithDatesByType(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), HOMEAppointmentTypes.Event);
                Events = app.Where(p => p.DateFrom != null && p.DateFrom >= DateTime.Now).ToList();
                Locations = await HomeProvider.GetLocations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                Newsletter = await HomeProvider.GetMunicipalNewsletters(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), 3);
                FileImageList = await HomeProvider.GetMunicipality_Images(SessionWrapper.AUTH_Municipality_ID.Value);

                var muns = await HomeProvider.GetMunicipalities(SessionWrapper.AUTH_Municipality_ID.Value);

                Municipality = muns.FirstOrDefault();
                ThematicSites = await HomeProvider.GetThematicsitesByType(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), HOMEThematicSiteTypes.Villagelife);
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void GoToEvents()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("hp/Event");
            StateHasChanged();
        }
        private void GoToLocations()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("hp/Location");
            StateHasChanged();
        }
        private void GoToMunicipalNewsLetter()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("hp/Newsletter");
            StateHasChanged();
        }
        private void GoToUrl(string Url)
        {
            if (!NavManager.Uri.Contains(Url))
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
}
