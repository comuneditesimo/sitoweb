using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using Telerik.DataSource.Extensions;
using ICWebApp.Domain.Models.User;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using Telerik.Blazor;
using ICWebApp.Application.Provider;

namespace ICWebApp.Components.Pages.UserManagement.Frontend
{
    public partial class MyServices
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }
        [Inject] IChatProvider ChatProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }
        [Inject] IFORMApplicationProvider FORMApplicationProvider { get; set; }
        [Inject] IMessageService Messageservice { get; set; }
        [Inject] IChatNotificationService ChatNotificationService { get; set; }

        private int CurrentTab = 1;
        private bool HasUnreadMessages = false;
        private bool HasUnreadServiceMessages = false;
        private bool HasNewRequestNotifications = false;

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper == null || SessionWrapper.CurrentUser == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/");
                StateHasChanged();
                return;
            }

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/User/Services", "MAINMENU_MY_SERVICES", null, null, true);

            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_MY_SERVICES");

            HasUnreadMessages = Messageservice.UserHasUnreadMessages(GetCurrentUserID());
            OnChatStatusChange();
            await ReadNewRequestNotifications();

            Messageservice.RefreshRequested += MessageService_RefreshRequested;
            ChatNotificationService.OnCitizenChatStatusChange += OnChatStatusChange;            

            SessionWrapper.ShowTitleSepparation = false;
            BusyIndicatorService.IsBusy = false;

            StateHasChanged();


            await base.OnInitializedAsync();
        }

        private void OnChatStatusChange()
        {
            List<V_CHAT_Unread_Messages_Responsible> unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();
            Guid currentUserID = GetCurrentUserID();

            if (ChatNotificationService != null)
            {
                unreadMessages.AddRange(ChatNotificationService.UnreadMessages.Where(p => p.AUTH_Municipality_ID == SessionWrapper.AUTH_Municipality_ID && p.AUTH_Frontend_Users_ID == currentUserID && p.AUTH_User_Type == "MunicipalUser"));
            }

            HasUnreadServiceMessages = unreadMessages != null && unreadMessages.Any();
            StateHasChanged();
        }
        private async Task ReadNewRequestNotifications()
        {
            bool hasNewRequestNotification = false;

            if (SessionWrapper != null &&
                SessionWrapper.AUTH_Municipality_ID != null &&
                LangProvider != null)
            {
                //List<V_CHAT_Unread_Messages_Responsible> unreadMessages = ChatNotificationService.UnreadMessages.Where(p => p.AUTH_Municipality_ID == SessionWrapper.AUTH_Municipality_ID && p.AUTH_Frontend_Users_ID == GetCurrentUserID() && p.AUTH_User_Type == "MunicipalUser");
                hasNewRequestNotification = await FORMApplicationProvider.CheckApplicationOpenPaymentsPersonalArea(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), LangProvider.GetCurrentLanguageID(), FORMCategories.Applications);

                if (hasNewRequestNotification != true)
                {
                    hasNewRequestNotification = await FORMApplicationProvider.CheckMantainancesOpenPaymentsPersonalArea(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), LangProvider.GetCurrentLanguageID(), FORMCategories.Maintenance);

                    if (hasNewRequestNotification != true)
                    {
                        if (SessionWrapper.CurrentUser != null)
                        {
                            hasNewRequestNotification = await RoomProvider.CheckBookingOpenPaymentsByUser(SessionWrapper.CurrentUser.ID);
                        }
                        else if (SessionWrapper.CurrentSubstituteUser != null)
                        {
                            hasNewRequestNotification = await RoomProvider.CheckBookingOpenPaymentsByUser(SessionWrapper.CurrentSubstituteUser.ID);
                        }
                    }
                }
            }
            HasNewRequestNotifications = hasNewRequestNotification;
        }

        private Guid GetCurrentUserID()
        {
            Guid? _currentUserID = null;

            if (SessionWrapper.CurrentUser != null)
            {
                _currentUserID = SessionWrapper.CurrentUser.ID;
            }

            if (SessionWrapper.CurrentSubstituteUser != null)
            {
                _currentUserID = SessionWrapper.CurrentSubstituteUser.ID;
            }
            if (_currentUserID != null)
            {
                return _currentUserID.Value;
            }
            return Guid.Empty;
        }
        private void SetCurrentTab(int TabIndex)
        {
            if(TabIndex != CurrentTab)
            {
                CurrentTab = TabIndex;

                StateHasChanged();
            }
        }
        private async void MessageService_RefreshRequested()
        {
            await InvokeAsync(() =>
            {
                HasUnreadMessages = Messageservice.UserHasUnreadMessages(GetCurrentUserID());                    
                StateHasChanged();
            });
        }
    }
}
