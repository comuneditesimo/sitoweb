using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Help
{
    public partial class HelpSection
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager {  get; set; }

        AUTH_Municipality? Mun;

        protected override async void OnInitialized()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Mun = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);
                StateHasChanged();
            }

            base.OnInitialized();
        }
        private void GoToMaintenance()
        {
            if (!NavManager.Uri.Contains("/Mantainance/Landing"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Mantainance/Landing");
                StateHasChanged();
            }
        }
        private void GoToReservations()
        {
            if (!NavManager.Uri.Contains("/Hp/Request"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Request");
                StateHasChanged();
            }
        }
        private void GoToHelp()
        {
            if (!NavManager.Uri.Contains("/Hp/Assistance"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Assistance");
                StateHasChanged();
            }
        }
        private void GoToPeopleReservations()
        {
            if (!NavManager.Uri.Contains("/Hp/Person/Request"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Person/Request");
                StateHasChanged();
            }
        }
        private void GoToFaQ()
        {
            if (!NavManager.Uri.Contains("/Hp/Faq"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Faq");
                StateHasChanged();
            }
        }
    }
}
