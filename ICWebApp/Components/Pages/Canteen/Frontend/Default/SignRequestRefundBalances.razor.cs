using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Canteen.Frontend.Default
{
    public partial class SignRequestRefundBalances
    {
        [Inject] IMyCivisService MyCivisService { get; set; }
        [Parameter] public string RequestRefundBalanceID { get; set; }
        protected override void OnInitialized()
        {
            MyCivisService.Enabled = false;
            StateHasChanged();

            base.OnInitialized();
        }
    }
}
