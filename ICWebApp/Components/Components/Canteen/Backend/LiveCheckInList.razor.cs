using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Navigations;

namespace ICWebApp.Components.Components.Canteen.Backend;

public partial class LiveCheckInList
{
    [Inject] public ISessionWrapper SessionWrapper { get; set; }
    [Inject] public ICANTEENProvider CanteenProvider { get; set; }
    [Inject] public ITEXTProvider TextProvider { get; set; }

    [Parameter] public string? AccessToken { get; set; } = null;

    
    private List<CANTEEN_Canteen> _canteens = new List<CANTEEN_Canteen>();
    private bool _accessTokenValid = false;
    private Guid _accessToken = Guid.Empty;
    private CANTEEN_Canteen? _canteen = null;
    protected override async Task OnInitializedAsync()
    {
        if (AccessToken != null)
        {
            if (Guid.TryParse(AccessToken, out _accessToken))
            {
                _canteen = await CanteenProvider.GetCanteenByAccessToken(_accessToken);
                if (_canteen != null)
                {
                    _accessTokenValid = true;
                }
            }
        }
        await LoadData();
        await base.OnInitializedAsync();
    }

    private async Task<bool> LoadData()
    {
        if (AccessToken != null && _accessToken != Guid.Empty && _accessTokenValid && _canteen != null)
        {
            _canteens = new List<CANTEEN_Canteen>() { _canteen };
        } else if (AccessToken == null && SessionWrapper.AUTH_Municipality_ID != null)
        {
            _canteens = await CanteenProvider.GetCanteens(SessionWrapper.AUTH_Municipality_ID.Value);
        }
        return true;
    }
    private void Select(SelectingEventArgs args)
    {
        if(args.IsSwiped)
        {
            args.Cancel = true;
        }
    }
}