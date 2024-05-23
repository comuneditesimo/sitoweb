using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Telerik.Blazor;
using Telerik.Reporting.Configuration;

namespace ICWebApp.Components.Pages.Canteen.Frontend.Default
{
    public partial class LandingPageCanteen
    {
        [Inject] IMyCivisService MyCivisService { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        [Parameter] public string Infopage { get; set; }

        protected override async void OnInitialized()
        {
            var IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

            if (!IsHomepage)
            {
                NavManager.NavigateTo("/Canteen/Service");
                return;
            }

            MyCivisService.Enabled = false;
            StateHasChanged();

            base.OnInitialized();
        }
    }
}
