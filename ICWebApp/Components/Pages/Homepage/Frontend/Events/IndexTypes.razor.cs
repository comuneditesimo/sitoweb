using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Events
{
    public partial class IndexTypes
    {
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? TypeID {  get; set; }

        private V_HOME_Appointment_Kategorie? Type;
        private List<V_HOME_Appointment> Items = new List<V_HOME_Appointment>();
        private List<V_HOME_Appointment_Kategorie>? Types;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async void OnParametersSet()
        {
            if (TypeID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Event");
                StateHasChanged();
                return;
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Types = await HomeProvider.GetAppointment_Kategories(LangProvider.GetCurrentLanguageID());
            }

            if (Types != null)
            {
                Type = Types.FirstOrDefault(p => p.ID == Guid.Parse(TypeID));
            }

            if (Type == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Event");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Type.Kategorie;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = Type.Description;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.DataElement = "page-name";

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Villagelife", "HP_MAINMENU_DORFLEBEN", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Event", "HOMEPAGE_FRONTEND_EVENT", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Type/Event/" + Type.ID.ToString(), Type.Kategorie, null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null && Type != null)
            {
                var app = await HomeProvider.GetAppointmentsWithDatesByType(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), HOMEAppointmentTypes.Event);
                Items = app.Where(p => p.DateFrom != null && p.DateFrom >= DateTime.Now && p.HOME_Appointment_Kategorie_ID == Type.ID).ToList();

                var kat = await HomeProvider.GetAppointment_Kategories(LangProvider.GetCurrentLanguageID());
                Types = kat.Where(p => p.HOME_Appointment_Type_ID == HOMEAppointmentTypes.Event).ToList();
            }

            try
            {
                EnviromentService.ScrollToTop();
            }
            catch { }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private void OnTypeClicked(Guid ID)
        {
            if (Type != null && ID != Type.ID)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Type/Event/" + ID);
                StateHasChanged();
            }
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Items.Count() < MaxCounter)
            {
                MaxCounter = Items.Count();
            }

            StateHasChanged();
        }
    }
}
