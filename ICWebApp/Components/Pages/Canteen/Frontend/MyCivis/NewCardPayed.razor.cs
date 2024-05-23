using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Canteen.Frontend.MyCivis
{
    public partial class NewCardPayed
    {
        [Inject] IMyCivisService MyCivisService { get; set; }

        [Parameter] public string RequestId { get; set; }

        protected override void OnInitialized()
        {
            MyCivisService.Enabled = true;
            StateHasChanged();

            base.OnInitialized();
        }
    }
}