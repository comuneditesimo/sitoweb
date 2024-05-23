using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Organisation
{
    public partial class PersonItemSmall
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Parameter] public V_HOME_Organisation_Person? Item { get; set; }
        [Parameter] public string? SubTitle { get; set; }
        private V_HOME_Person? Person;

        protected override async void OnParametersSet()
        {
            if (Item != null && Item.HOME_Person_ID != null)
            {
                Person = await HOMEProvider.GetVPerson(Item.HOME_Person_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnParametersSet();
        }

        private void OnClick()
        {
            if (Item != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Person/" + Item.HOME_Person_ID);
                StateHasChanged();
            }
        }
    }
}
