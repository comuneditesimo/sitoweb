using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace ICWebApp.Components.Components.Canteen.Backend;

public partial class CanteenLiveList : IDisposable
{
    [Parameter] public Guid CanteenId { get; set; }
    [Parameter] public bool External { get; set; } = false;
    
    [Inject] public ICANTEENProvider CanteenProvider { get; set; }
    [Inject] public NavigationManager NavManager { get; set; }
    [Inject] public ITEXTProvider TextProvider { get; set; }

    private List<V_CANTEEN_Subscriber_Movements> _movements = new List<V_CANTEEN_Subscriber_Movements>();

    private Timer? _timer;
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        if (!External)
        { 
            StartTimer(); 
        }
        await base.OnInitializedAsync();
    }

    private void StartTimer()
    {
        _timer = new Timer(5000);
        _timer.Elapsed += TimerElapsed;
        _timer.Enabled = true;
    }

    private async void TimerElapsed(Object? source, System.Timers.ElapsedEventArgs e)
    {
        await LoadData();
        await InvokeAsync(StateHasChanged);
    }

    private async Task CheckIn(Guid movementId)
    {
        await CanteenProvider.ManualCheckIn(movementId);
        await LoadData();
        await InvokeAsync(StateHasChanged);
    }
    
    private async Task<bool> LoadData()
    {
        _movements = await CanteenProvider.GetTodaysCanteenMovements(CanteenId);
        return true;
    }
    public void Dispose()
    {
        _timer?.Dispose();
    }
}
