using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using ICWebApp.Domain.DBModels;
using Syncfusion.Blazor.Popups;
using ICWebApp.Domain.Models;
using Telerik.Blazor;

namespace ICWebApp.Components.Pages.Organziation.Backend
{
    public partial class ApplicationList : IDisposable
    {
        [Inject] IRequestAdministrationHelper RequestAdministrationHelper { get; set; }
        [Inject] IChatNotificationService? ChatNotificationService { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IChatProvider ChatProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IORGProvider OrgProvider { get; set; }

        [Inject] public SfDialogService Dialogs { get; set; }

        private bool IsDataBusy = true;
        private List<V_ORG_Requests> Data = new List<V_ORG_Requests>();
        private Administration_Filter_Request Filter = new Administration_Filter_Request();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("ORG_BACKEND_APPLICATION_TITLE");

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/RoomBooking/List", "ROOMBOOKING_ROOM_BOOKING_LIST", null, null, true);

            if (RequestAdministrationHelper.Filter != null)
            {
                Filter = RequestAdministrationHelper.Filter;
            }
            else
            {
                Filter.Company_Type_ID = new List<Guid>();
                Filter.Request_Status_ID = new List<Guid>();
                Filter.Request_Status_ID.Add(Guid.Parse("d09bfdf6-406b-44b8-9def-d37481b0828a"));   //REQUEST
            }

            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnMunicipalChatStatusChange += OnChatStatusChange;
            }
            Data = await GetData();

            UpdateUnreadChatStatus();


            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task<List<V_ORG_Requests>> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var data = await OrgProvider.GetRequestList(Filter);

                RequestAdministrationHelper.Filter = Filter;

                return data;
            }

            return new List<V_ORG_Requests>();
        }
        private async void OnChatStatusChange()
        {

            UpdateUnreadChatStatus();

        }
        private async Task UpdateUnreadChatStatus()
        {
            List<V_ORG_Requests> requests = Data;
            List<V_CHAT_Unread_Messages_Responsible> unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();

            if (ChatNotificationService != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                unreadMessages.AddRange(ChatNotificationService.UnreadMessages.Where(p => p.AUTH_Municipality_ID == SessionWrapper.AUTH_Municipality_ID && p.ResponsibleMunicipalUserID == SessionWrapper.CurrentMunicipalUser.ID && p.AUTH_User_Type == "CitizenUser" && p.ContextType == "OrgRequests"));
            }
            foreach (V_ORG_Requests request in requests)
            {
                if (unreadMessages != null && unreadMessages.Any() && unreadMessages.Where(p => !string.IsNullOrEmpty(p.ContextElementId))
                                                                                    .Select(p => p.ContextElementId.ToLower().Trim()).Distinct().ToList().Contains(request.ID.ToString().ToLower().Trim()))
                {
                    request.HasUnreadChatMessages = true;
                }
                else
                {
                    request.HasUnreadChatMessages = false;
                }
            }

            Data = requests;

            if (Filter.OnlyNewChatMessages == true)
            {
                Data = Data.Where(p => p.HasUnreadChatMessages == true).ToList();
            }

            await InvokeAsync(StateHasChanged);
        }
        private void ShowDetails(V_ORG_Requests Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Organization/Backend/Application/Detail/" + Item.ID);
            StateHasChanged();
        }
        private async void FilterSearch(Administration_Filter_Request Filter)
        {
            IsDataBusy = true;
            StateHasChanged();

            Data = await GetData();

            UpdateUnreadChatStatus();


            IsDataBusy = false;
            StateHasChanged();
        }
        private async Task<bool> OnRowClick(GridRowClickEventArgs Args)
        {
            var item = (V_ORG_Requests)Args.Item; ;

            if (item != null)
            {
                ShowDetails(item);
            }

            return true;
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
