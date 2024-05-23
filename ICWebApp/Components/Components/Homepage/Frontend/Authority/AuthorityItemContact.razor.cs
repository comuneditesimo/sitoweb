using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Authority
{
    public partial class AuthorityItemContact
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Parameter] public Guid? HOME_Authority_ID { get; set; }
        [Parameter] public V_HOME_Authority? Authority { get; set; }
        [Parameter] public bool CssColumn { get; set; } = true;

        protected override async void OnParametersSet()
        {
            if(HOME_Authority_ID != null)
            {
                Authority = await HOMEProvider.GetVAuthority(HOME_Authority_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnParametersSet();
        }

        private void OnClick()
        {
            if (Authority != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Authority/" + Authority.ID);
                StateHasChanged();
            }
        }
    }
}
