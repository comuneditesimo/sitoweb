using DocumentFormat.OpenXml.InkML;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Canteen.Frontend
{
    public partial class RequestRefundBalancesCommittedComponent
    {
        [Inject] IBusyIndicatorService? BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService? BreadCrumbService { get; set; }
        [Inject] ICANTEENProvider? CanteenProvider { get; set; }
        [Inject] IMyCivisService? MyCivisService { get; set; }
        [Inject] ISessionWrapper? SessionWrapper { get; set; }
        [Inject] IPRIVProvider? PrivacyProvider { get; set; }
        [Inject] IAUTHProvider? AuthProvider { get; set; }
        [Inject] NavigationManager? NavManager { get; set; }
        [Inject] ITEXTProvider? TextProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper != null && TextProvider != null)
            {
                SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_REQUESTREFUND_TITLE_COMITTED");
                SessionWrapper.PageDescription = null;
            }

            SetBreadCrumb();

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }

            await base.OnInitializedAsync();
        }
        private void SetBreadCrumb()
        {
            if (BreadCrumbService != null)
            {
                BreadCrumbService.ClearBreadCrumb();
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/RequestRefundBalances", "CANTEEN_REQUESTREFUND_TITLE", null, null, false);
                }
                else
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/RequestRefundBalances", "CANTEEN_REQUESTREFUND_TITLE", null, null, false);
                }
            }
        }
        private void ReturnToPreviousPage()
        {
            if (NavManager != null)
            {
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    NavManager.NavigateTo("/Canteen/MyCivis/Service");
                }
                else
                {
                    NavManager.NavigateTo("/Canteen/Service");
                }
            }
        }
    }
}
