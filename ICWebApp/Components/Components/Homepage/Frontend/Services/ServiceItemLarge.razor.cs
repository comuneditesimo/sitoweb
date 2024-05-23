using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Services
{
    public partial class ServiceItemLarge
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager {  get; set; }

        [Parameter] public ServiceItem Item { get; set; }
        [Parameter] public bool TypeClickable { get; set; } = true;

        private void OnTypeItemClicked()
        {
            if (Item != null && !string.IsNullOrEmpty(Item.Kategorie_Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Item.Kategorie_Url);
                StateHasChanged();
            }
        }
        private void OnItemClicked()
        {
            if (Item != null && !string.IsNullOrEmpty(Item.Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Item.Url);
                StateHasChanged();
            }
        }
    }
}
