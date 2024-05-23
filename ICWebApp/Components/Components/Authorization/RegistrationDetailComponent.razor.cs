using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using ICWebApp.DataStore;
using Microsoft.AspNetCore.Components.Authorization;
using ICWebApp.Application.Helper;
using ICWebApp.Domain.DBModels;
using Microsoft.JSInterop;
using ICWebApp.Application.Services;

namespace ICWebApp.Components.Components.Authorization
{
    public partial class RegistrationDetailComponent
    {
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }   
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAccountService AccountService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ICONFProvider CONFProvider { get; set; }

        private CONF_Enviroment? EnviromentConf;

        protected override async Task OnInitializedAsync()
        {         
            EnviromentService.OnIsMobileChanged += EnviromentService_OnIsMobileChanged;

            EnviromentConf = await CONFProvider.GetEnviromentConfiguration(null);

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private void EnviromentService_OnIsMobileChanged()
        {
            StateHasChanged();
        }
        private void HandleRegisterPassword()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Registration");
            StateHasChanged();
        }
    }
}
