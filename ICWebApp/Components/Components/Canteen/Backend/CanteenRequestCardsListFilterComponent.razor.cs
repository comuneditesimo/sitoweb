using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Canteen.Backend
{
    public partial class CanteenRequestCardsListFilterComponent
    {
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }

        [Parameter] public EventCallback<Administration_Filter_CanteenRequestCardList?> OnSearch { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnClear { get; set; }
        [Parameter] public Administration_Filter_CanteenRequestCardList? Filter { get; set; }
        [Parameter] public bool Modal { get; set; } = false;

        private List<V_CANTEEN_Subscriber_Card_Status> _statusList = new List<V_CANTEEN_Subscriber_Card_Status>();

        protected override async Task OnInitializedAsync()
        {
            _statusList = await GetStatus();

            StateHasChanged();
        }
        private async Task<List<V_CANTEEN_Subscriber_Card_Status>> GetStatus()
        {
            return await CanteenProvider.GetCardStatusList(LangProvider.GetCurrentLanguageID());
        }
        private void AddFilter(int StatusID)
        {
            if (Filter != null)
            {
                Filter.CANTEEN_Card_Status_ID = new List<int>();


                if (Filter.CANTEEN_Card_Status_ID.Contains(StatusID))
                {
                    Filter.CANTEEN_Card_Status_ID.Remove(StatusID);

                }
                else
                {
                    Filter.CANTEEN_Card_Status_ID.Add(StatusID);
                }
            }

            OnSearch.InvokeAsync(Filter);

            StateHasChanged();
        }
        private void ClearTagFilter()
        {
            if (Filter != null && Filter.CANTEEN_Card_Status_ID != null)
            {
                Filter.CANTEEN_Card_Status_ID = new List<int>();
                OnSearch.InvokeAsync(Filter);
                StateHasChanged();
            }
        }
        private async void FilterClear()
        {
            if (Filter != null)
            {
                Filter.Text = null;
                Filter.CANTEEN_Card_Status_ID = new List<int>();
            }

            await OnClear.InvokeAsync();

            StateHasChanged();
        }
        private void FilterSearch()
        {
            OnSearch.InvokeAsync(Filter);
            StateHasChanged();
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
