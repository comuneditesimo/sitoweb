using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Amministration;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Amministration
{
    public partial class AmministrationItemSmall
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Parameter] public AmministrationtItem Item { get; set; }
        [Parameter] public bool ShowDataElement { get; set; } = false;

        private void OnClick()
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
