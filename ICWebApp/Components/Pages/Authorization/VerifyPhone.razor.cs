using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;

namespace ICWebApp.Components.Pages.Authorization
{
    public partial class VerifyPhone
    {
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ITEXTProvider TEXTProvider { get; set; }
        [Inject] ICONFProvider ConfProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IUnitOfWork UnitOfWork { get; set; }

        [Parameter] public string ID { get; set; }

        protected override void OnInitialized()
        {
            SessionWrapper.PageTitle = TEXTProvider.Get("VERIFYPHONE_TITLE");

            BusyIndicatorService.IsBusy = false;
            StateHasChanged(); 
            base.OnInitialized();
        }
    }
}
