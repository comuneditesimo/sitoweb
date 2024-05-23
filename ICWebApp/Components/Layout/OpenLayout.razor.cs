using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Layout
{
    public partial class OpenLayout
    {
        [Inject] IBusyIndicatorService? BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper? SessionWrapper { get; set; }
        [Inject] IThemeHelper ThemeHelper { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
            if (SessionWrapper != null)
            {
                SessionWrapper.OnInitialized += SessionWrapper_OnInitialized;

                SessionWrapper.OnPageTitleChanged += SessionWrapper_OnPageTitleChanged;
                SessionWrapper.OnPageSubTitleChanged += SessionWrapper_OnPageTitleChanged;
            }

            await base.OnInitializedAsync();
        }
        private void SessionWrapper_OnInitialized()
        {
            StateHasChanged();
        }
        private void SessionWrapper_OnPageTitleChanged()
        {
            StateHasChanged();
        }
    }
}
