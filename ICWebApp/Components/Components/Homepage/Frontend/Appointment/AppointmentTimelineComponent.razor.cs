using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Homepage.Frontend.Appointment
{
    public partial class AppointmentTimelineComponent
    {
        [Inject] private ITEXTProvider? TextProvider { get; set; }
        [Parameter] public DateTime? FromDate { get; set; }
        [Parameter] public DateTime? ToDate { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            StateHasChanged();
            await base.OnParametersSetAsync();
        }
    }
}
