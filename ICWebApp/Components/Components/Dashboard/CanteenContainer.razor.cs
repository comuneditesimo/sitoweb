using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using Telerik.Blazor.Components;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Dashboard;

public partial class CanteenContainer
{
    [Inject] private IFormAdministrationHelper FormAdministrationHelper { get; set; }
    [Inject] private IFORMApplicationProvider FormApplicationProvider { get; set; }
    [Inject] private IBusyIndicatorService BusyIndicatorService { get; set; }
    [Inject] private ICanteenAdministrationHelper FilterHelper { get; set; }
    [Inject] private ICANTEENProvider CanteenProvider { get; set; }
    [Inject] private ISessionWrapper SessionWrapper { get; set; }
    [Inject] private NavigationManager NavManager { get; set; }
    [Inject] private ITEXTProvider TextProvider { get; set; }
    [Inject] private IAUTHProvider AuthProvider { get; set; }
    [Inject] private ILANGProvider LangProvider { get; set; }
    [Parameter] public string AuthorityID { get; set; }

    private Administration_Filter_CanteenSubscriptions FilterCanteen = new Administration_Filter_CanteenSubscriptions();
    private List<V_CANTEEN_Subscriber> Subscribers = new List<V_CANTEEN_Subscriber>();
    
    private AUTH_Authority? Authority { get; set; }
    private bool IsDataBusy { get; set; } = true;


    private int ApplicationOpenCount = 0;
    private int ApplicationWaitinglistCount = 0;

    protected override async Task OnParametersSetAsync()
    {
        IsDataBusy = true;
        StateHasChanged();

        Authority = await GetData();

        await GetApplications();

        IsDataBusy = false;
        StateHasChanged();

        await base.OnParametersSetAsync();
    }

    private async Task<AUTH_Authority?> GetData()
    {
        if (AuthorityID != null) return await AuthProvider.GetAuthority(Guid.Parse(AuthorityID));

        return null;
    }


    private async Task<List<V_CANTEEN_Subscriber>> GetApplications()
    {
        if (AuthorityID != null && SessionWrapper.AUTH_Municipality_ID != null)
        {
            if (FilterHelper.Filter != null)
            {
                FilterCanteen = FilterHelper.Filter;
            }
            else
            {
                FilterCanteen = new Administration_Filter_CanteenSubscriptions();
                FilterCanteen.Text = null;
                FilterCanteen.DeadlineFrom = null;
                FilterCanteen.DeadlineTo = null;
                FilterCanteen.SubmittedFrom = null;
                FilterCanteen.SubmittedTo = null;
                FilterCanteen.EskalatedTasks = null;
                FilterCanteen.ManualInput = null;
                FilterCanteen.MinDistnanceFromSchool = null;
                FilterCanteen.MaxDistnanceFromSchool = null;
                FilterCanteen.Auth_User_ID = null;
                FilterCanteen.AUTH_Authority_ID = new List<Guid>();
                FilterCanteen.School_ID = new List<Guid>();
                FilterCanteen.SchoolYear_ID = new List<Guid>();
                FilterCanteen.Canteen_ID = new List<Guid>();
                FilterCanteen.Menu_ID = new List<Guid>();
                FilterCanteen.Subscription_Status_ID = new List<Guid>();
                FilterCanteen.Subscription_Status_ID.Add(CanteenStatus.Comitted);
                FilterCanteen.Subscription_Status_ID.Add(CanteenStatus.Waitlist);
            }

            Subscribers = await GetData(FilterCanteen);

            ApplicationOpenCount = Subscribers.Where(a => a.CANTEEN_Subscriber_Status_ID == CanteenStatus.Comitted).Count();
            ApplicationWaitinglistCount = Subscribers.Where(a => a.CANTEEN_Subscriber_Status_ID == CanteenStatus.Waitlist).Count();
        }


        return Subscribers;
    }

    private async Task<List<V_CANTEEN_Subscriber>> GetData(Administration_Filter_CanteenSubscriptions? Filter)
    {
        if (SessionWrapper.AUTH_Municipality_ID != null && SessionWrapper.CurrentMunicipalUser != null)
        {
            var subscriptions = await CanteenProvider.GetVSubscriptions(SessionWrapper.AUTH_Municipality_ID.Value,
                SessionWrapper.CurrentMunicipalUser.ID, Filter, LangProvider.GetCurrentLanguageID());


            FilterHelper.Filter = Filter;

            return subscriptions;
        }

        return new List<V_CANTEEN_Subscriber>();
    }


    private void ShowListByStatusGroup(int Group)
    {
        if (Authority != null)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Canteen/Subscriptionlist");
            StateHasChanged();
        }
    }

    private void ShowDetailPage(Guid ApplicationID)
    {
        BusyIndicatorService.IsBusy = true;
        NavManager.NavigateTo("/Backend/Canteen/Subscriptionlist");
        StateHasChanged();
    }

    private async Task<bool> OnRowClick(GridRowClickEventArgs Args)
    {
        var item = (V_CANTEEN_Subscriber)Args.Item;
        ;

        if (item != null) ShowDetailPage(item.ID);

        return true;
    }
}