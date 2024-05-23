using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Organisation
{
    public partial class OrganisationItemSmall
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Parameter] public V_HOME_Organisation? Organisation { get; set; }
        [Parameter] public Guid? OrganisationID { get; set; }

        protected override async void OnParametersSet()
        {
            if(OrganisationID != null)
            {
                Organisation = await HOMEProvider.GetVOrganisation(OrganisationID.Value, LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnParametersSet();
        }

        private void OnClick()
        {
            if (Organisation != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Organisation/" + Organisation.ID);
                StateHasChanged();
            }
        }
    }
}
