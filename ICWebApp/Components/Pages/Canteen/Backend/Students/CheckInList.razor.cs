using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Canteen.Backend.Students;

public partial class CheckInList
{
    [Inject] private IBusyIndicatorService BusyIndicatorService { get; set; }
    [Inject] private ISessionWrapper SessionWrapper { get; set; }
    [Inject] public ITEXTProvider TextProvider { get; set; }
    protected override void OnInitialized()
    {
        SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_POS_CHECKIN_LIST");
        BusyIndicatorService.IsBusy = false;
        base.OnInitialized();
    }
}

