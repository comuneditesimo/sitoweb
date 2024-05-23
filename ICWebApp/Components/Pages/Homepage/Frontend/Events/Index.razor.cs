using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Events
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

        private List<V_HOME_Appointment> Event = new List<V_HOME_Appointment>();
        private List<V_HOME_Appointment_Kategorie>? Kategories;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_EVENT");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_EVENT_DESCRIPTION_SHORT");
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.DataElement = "page-name";

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Villagelife", "HP_MAINMENU_DORFLEBEN", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Event", "HOMEPAGE_FRONTEND_EVENT", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                var app = await HomeProvider.GetAppointmentsWithDatesByType(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), HOMEAppointmentTypes.Event);
                Event = app.Where(p => p.DateFrom != null && p.DateFrom >= DateTime.Now).ToList();

                var kat = await HomeProvider.GetAppointment_Kategories(LangProvider.GetCurrentLanguageID());
                Kategories = kat.Where(p => p.HOME_Appointment_Type_ID == HOMEAppointmentTypes.Event).ToList();
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void OnKategorieClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Event/" + ID);
            StateHasChanged();
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Event.Count() < MaxCounter)
            {
                MaxCounter = Event.Count();
            }

            StateHasChanged();
        }
    }
}
