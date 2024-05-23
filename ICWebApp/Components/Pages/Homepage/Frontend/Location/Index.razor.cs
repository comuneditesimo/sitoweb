using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Location
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider {  get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<V_HOME_Location> Location = new List<V_HOME_Location>();
        private List<V_HOME_Location_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_LOCATION");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.ShowTitleSepparation = false;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Locations);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageDescription = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_LOCATION_DESCRIPTIONSHORT");
                }
            }

            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.DataElement = "page-name";

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Villagelife", "HP_MAINMENU_DORFLEBEN", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Location", "HOMEPAGE_FRONTEND_LOCATION", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                Location = await HomeProvider.GetLocations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                Types = await HomeProvider.GetLocation_Type(LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void OnTypeClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Location/" + ID);
            StateHasChanged();
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Location.Count() < MaxCounter)
            {
                MaxCounter = Location.Count();
            }

            StateHasChanged();
        }
    }
}
