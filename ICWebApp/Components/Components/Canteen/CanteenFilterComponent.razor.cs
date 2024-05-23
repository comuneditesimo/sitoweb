using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Canteen
{
    public partial class CanteenFilterComponent
    { 
        [Inject] IFORMApplicationProvider Form_ApplicationProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }

        [Parameter] public EventCallback<Administration_Filter_CanteenSubscriptions> OnSearch { get; set; }
        [Parameter] public Administration_Filter_CanteenSubscriptions Filter { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnClear{ get; set; }
        [Parameter] public bool Modal { get; set; } = false;

        private List<AUTH_Authority> AuthorityList = new List<AUTH_Authority>();
        private List<CANTEEN_Subscriber_Status> StatusList = new List<CANTEEN_Subscriber_Status>();
        private List<V_FORM_Application_Users> UserList = new List<V_FORM_Application_Users>();
        private List<V_CANTEEN_Schoolyear> SchoolyearList = new();
        private List<Guid> AllowedAuthorities = new List<Guid>();
        private List<CANTEEN_Canteen> CanteenList = new();
        private List<CANTEEN_School> SchoolList = new();
        private bool FilterWindowVisible { get; set; } = false;
        private bool PopUpAktivated = false;

        public int StartDistanceValue { get; set; } = 0;
        public int EndDistanceValue { get; set; } =0;
        public int MinDistance { get; set; } = 0;
        public int MaxDistance { get; set; } = 100;




        protected override async Task OnInitializedAsync()
        {
            EnviromentService.OnScreenClicked += EnviromentService_OnScreenClicked;

            StatusList = await GetStatus();
            SchoolList = await CanteenProvider.GetSchools(SessionWrapper.AUTH_Municipality_ID.Value);
            CanteenList = await CanteenProvider.GetCanteens(SessionWrapper.AUTH_Municipality_ID.Value);
            SchoolyearList = await CanteenProvider.GetSchoolsyearAll(SessionWrapper.AUTH_Municipality_ID.Value);

            AllowedAuthorities = await GetAuthorities();
            await GetFilterValues();
            FilterClear();
            StateHasChanged();
        }
        private async Task<List<CANTEEN_Subscriber_Status>> GetStatus()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var statusList = await CanteenProvider.GetSubscriberStatuses();

                return statusList.ToList();
            }

            return new List<CANTEEN_Subscriber_Status>();
        }
        private async Task<List<Guid>> GetAuthorities()
        {
            if (SessionWrapper.CurrentMunicipalUser != null)
            {
                var userAuthorities = await AuthProvider.GetMunicipalUserAuthorities(SessionWrapper.CurrentMunicipalUser.ID);

                return userAuthorities.Where(p => p.AUTH_Authority_ID != null).Select(p => p.AUTH_Authority_ID.Value).ToList();
            }

            return new List<Guid>();
        }
        private async Task<bool> GetFilterValues()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var authlocList = await AuthProvider.GetAuthorityList(SessionWrapper.AUTH_Municipality_ID.Value, null);

                //AuthorityList = authlocList.Where(p => p.AUTH_Municipality_ID != null && AllowedAuthorities.Contains(p.ID)).ToList();

                var userLoclist = await AuthProvider.GetUserList(SessionWrapper.AUTH_Municipality_ID.Value);

                UserList = await Form_ApplicationProvider.GetApplicationUsers(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            return true;
        }
        private void AddFilter(Guid StatusID)
        {
            if (Filter.Subscription_Status_ID == null)
                Filter.Subscription_Status_ID = new List<Guid>();

            if (Filter.Subscription_Status_ID.Contains(StatusID))
            {
                Filter.Subscription_Status_ID.Remove(StatusID);

            }
            else
            {
                Filter.Subscription_Status_ID.Add(StatusID);
            }

            OnSearch.InvokeAsync(Filter);

            StateHasChanged();
        }
        private void ClearTagFilter()
        {
            if (Filter != null && Filter.Subscription_Status_ID != null)
            {
                Filter.Subscription_Status_ID = new List<Guid>();
                OnSearch.InvokeAsync(Filter);
                StateHasChanged();
            }
        }
        private async void FilterClear()
        {
            Administration_Filter_CanteenSubscriptions _filter = new Administration_Filter_CanteenSubscriptions();
            _filter.Text = null;
            _filter.DeadlineFrom = null;
            _filter.DeadlineTo = null;
            _filter.SubmittedFrom = null;
            _filter.SubmittedTo = null;
            _filter.EskalatedTasks = null;
            _filter.ManualInput = null;
            _filter.MinDistnanceFromSchool = null;
            _filter.MaxDistnanceFromSchool = null;
            _filter.Auth_User_ID = null;
            _filter.AUTH_Authority_ID = new List<Guid>();
            _filter.School_ID = new List<Guid>();
            _filter.SchoolYear_ID = new List<Guid>();
            _filter.Canteen_ID = new List<Guid>();
            _filter.Menu_ID = new List<Guid>();
            _filter.Subscription_Status_ID = new List<Guid>();
            _filter.Subscription_Status_ID.Add(CanteenStatus.Comitted);
            _filter.Subscription_Status_ID.Add(CanteenStatus.Waitlist);

            Filter = _filter;
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
