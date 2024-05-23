using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Person
{
    public partial class PersonChip
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Parameter] public Guid HOME_Person_ID { get; set; }

        private V_HOME_Person? Person;

        protected override async void OnParametersSet()
        {
            Person = await HOMEProvider.GetVPerson(HOME_Person_ID, LANGProvider.GetCurrentLanguageID());

            StateHasChanged();
            base.OnParametersSet();
        }

        private void OnClick()
        {
            if (Person != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Person/" + Person.ID);
                StateHasChanged();
            }
        }
    }
}
