using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;


namespace ICWebApp.Components.Pages.Canteen.Frontend.Default
{
    public partial class RequestNewPOSCard
    {
        [Inject] IMyCivisService MyCivisService { get; set; }

        [Parameter] public string SubscriberTaxNumber { get; set; }

        protected override void OnInitialized()
        {
            MyCivisService.Enabled = false;
            StateHasChanged();

            base.OnInitialized();
        }
    }
}