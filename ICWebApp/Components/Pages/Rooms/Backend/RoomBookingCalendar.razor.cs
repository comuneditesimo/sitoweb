using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;

namespace ICWebApp.Components.Pages.Rooms.Backend
{
    public partial class RoomBookingCalendar
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IRoomAdministrationHelper RoomAdministrationHelper { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        protected override void OnInitialized()
        {
            SessionWrapper.PageTitle = TextProvider.Get("ROOMBOOKING_ROOM_BOOKING_CALENDAR");

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/RoomBooking/Calendar", "ROOMBOOKING_ROOM_BOOKING_CALENDAR", null, null, true);

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnInitialized();
        }
    }
}
