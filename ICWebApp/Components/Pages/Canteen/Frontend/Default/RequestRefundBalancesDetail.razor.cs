using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Canteen.Frontend.Default
{
    public partial class RequestRefundBalancesDetail
    {
        [Inject] IMyCivisService MyCivisService { get; set; }

        [Parameter] public string? ID { get; set; }

        protected override void OnInitialized()
        {
            MyCivisService.Enabled = false;
            StateHasChanged();

            base.OnInitialized();
        }
    }
}
