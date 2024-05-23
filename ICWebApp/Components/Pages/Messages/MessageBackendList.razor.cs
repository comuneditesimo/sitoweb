using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;

namespace ICWebApp.Components.Pages.Messages
{
    public partial class MessageBackendList : IDisposable
    {
        [Inject] private IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] private ISessionWrapper SessionWrapper { get; set; }
        [Inject] private NavigationManager NavManager { get; set; }
        [Inject] private ITEXTProvider TextProvider { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ILANGProvider LanguageProvider { get; set; }
        [Inject] IMessageService Messageservice { get; set; }
        [Inject] IMSGProvider MSGProvider { get; set; }


        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ExternalID { get; set; }

        public EventCallback OnUpdateRead { get; set; }

        private MSG_Message CurrentMessage = new MSG_Message();
        private List<MSG_Message> Messages = new List<MSG_Message>();
        private bool IsDataBusy { get; set; } = false;
        private bool WindowVisible { get; set; }


        protected override async Task OnInitializedAsync()
        {
            IsDataBusy = true;
            StateHasChanged();

            Messageservice.RefreshRequested += OnMessageServiceChange;
            await LoadData();

            IsDataBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void OnMessageServiceChange()
        {

            LoadData();

        }
        protected override async Task OnParametersSetAsync()
        {
            BusyIndicatorService.IsBusy = true;
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_MESSAGES");

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/MessageCommunications", "MAINMENU_MESSAGES", null, null, true);

            await LanguageProvider.GetAll();

            if (SessionWrapper != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                await LoadData();
            }

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnParametersSetAsync();
        }
        private async void ReadMessage(MSG_Message Item)
        {
            CurrentMessage = Item;
            WindowVisible = true;

            if (Item.FirstReadDate == null)
            {
                await Messageservice.SetMessageRead(Item); 
                await OnUpdateRead.InvokeAsync();
            }

            StateHasChanged();
        }
        private async Task<bool> LoadData()
        {
            if (SessionWrapper.CurrentMunicipalUser != null)
            {
                Messages = await Messageservice.GetMessages(SessionWrapper.CurrentMunicipalUser.ID, null);
                List<MSG_Message> _sortedMessages = new List<MSG_Message>();
                _sortedMessages.AddRange(Messages.Where(p => p.ShowInList == true && p.FirstReadDate == null).OrderByDescending(p => p.CreationDate).ToList());
                _sortedMessages.AddRange(Messages.Where(p => p.ShowInList == true && p.FirstReadDate != null).OrderByDescending(p => p.CreationDate).ToList());
                Messages = _sortedMessages;

                await InvokeAsync(StateHasChanged);
            }

            return true;
        }
        private void GoToMessage(MSG_Message? Message)
        {
            if (Message != null)
            {
                Message.FirstReadDate = DateTime.Now;

                MSGProvider.SetMessage(Message);

                if (Message.Link != null)
                {
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
                    if (!NavManager.Uri.Contains("/Backend/MessageCommunications"))
                    {
                        BusyIndicatorService.IsBusy = true;
                        NavManager.NavigateTo("/Backend/MessageCommunications");
                    }
                    StateHasChanged();
                }
            }
        }
        public void Dispose()
        {
            Messageservice.RefreshRequested -= OnMessageServiceChange;
        }
    }
}
