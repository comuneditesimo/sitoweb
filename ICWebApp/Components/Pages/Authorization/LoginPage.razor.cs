using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Application.Provider;

namespace ICWebApp.Components.Pages.Authorization
{
    public partial class LoginPage
    {
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ITEXTProvider TEXTProvider { get; set; }
        [Inject] ICONFProvider ConfProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }

        [Parameter] public string? ReturnUrl { get; set; }

        CONF_Enviroment? GlobalConfiguration { get; set; }
        CONF_Spid? SpidConfiguration { get; set; }
        CONF_Spid_Maintenance? SpidMaintainance { get; set; }

        protected override async void OnInitialized()
        {
            SessionWrapper.PageTitle = TEXTProvider.Get("LOGIN_OPTIONS");
            GlobalConfiguration = await ConfProvider.GetEnviromentConfiguration(null);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                SpidConfiguration = await ConfProvider.GetSpidConfiguration(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            SpidMaintainance = await ConfProvider.GetSpidMaintenance();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged(); 
            base.OnInitialized();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if(BusyIndicatorService != null && BusyIndicatorService.IsBusy == true)
            {
                BusyIndicatorService.IsBusy = false;
            }

            base.OnAfterRender(firstRender);
        }
    }
}
