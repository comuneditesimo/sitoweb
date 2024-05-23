using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;

namespace ICWebApp.Components.Pages.Errors
{
    public partial class NotFound
    {
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService BreadCrumbService { get; set; }
        [Inject] ITEXTProvider TEXTProvider { get; set; }
        [Inject] ICONFProvider ConfProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }

        protected override void OnInitialized()
        {
            SessionWrapper.PageTitle = TEXTProvider.Get("PAGE_NOT_FOUND");
            SessionWrapper.PageSubTitle = TEXTProvider.Get("PAGE_NOT_FOUND_DESCRIPTION");

            BreadCrumbService.AddBreadCrumb("/404", "PAGE_NOT_FOUND", null, null, true);

            SessionWrapper.ShowTitleSepparation = true;
            SessionWrapper.ShowHelpSection = false;
            SessionWrapper.ShowQuestioneer = false;

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
        private void OnClick()
        {
            BusyIndicatorService.IsBusy = true;
            NavigationManager.NavigateTo("/");
            StateHasChanged();
        }
    }
}
