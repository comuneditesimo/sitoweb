using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Global
{
    public partial class HideLoader
    {
        [Inject] IBusyIndicatorService BusyIndicatorService {  get; set; }

        protected override void OnInitialized()
        {
            BusyIndicatorService.IsBusy = false;

            StateHasChanged();
            base.OnInitialized();
        }
        protected override void OnAfterRender(bool firstRender)
        {
            if (BusyIndicatorService.IsBusy == true)
            {
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }
    }
}
