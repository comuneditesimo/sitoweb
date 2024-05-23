using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using Microsoft.JSInterop;
using ICWebApp.Domain.Models.Homepage.MainMenu;

namespace ICWebApp.Components.Components.MainMenu.Homepage
{
    public partial class NavLinkComponent
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Parameter] public MainMenuItem MenuItem { get; set; }

        private async void NavigateTo(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (!NavManager.Uri.EndsWith(url))
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo(url);
                    StateHasChanged();
                }
            }
            await JSRuntime.InvokeVoidAsync("navmenu_ToggleVisibility");
        }
    }
}
