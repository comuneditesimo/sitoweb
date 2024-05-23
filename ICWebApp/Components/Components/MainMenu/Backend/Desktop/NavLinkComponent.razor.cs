using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.MainMenu.Backend.Desktop
{
    public partial class NavLinkComponent : IDisposable
    {
        [Inject] IChatNotificationService? ChatNotificationService { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] INavMenuHelper NavMenuHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ICONFProvider ConfProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Parameter] public CONF_MainMenu MenuItem { get; set; }
        private List<CONF_MainMenu> SubMenuItems { get; set; }
        private bool SubMenuVisible { get; set; } = false;

        private bool Clicked = false;

        protected override async Task OnInitializedAsync()
        {
            if (MenuItem != null)
            {
                await LoadSubMenu();
            }

            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnMunicipalChatStatusChange += OnChatStatusChange;
            }
            NavMenuHelper.OnChange += NavMenuHelper_OnChange;
            NavManager.LocationChanged += NavManager_LocationChanged;


            UpdateUnreadChatStatus();


            await base.OnInitializedAsync();
            StateHasChanged();
        }
        private void NavMenuHelper_OnChange()
        {
            if (!Clicked) 
            { 
                SubMenuVisible = false;
                StateHasChanged(); 
            }
            else
            {
                Clicked = false;
            }
        }
        private void NavManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            SubMenuVisible = false;
            StateHasChanged();
        }
        private async void OnChatStatusChange()
        {

            UpdateUnreadChatStatus();

        }
        private async Task UpdateUnreadChatStatus()
        {
            CONF_MainMenu menuItem = MenuItem;
            List<CONF_MainMenu> subMenuItems = SubMenuItems;
            List<V_CHAT_Unread_Messages_Responsible> unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();

            if (ChatNotificationService != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                unreadMessages.AddRange(ChatNotificationService.UnreadMessages.Where(p => p.AUTH_Municipality_ID == SessionWrapper.AUTH_Municipality_ID && p.ResponsibleMunicipalUserID == SessionWrapper.CurrentMunicipalUser.ID && p.AUTH_User_Type == "CitizenUser"));
            }
            // Setze Neue Chatnachrichtmarkierung im Hauptnavigationsitem
            if (unreadMessages != null && unreadMessages.Any() && MenuItem != null && (
               (MenuItem.NotificationContext == "Chat_OrgRequests" && unreadMessages.FirstOrDefault(p => p.ContextType == "OrgRequests") != null) ||
               (MenuItem.NotificationContext == "Chat_FormApplication" && unreadMessages.FirstOrDefault(p => p.ContextType == "FormApplication") != null)))
            {
                menuItem.NewNotification = true;
            }
            else
            {
                menuItem.NewNotification = false;
            }
            // Setze Neue Chatnachrichtmarkierung im Subnavigationsitem
            foreach (CONF_MainMenu subMenuItem in subMenuItems.Where(p => p.NotificationContext == "Chat_OrgRequests" || p.NotificationContext == "Chat_FormApplication"))
            {
                if (unreadMessages != null && unreadMessages.Any() && (
                   (subMenuItem.NotificationContext == "Chat_OrgRequests" && unreadMessages.FirstOrDefault(p => p.ContextType == "OrgRequests") != null) ||
                   (subMenuItem.NotificationContext == "Chat_FormApplication" && unreadMessages.FirstOrDefault(p => p.ContextType == "FormApplication") != null)))
                {
                    subMenuItem.NewNotification = true;
                }
                else
                {
                    subMenuItem.NewNotification = false;
                }
            }

            MenuItem = menuItem;
            SubMenuItems = subMenuItems;
            await InvokeAsync(StateHasChanged);           
        }
        private async Task<bool> LoadSubMenu()
        {
            List<CONF_MainMenu> locMenu = new List<CONF_MainMenu>();

            var menuBackend = await ConfProvider.GetMunicipalLoggedInSubMenuElements(MenuItem.ID);

            if (menuBackend != null)
            {
                if (NavManager.BaseUri.Contains("localhost"))
                {
                    locMenu = locMenu.Union(menuBackend).ToList();
                }
                else if (NavManager.BaseUri.Contains("192.168.77"))
                {
                    locMenu = locMenu.Union(menuBackend).ToList();
                }
                else
                {
                    locMenu = locMenu.Union(menuBackend.Where(p => p.InDevelopment == false).ToList()).ToList();
                }
            }

            if (MenuItem.ID == Guid.Parse("bdc54b2a-b6a1-49bc-b960-5f23d84f7731"))  //Dynamic Menu from Authorities
            {
                var list = await ConfProvider.GetBackendAuthorityList(MenuItem.ID);

                if (list != null && list.Count() > 0)
                {
                    locMenu = locMenu.Union(list).ToList();
                }
            }

            SubMenuItems = locMenu;

            StateHasChanged();

            return true;
        }
        private void ToggleVisbility()
        {
            SubMenuVisible = !SubMenuVisible;
            Clicked = true;
            NavMenuHelper.NotifyStateChanged();
            StateHasChanged();
        }
        private void CloseSubMenu()
        {
            SubMenuVisible = false;
            StateHasChanged();
        }
        private void NavigateTo(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (!NavManager.Uri.EndsWith(url))
                {
                    JSRuntime.InvokeVoidAsync("NavMenu_Mobile_OnHide", "mobile-menu-popup-container");

                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo(url);
                    StateHasChanged();
                }
                else
                {
                    StateHasChanged();
                }
            }
        }
        public void Dispose()
        {
            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnMunicipalChatStatusChange -= OnChatStatusChange;
            }
        }
    }
}
