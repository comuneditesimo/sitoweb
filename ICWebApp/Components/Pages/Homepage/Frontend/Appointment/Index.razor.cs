using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Appointment
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

        private List<V_HOME_Appointment> Appointments = new List<V_HOME_Appointment>();
        private string? SearchText;
        int MaxCounter = 6;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_APPOINTMENT");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_APPOINTMENT_DESCRIPTION_SHORT");
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Appointment", "HOMEPAGE_FRONTEND_APPOINTMENT", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                var app = await HomeProvider.GetAppointmentsWithDatesByType(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), HOMEAppointmentTypes.Appointment);
                Appointments = app.Where(p => p.DateFrom != null && p.DateFrom >= DateTime.Now).ToList();
            }
            if (Appointments.Where(p => p.Highlight == true).Any())
            {
                SessionWrapper.ShowTitleSepparation = true;
            }
            else
            {
                SessionWrapper.ShowTitleSepparation = false;
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Appointments.Count() < MaxCounter)
            {
                MaxCounter = Appointments.Count();
            }

            StateHasChanged();
        }
    }
}
