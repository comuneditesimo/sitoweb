using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Layout.Frontend
{
    public partial class Title
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IAccountService AccountService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Parameter] public string TitleValue { get; set; }
        private AUTH_Municipality? Municipality { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                SessionWrapper.PageIsPublic = true;

                if (!string.IsNullOrEmpty(TitleValue))
                {
                    TitleValue = TitleValue.Replace("TitleValue=", "");
                }
                SessionWrapper.PageTitle = TextProvider.Get(TitleValue);
                SessionWrapper.PageDescription = null;


                var municipality = await SessionWrapper.GetMunicipalityID();

                if (SessionWrapper != null && municipality != null)
                {

                    Municipality = await AuthProvider.GetMunicipality(municipality.Value);
                }

                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
