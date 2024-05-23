using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Appointment
{
    public partial class AppointmentItemLarge
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Parameter] public V_HOME_Appointment Appointment { get; set; }
        [Parameter] public bool TypeClickable { get; set; } = true;

        private void OnTypeItemClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Event/" + ID);
            StateHasChanged();
        }
        private void OnItemClicked()
        {
            BusyIndicatorService.IsBusy = true;

            if(Appointment.HOME_Appointment_Type_ID == HOMEAppointmentTypes.Event)
            {
                NavManager.NavigateTo("/hp/Event/" + Appointment.ID);
            }
            else
            {
                NavManager.NavigateTo("/hp/Appointment/" + Appointment.ID);
            }

            StateHasChanged();
        }
    }
}
