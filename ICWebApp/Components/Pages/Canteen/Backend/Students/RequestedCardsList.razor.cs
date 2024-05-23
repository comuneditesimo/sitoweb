using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Canteen.Backend.Students;

public partial class RequestedCardsList
{
    [Inject] private ICanteenRequestCardListHelper? CanteenRequestCardListHelper { get; set; }
    [Inject] private IBusyIndicatorService BusyIndicatorService { get; set; }
    [Inject] private ISessionWrapper SessionWrapper { get; set; }
    [Inject] private ITEXTProvider TextProvider { get; set; }
    [Inject] private IBreadCrumbService CrumbService { get; set; }
    [Inject] private NavigationManager NavManager { get; set; }
    [Inject] private ICANTEENProvider CanteenProvider { get; set; }

    private Administration_Filter_CanteenRequestCardList _filter = new Administration_Filter_CanteenRequestCardList();
    private List<V_CANTEEN_Subscriber_Card_Request> _cardRequests { get; set; }
    private bool _isDataBusy = false;
    protected override async Task OnInitializedAsync()
    {
        _isDataBusy = true;
        StateHasChanged();
        if (SessionWrapper.AUTH_Municipality_ID == null)
        {
            NavManager.NavigateTo("/Backend/Landing");
        }

        SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_REQUESTED_CARDS");

        CrumbService.ClearBreadCrumb();
        CrumbService.AddBreadCrumb("/Backend/RequestedCardsList", "MAINMENU_BACKEND_REQUESTED_CARDS", null, null, true);

        await GetData();

        _isDataBusy = false;
        BusyIndicatorService.IsBusy = false;
        await base.OnInitializedAsync();
    }
    private async Task<List<V_CANTEEN_Subscriber_Card_Request>> GetData()
    {
        List<V_CANTEEN_Subscriber_Card_Request> _allRequests = new List<V_CANTEEN_Subscriber_Card_Request>();

        _cardRequests = await CanteenProvider.GetPayedCardRequests(SessionWrapper.AUTH_Municipality_ID!.Value, _filter);
        
        if (CanteenRequestCardListHelper != null)
        {
            CanteenRequestCardListHelper.Filter = _filter;
        }

        return _allRequests;
    }
    private async void FilterSearch(Administration_Filter_CanteenRequestCardList Filter)
    {
        _isDataBusy = true;
        StateHasChanged();

        this._filter = Filter;
        await GetData();

        _isDataBusy = false;
        StateHasChanged();
    }
}

