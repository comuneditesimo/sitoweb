using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Blazored.LocalStorage;
using ICWebApp.Application.Provider;

namespace ICWebApp.Components.Components.User.Frontend
{
    public partial class UserProfileComponent : IDisposable
    {
        [Inject] IChatNotificationService? ChatNotificationService { get; set; }
        [Inject] IFORMApplicationProvider FORMApplicationProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ILocalStorageService LocalStorageService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IAccountService AccountService { get; set; }
        [Inject] IMessageService MessageService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }

        private List<ORG_Selection_Item> DropDownData = new List<ORG_Selection_Item>();
        private List<V_AUTH_Users_Organizations> Organizations = new List<V_AUTH_Users_Organizations>();

        private bool _userHasUnreadMessages = false;
        private bool _userHasUnreadChatMessage = false;
        private bool _hasNewRequestNotifications = false;
        protected override async Task OnInitializedAsync()
        {
            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnCitizenChatStatusChange += OnChatStatusChange;
            }

            SessionWrapper.OnCurrentUserChanged += SessionWrapper_OnCurrentUserChanged;
            SessionWrapper.OnCurrentSubUserChanged += SessionWrapper_OnCurrentSubUserChanged;
            SessionWrapper.MunicipalityApps = await AuthProvider.GetMunicipalityApps();

            if (SessionWrapper.CurrentUser != null)
            {
                Organizations = await AuthProvider.GetUsersOrganizations(SessionWrapper.CurrentUser.ID);                

                Organizations = Organizations.Where(p => p.ConfirmedAt != null).ToList();

                DropDownData.Clear();

                DropDownData.Add(new ORG_Selection_Item()
                {
                    ID = SessionWrapper.CurrentUser.ID,
                    Name = SessionWrapper.CurrentUser.Firstname + " " + SessionWrapper.CurrentUser.Lastname
                });

                foreach (var orgs in Organizations)
                {
                    DropDownData.Add(new ORG_Selection_Item() { ID = orgs.ORG_AUTH_Users_ID.Value, Name = orgs.ORG_Fullname });
                }

                _userHasUnreadMessages = MessageService.UserHasUnreadMessages(GetCurrentUserID());
                MessageService.RefreshRequested += MessageService_RefreshRequested;            
            }
            StateHasChanged();

            UpdateUnreadChatStatus();

            await ReadNewRequestNotifications();

            await base.OnInitializedAsync();
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

        private async void MessageService_RefreshRequested()
        {
            try
            {
                await InvokeAsync(() =>
                {
                    if (SessionWrapper.CurrentUser != null)
                    {
                        _userHasUnreadMessages = MessageService.UserHasUnreadMessages(SessionWrapper.CurrentUser.ID);
                    }
                    if (SessionWrapper.CurrentSubstituteUser != null)
                    {
                        _userHasUnreadMessages = MessageService.UserHasUnreadMessages(SessionWrapper.CurrentSubstituteUser.ID);
                    }
                    StateHasChanged();
                });
            }
            catch { }
        }

        private async void SessionWrapper_OnCurrentSubUserChanged()
        {

            UpdateUnreadChatStatus();

            await ReadNewRequestNotifications();
            StateHasChanged();
        }

        private async void SessionWrapper_OnCurrentUserChanged()
        {
            if (SessionWrapper.CurrentUser != null)
            {
                Organizations = await AuthProvider.GetUsersOrganizations(SessionWrapper.CurrentUser.ID);

                Organizations = Organizations.Where(p => p.ConfirmedAt != null).ToList();

                DropDownData.Clear();

                DropDownData.Add(new ORG_Selection_Item()
                {
                    ID = SessionWrapper.CurrentUser.ID,
                    Name = SessionWrapper.CurrentUser.Firstname + " " + SessionWrapper.CurrentUser.Lastname
                });

                foreach (var orgs in Organizations)
                {
                    DropDownData.Add(new ORG_Selection_Item() { ID = orgs.ORG_AUTH_Users_ID.Value, Name = orgs.ORG_Fullname });
                }
            }


            UpdateUnreadChatStatus();

            await ReadNewRequestNotifications();
            StateHasChanged();
        }
        private async void OnChatStatusChange()
        {

            UpdateUnreadChatStatus();

        }
        private async Task UpdateUnreadChatStatus()
        {
            List<V_CHAT_Unread_Messages_Responsible> unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();
            Guid currentUserID = GetCurrentUserID();

            if (ChatNotificationService != null)
            {
                unreadMessages.AddRange(ChatNotificationService.UnreadMessages.Where(p => p.AUTH_Municipality_ID == SessionWrapper.AUTH_Municipality_ID && p.AUTH_Frontend_Users_ID == currentUserID && p.AUTH_User_Type == "MunicipalUser"));
            }

            _userHasUnreadChatMessage = unreadMessages != null && unreadMessages.Any();
            await InvokeAsync(StateHasChanged);
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
            _hasNewRequestNotifications = hasNewRequestNotification;
        }
        private void HandleOrganizationRequest()
        {
            if (!NavManager.Uri.EndsWith("/Organization/Dashboard"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Organization/Dashboard");
                StateHasChanged();
            }
        }
        private void HandleNewOrganizationRequest()
        {
            if (!NavManager.Uri.EndsWith("/Organization/Application"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Organization/Application");
                StateHasChanged();
            }
        }
        private void UserSettingsPage()
        {
            BusyIndicatorService.IsBusy = true;

            if (SessionWrapper.CurrentSubstituteUser != null)
            {
                if (!NavManager.Uri.EndsWith("/Organization/Management/" + SessionWrapper.CurrentSubstituteUser.ID))
                {
                    NavManager.NavigateTo("/Organization/Management/" + SessionWrapper.CurrentSubstituteUser.ID);
                }
                else
                {
                    BusyIndicatorService.IsBusy = false;
                }
            }
            else
            {
                if (!NavManager.Uri.EndsWith("/User/Profile"))
                {
                    NavManager.NavigateTo("/User/Profile");
                }
                else
                {
                    BusyIndicatorService.IsBusy = false;
                }
            }

            StateHasChanged();
        }
        private async void SelectOrg(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
                                
            var SelecteddbUser = await AuthProvider.GetUser(ID);

            if (SelecteddbUser != null)
            {
                var organization = Organizations.FirstOrDefault(p => p.ORG_AUTH_Users_ID == ID);

                if (SelecteddbUser.IsOrganization && organization != null && organization.AUTH_Users_ID != null)
                {
                    if (!AuthProvider.HasUserRole(SelecteddbUser.ID, AuthRoles.Citizen))
                    {
                        await AuthProvider.SetRole(SelecteddbUser.ID, AuthRoles.Citizen);
                    }

                    SessionWrapper.CurrentSubstituteUser = SelecteddbUser;
                    await LocalStorageService.SetItemAsStringAsync("Comunix.Login.SubstituteUserID", SelecteddbUser.ID.ToString());
                }
                else
                {
                    SessionWrapper.CurrentSubstituteUser = null;
                    await LocalStorageService.RemoveItemAsync("Comunix.Login.SubstituteUserID");
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/");
                StateHasChanged();
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            
        }
        private void GoToLogin()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Login");
            StateHasChanged();
        }
        private async void HandleLogout()
        {
            await AccountService.Logout();
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/");
        }
        private void ShowMyServices()
        {
            if (!NavManager.Uri.Contains("/User/Services"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/User/Services");
                StateHasChanged();
            }
        }

        public void Dispose()
        {
            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnCitizenChatStatusChange -= OnChatStatusChange;
            }
        }
    }
}
