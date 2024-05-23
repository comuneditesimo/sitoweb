using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using Telerik.Blazor.Components;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Globalization;
using System.Text.Json;

namespace ICWebApp.Components.Pages.Canteen.Backend.RequestRefundBalances
{
    public partial class Administration
    {
        [Inject] ICanteenRequestsAdministrationHelper? CanteenRequestsAdministrationHelper { get; set; }
        [Inject] IBusyIndicatorService? BusyIndicatorService { get; set; }
        [Inject] ICANTEENProvider? CanteenProvider { get; set; }
        [Inject] IBreadCrumbService? BreadcrumbService { get; set; }
        [Inject] ISessionWrapper? SessionWrapper { get; set; }
        [Inject] NavigationManager? NavManager { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }
        [Inject] ITEXTProvider? TextProvider { get; set; }
        [Inject] ISETTProvider? SettProvider { get; set; }

        private bool _isDataBusy = false;
        private List<V_CANTEEN_RequestRefundBalances> _requests = new List<V_CANTEEN_RequestRefundBalances>();
        private Administration_Filter_CanteenRequestRefundBalances _filter = new Administration_Filter_CanteenRequestRefundBalances();

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper != null && TextProvider != null)
            {
                SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CANTEEN_REQUESTREFUNDBALANCES_ADMINISTRATION");
            }
            if (BreadcrumbService != null)
            {
                BreadcrumbService.ClearBreadCrumb();
                BreadcrumbService.AddBreadCrumb("/Backend/Canteen/RequestRefundBalances/Administration", "MAINMENU_BACKEND_CANTEEN_REQUESTREFUNDBALANCES_ADMINISTRATION", null, null, true);
            }
            if (CanteenRequestsAdministrationHelper != null && CanteenRequestsAdministrationHelper.Filter != null)
            {
                _filter = CanteenRequestsAdministrationHelper.Filter;
            }
            else
            {
                List<CANTEEN_RequestRefundBalances_Status> _requestStatusList = new List<CANTEEN_RequestRefundBalances_Status>();
                if (CanteenProvider != null)
                {
                    _requestStatusList = await CanteenProvider.GetAllRequestRefundBalanceStatus();
                }
                _filter.CANTEEN_RequestRefundBalances_Status_ID = new List<Guid>() {Guid.Parse("E959DB7A-9AE0-4C29-BA2F-2477C581D30C")};
            }
            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            _isDataBusy = true;
            StateHasChanged();

            await GetData();

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
            }
            _isDataBusy = false;
            StateHasChanged();

            await base.OnParametersSetAsync();
        }
        private async Task<List<V_CANTEEN_RequestRefundBalances>> GetData()
        {
            List<V_CANTEEN_RequestRefundBalances> _allRequests = new List<V_CANTEEN_RequestRefundBalances>();

            if (SessionWrapper != null && CanteenProvider != null && SessionWrapper.AUTH_Municipality_ID != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                if (LangProvider != null)
                {
                    _requests = await CanteenProvider.GetRequestRefundBalances(SessionWrapper.AUTH_Municipality_ID.Value, _filter, LangProvider.GetCurrentLanguageID());
                }
                else
                {
                    _requests = await CanteenProvider.GetRequestRefundBalances(SessionWrapper.AUTH_Municipality_ID.Value, _filter, LanguageSettings.German);
                }
            }
            if (CanteenRequestsAdministrationHelper != null)
            {
                CanteenRequestsAdministrationHelper.Filter = _filter;
            }

            return _allRequests;
        }
        private void OnRowClick(GridRowClickEventArgs Args)
        {
            V_CANTEEN_RequestRefundBalances _item = (V_CANTEEN_RequestRefundBalances)Args.Item;

            if (_item != null)
            {
                ShowDetailPage(_item.ID);
            }
        }
        private void ShowDetailPage(Guid CANTEEN_RequestRefundBalances_ID)
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
            }
            StateHasChanged();
            if (NavManager != null)
            {
                NavManager.NavigateTo("/Backend/Canteen/RequestRefundBalances/Details/" + CANTEEN_RequestRefundBalances_ID);
            }
        }
        private async void FilterSearch(Administration_Filter_CanteenRequestRefundBalances Filter)
        {

            _isDataBusy = true;
            StateHasChanged();

            this._filter = Filter;
            await GetData();

            _isDataBusy = false;
            StateHasChanged();
        }
        private async Task OnStateInitHandler(GridStateEventArgs<V_CANTEEN_RequestRefundBalances> args)
        {
            try
            {
                if (SettProvider != null && SessionWrapper != null && SessionWrapper.CurrentMunicipalUser != null)
                {
                    SETT_User_States? _state = await SettProvider.GetUserState(SessionWrapper.CurrentMunicipalUser.ID);

                    if (_state != null && !string.IsNullOrEmpty(_state.CANTEEN_RequestRefundBalances_Administration_State))
                    {
                        args.GridState = JsonSerializer.Deserialize<GridState<V_CANTEEN_RequestRefundBalances>>(_state.CANTEEN_RequestRefundBalances_Administration_State);
                    }
                }
            }
            catch
            {
            }
        }
        private async void OnStateChangedHandler(GridStateEventArgs<V_CANTEEN_RequestRefundBalances> args)
        {
            if (SettProvider != null && SessionWrapper != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                SETT_User_States? _state = await SettProvider.GetUserState(SessionWrapper.CurrentMunicipalUser.ID);
                string data = JsonSerializer.Serialize(args.GridState);

                if (_state != null)
                {
                    _state.CANTEEN_RequestRefundBalances_Administration_State = data;

                    await SettProvider.SetUserState(_state);
                }
                else
                {
                    _state = new SETT_User_States();
                    _state.ID = Guid.NewGuid();
                    _state.AUTH_Users_ID = SessionWrapper.CurrentMunicipalUser.ID;
                    _state.FORM_Administration_State = data;

                    await SettProvider.SetUserState(_state);
                }
            }
        }
    }
}
