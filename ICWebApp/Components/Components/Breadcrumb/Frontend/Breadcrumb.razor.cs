using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Breadcrumb.Frontend
{
    public partial class Breadcrumb
    {
        [Inject] IBusyIndicatorService BusyIndicatorService {  get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService BreadCrumbService { get; set; }
        protected override void OnInitialized()
        {
            BreadCrumbService.OnBreadCrumbDataChanged += BreadCrumbService_OnBreadCrumbDataChanged;

            base.OnInitialized();
        }

        private void BreadCrumbService_OnBreadCrumbDataChanged()
        {
            StateHasChanged();
        }
        private void NavigateTo(string Url)
        {
            if (!NavManager.Uri.EndsWith(Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Url);
                StateHasChanged();
            }
        }
    }
}
