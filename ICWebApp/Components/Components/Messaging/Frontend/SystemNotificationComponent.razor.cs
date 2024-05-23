using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;

namespace ICWebApp.Components.Components.Messaging.Frontend
{
    public partial class SystemNotificationComponent
    {
        [Inject] IMSGProvider MsgProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }

        public ObservableCollection<MSG_SystemNotifications> SystemNotifications = new ObservableCollection<MSG_SystemNotifications>();
        private System.Timers.Timer? _timer;
        private bool _timerRunning = false;
        private string _priorityClass = "";

        protected override async Task OnInitializedAsync()
        {
            _timer = new System.Timers.Timer(60000);
            _timer.Elapsed += CheckNotifications;
            _timer.Enabled = true;
            _timer.AutoReset = true;

            await base.OnInitializedAsync();
        }
        private async void CheckNotifications(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_timerRunning)
            {
                _timerRunning = true;

                await GetData();
            }
        }
        private async Task<bool> GetData()
        {
            var remoteData = await MsgProvider.GetSystemNotifications();

            if (remoteData != null)
            {
                foreach (var remoteNotification in remoteData)
                {
                    if (!SystemNotifications.Select(p => p.ID).Contains(remoteNotification.ID))
                    {
                        SystemNotifications.Add(remoteNotification);
                    }
                }

                var ItemsToRemove = new List<MSG_SystemNotifications>();

                foreach (var localNotification in SystemNotifications)
                {
                    if (!remoteData.Select(p => p.ID).Contains(localNotification.ID))
                    {
                        ItemsToRemove.Add(localNotification);
                    }
                }

                foreach (var itemToRemove in ItemsToRemove)
                {
                    SystemNotifications.Remove(itemToRemove);
                }
            }
            else
            {
                SystemNotifications.Clear();
            }
            await UpdateNotificationDropdown();
            _timerRunning = false;
            await InvokeAsync(StateHasChanged);

            return true;
        }
        private async Task UpdateNotificationDropdown()
        {
            MSG_SystemNotifications? _SystemNotifications = SystemNotifications.OrderBy(p => p.MSG_SystemMessageTypes.Priority).FirstOrDefault();
            if (_SystemNotifications != null && _SystemNotifications.MSG_SystemMessageTypes != null)
            {
                _priorityClass = "ntf_" + _SystemNotifications.MSG_SystemMessageTypes.Name.Trim().ToLower();
                await InvokeAsync(StateHasChanged);
                try
                {
                    await InvokeAsync(() => EnviromentService.OpenDropdown("dropdownNotification"));
                }
                catch { }
            }
            else
            {
                _priorityClass = String.Empty;
            }
        }
    }
}