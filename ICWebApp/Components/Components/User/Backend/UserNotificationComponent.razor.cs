using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Components.Components.User.Backend
{
    public partial class UserNotificationComponent : IDisposable
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IMessageService Messageservice { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IMSGProvider MSGProvider { get; set; }
        
        private bool ShowPopupMenu = false;
        private bool PopUpAktivated = false;
        private int MessagesToRead { get; set; }
        private List<MSG_Message> Messages = new List<MSG_Message>();
        private MSG_Message? CurrentMessage;
        private string ShowPopupMenuCSS
        {
            get
            {
                if (ShowPopupMenu)
                    return "nav-item-backend-active";

                return "";
            }
        }
        private bool WindowVisible { get; set; }

        protected override async Task OnInitializedAsync()
        {
            EnviromentService.OnScreenClicked += EnviromentService_OnScreenClicked;
            Messageservice.RefreshRequested += OnMessageServiceChange;

            await LoadData();

            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void ToggleMenu()
        {
            ShowPopupMenu = !ShowPopupMenu;
            StateHasChanged();
        }
        private void HideMenu()
        {
            ShowPopupMenu = false;
            StateHasChanged();
        }
        private async void EnviromentService_OnScreenClicked()
        {
            if (ShowPopupMenu && PopUpAktivated)
            {
                var onScreen = await EnviromentService.MouseOverDiv("notification-popup-menu");

                if (!onScreen)
                {
                    HideMenu();
                }
            }
            else
            {
                PopUpAktivated = true;
            }
        }
        private async void OnMessageServiceChange()
        {

            LoadData();

        }
        private async Task<bool> LoadData()
        {
            if (SessionWrapper.CurrentMunicipalUser != null)
            {
                var m = await Messageservice.GetMessagesToReadCount(SessionWrapper.CurrentMunicipalUser.ID);
                MessagesToRead = m;

                var MessageList = await Messageservice.GetMessages(SessionWrapper.CurrentMunicipalUser.ID, 10);
                List<MSG_Message> _sortedMessages = new List<MSG_Message>();
                _sortedMessages.AddRange(MessageList.Where(p => p.FirstReadDate == null).OrderByDescending(p => p.CreationDate).ToList());
                _sortedMessages.AddRange(MessageList.Where(p => p.FirstReadDate != null).OrderByDescending(p => p.CreationDate).ToList());
                Messages = _sortedMessages;

                await InvokeAsync(StateHasChanged);
            }

            return true;
        }
        private void ShowAllMessages()
        {
            ShowPopupMenu = false;
            StateHasChanged();
            NavManager.NavigateTo("/Backend/MessageCommunications");
        }
        private void GoToMessage(MSG_Message? Message)
        {
            if (Message != null)
            {
                if (Message.FirstReadDate == null)
                {
                    Message.FirstReadDate = DateTime.Now;

                    MSGProvider.SetMessage(Message);
                }

                WindowVisible = false;
                StateHasChanged();

                if (Message.Link != null)
                {
                    ShowPopupMenu = false;
                    //URLUPDATE --OK Doesn't matter
                    if (NavManager.BaseUri.Contains("localhost"))
                    {
                        Message.Link = Message.Link.Replace("https://test.comunix.bz.it/", "https://localhost:7149/");
                    }

                    if (NavManager.Uri != Message.Link)
                    {
                        BusyIndicatorService.IsBusy = true;
                        NavManager.NavigateTo(Message.Link);
                    }

                    StateHasChanged();
                }
                else
                {
                    ShowPopupMenu = false;
                    if (!NavManager.Uri.Contains("/Backend/MessageCommunications"))
                    {
                        BusyIndicatorService.IsBusy = true;
                        NavManager.NavigateTo("/Backend/MessageCommunications");
                    }
                    StateHasChanged();
                }
            }
        }
        private async Task ShowMessage(MSG_Message? Message)
        {
            if (Message != null)
            {
                CurrentMessage = Message;
                WindowVisible = true;
                if (Message.FirstReadDate == null)
                {
                    await Messageservice.SetMessageRead(Message);
                }
                StateHasChanged();
            }
        }
        private async Task SetAllasRead()
        {
            if (SessionWrapper.CurrentMunicipalUser != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();

                var messages = await Messageservice.GetMessagesToRead(SessionWrapper.CurrentMunicipalUser.ID, null);

                await Messageservice.SetAllMessageRead(messages);

                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
        }
        public void Dispose()
        {
            EnviromentService.OnScreenClicked -= EnviromentService_OnScreenClicked;
            Messageservice.RefreshRequested -= OnMessageServiceChange;
        }
    }
}
