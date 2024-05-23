using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ICWebApp.Components.Components.ActionBar
{
    public partial class ActionBar
    {
        [Inject] IActionBarService ActionBarService { get; set; }
        [Inject] ITEXTProvider TEXTProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        protected override void OnInitialized()
        {
            ActionBarService.OnActionBarChanged += ActionBarService_OnActionBarChanged;
            ActionBarService.OnShowShareButtonChanged += ActionBarService_OnActionBarChanged;
            ActionBarService.OnShowDefaultButtonsChanged += ActionBarService_OnActionBarChanged;
            ActionBarService.OnThemeListChanged += ActionBarService_OnActionBarChanged;

            base.OnInitialized();
        }

        private void ActionBarService_OnActionBarChanged()
        {
            StateHasChanged();
        }
        private void GoToTheme(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Theme/" + ID);
            StateHasChanged();
        }
    }
}
