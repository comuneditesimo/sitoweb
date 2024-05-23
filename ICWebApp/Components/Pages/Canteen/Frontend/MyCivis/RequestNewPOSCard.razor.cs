using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Canteen.Frontend.MyCivis
{
    public partial class RequestNewPOSCard
    {
        [Inject] IMyCivisService MyCivisService { get; set; }

        [Parameter] public string SubscriberTaxNumber { get; set; }

        protected override void OnInitialized()
        {
            MyCivisService.Enabled = true;
            StateHasChanged();

            base.OnInitialized();
        }
    }
}