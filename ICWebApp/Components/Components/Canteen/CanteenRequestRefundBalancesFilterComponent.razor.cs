using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Canteen
{
    public partial class CanteenRequestRefundBalancesFilterComponent
    {
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }

        [Parameter] public EventCallback<Administration_Filter_CanteenRequestRefundBalances?> OnSearch { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnClear { get; set; }
        [Parameter] public Administration_Filter_CanteenRequestRefundBalances? Filter { get; set; }
        [Parameter] public bool Modal { get; set; } = false;

        private List<V_CANTEEN_RequestRefundBalances_UsersWithRequests> _userList = new List<V_CANTEEN_RequestRefundBalances_UsersWithRequests>();
        private List<CANTEEN_RequestRefundBalances_Status> _statusList = new List<CANTEEN_RequestRefundBalances_Status>();
        private bool FilterWindowVisible { get; set; } = false;
        private bool PopUpAktivated = false;

        protected override async Task OnInitializedAsync()
        {
            EnviromentService.OnScreenClicked += EnviromentService_OnScreenClicked;

            _userList = await GetUsers();
            _statusList = await GetStatus();
            
            StateHasChanged();
        }
        private async Task<List<V_CANTEEN_RequestRefundBalances_UsersWithRequests>> GetUsers()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                return await CanteenProvider.GetRequestRefundBalancesUsers(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            return new List<V_CANTEEN_RequestRefundBalances_UsersWithRequests>();
        }
        private async Task<List<CANTEEN_RequestRefundBalances_Status>> GetStatus()
        {
            return await CanteenProvider.GetAllRequestRefundBalanceStatus();
        }
        private void AddFilter(Guid StatusID)
        {
            if (Filter != null)
            {
                
                Filter.CANTEEN_RequestRefundBalances_Status_ID = new List<Guid>();
                

                if (Filter.CANTEEN_RequestRefundBalances_Status_ID.Contains(StatusID))
                {
                    Filter.CANTEEN_RequestRefundBalances_Status_ID.Remove(StatusID);

                }
                else
                {
                    Filter.CANTEEN_RequestRefundBalances_Status_ID.Add(StatusID);
                }
            }

            OnSearch.InvokeAsync(Filter);

            StateHasChanged();
        }
        private void ClearTagFilter()
        {
            if (Filter != null && Filter.CANTEEN_RequestRefundBalances_Status_ID != null)
            {
                Filter.CANTEEN_RequestRefundBalances_Status_ID = new List<Guid>();
                OnSearch.InvokeAsync(Filter);
                StateHasChanged();
            }
        }
        private async void FilterClear()
        {
            if (Filter != null)
            {
                Filter.Text = null;
                Filter.CANTEEN_RequestRefundBalances_Status_ID = new List<Guid>();
                Filter.Auth_User_ID = null;
                Filter.SignedFrom = null;
                Filter.SignedTo = null;
            }

            await OnClear.InvokeAsync();

            StateHasChanged();
        }
        private void FilterSearch()
        {
            OnSearch.InvokeAsync(Filter);
            FilterWindowVisible = false;
            StateHasChanged();
        }
        private void ToggleFilter()
        {
            FilterWindowVisible = !FilterWindowVisible;

            if (FilterWindowVisible)
            {
                PopUpAktivated = false;
            }

            StateHasChanged();
        }
        private async void EnviromentService_OnScreenClicked()
        {
            if (FilterWindowVisible && PopUpAktivated)
            {
                var onScreen = await EnviromentService.MouseOverDiv("filter-popup");

                if (!onScreen)
                {
                    ToggleFilter();
                }
            }
            else
            {
                PopUpAktivated = true;
            }
        }
        private void ClearSearchBar()
        {
            if (Filter != null)
            {
                FilterClear();

                FilterSearch();
            }
        }
    }
}
