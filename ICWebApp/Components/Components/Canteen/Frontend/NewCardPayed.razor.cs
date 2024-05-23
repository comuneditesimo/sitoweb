using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Canteen.Frontend
{
    public partial class NewCardPayed
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IMyCivisService MyCivisService { get; set; }
        [Parameter] public string RequestId { get; set; }

        private CANTEEN_Subscriber_Card_Request? _request;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_REQUEST_NEW_CARD");
            SessionWrapper.PageDescription = null;

            SessionWrapper.OnCurrentSubUserChanged += SessionWrapper_OnCurrentUserChanged;

            CrumbService.ClearBreadCrumb();

            if (MyCivisService.Enabled == true)
            {
                CrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
                CrumbService.AddBreadCrumb("/Canteen/MyCivis/Absence", "CANTEEN_REQUEST_NEW_CARD", null, null);
            }
            else
            {
                CrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
                CrumbService.AddBreadCrumb("/Canteen/Absence", "CANTEEN_REQUEST_NEW_CARD", null, null);
            }

            if (Guid.TryParse(RequestId, out var reqId))
            {
                _request = await CanteenProvider.GetSubscriberCardRequest(reqId);
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void SessionWrapper_OnCurrentUserChanged()
        {
            if (SessionWrapper != null && SessionWrapper.CurrentUser != null)
            {
                BusyIndicatorService.IsBusy = true;

                if (MyCivisService.Enabled == true)
                {
                    CrumbService.AddBreadCrumb("/Canteen/MyCivis", "MAINMENU_CANTEEN", null, null);
                }
                else
                {
                    CrumbService.AddBreadCrumb("/Canteen", "MAINMENU_CANTEEN", null, null);
                }

                StateHasChanged();
            }
        }
        private void BackToOverview()
        {
            NavManager.NavigateTo("/Canteen/Service");
        }
    }
}