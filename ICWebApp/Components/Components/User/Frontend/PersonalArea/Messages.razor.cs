using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ICWebApp.Components.Components.User.Frontend.PersonalArea
{
    public partial class Messages
    {
        [Inject] IChatNotificationService ChatNotificationService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }
        [Inject] IMessageService Messageservice { get; set; }
        [Inject] IMSGProvider MSGProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IChatProvider ChatProvider { get; set; }

        private List<ServiceDataItem> MessageList = new List<ServiceDataItem>();
        private bool IsBusy = true;
        protected override void OnInitialized()
        {
            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnCitizenChatStatusChange += OnChatStatusChange;
            }

            base.OnInitialized();
        }
        protected override async void OnParametersSet()
        {
            IsBusy = true;
            StateHasChanged();
            AnchorService.ClearAnchors();

            await GetMessages();

            IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private async void OnChatStatusChange()
        {

            await GetMessages();
            await InvokeAsync(StateHasChanged);

        }
        private async Task GetMessages()
        {   
            List<V_CHAT_Unread_Messages_Responsible> unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();
            var messages = await ChatProvider.GetMessagesByUser(GetCurrentUserID(), LangProvider.GetCurrentLanguageID());

            if (ChatNotificationService != null)
            {
                unreadMessages.AddRange(ChatNotificationService.UnreadMessages.Where(p => p.AUTH_Municipality_ID == SessionWrapper.AUTH_Municipality_ID && p.AUTH_Frontend_Users_ID == GetCurrentUserID() && p.AUTH_User_Type == "MunicipalUser"));
            }

            foreach (var msg in messages)
            {
                var item = new ServiceDataItem()
                {
                    Title = msg.FormName,
                    Description = msg.Message,
                    CreationDate = msg.SendDate
                };

                if (unreadMessages != null && unreadMessages.Any() && msg.ContextElementId != null &&
                    unreadMessages.Where(p => p.ContextType == "FormApplication" && !string.IsNullOrEmpty(p.ContextElementId))
                                    .Select(p => p.ContextElementId.ToLower().Trim()).Distinct()
                                    .Contains(msg.ContextElementId.ToString().ToLower().Trim()))
                {
                    item.HasUnreadChatMessage = true;
                }
                if (unreadMessages != null && unreadMessages.Any() && msg.ContextElementId != null &&
                    unreadMessages.Where(p => p.ContextType == "OrgRequests" && !string.IsNullOrEmpty(p.ContextElementId))
                                    .Select(p => p.ContextElementId.ToLower().Trim()).Distinct()
                                    .Contains(msg.ContextElementId.ToString().ToLower().Trim()))
                {
                    item.HasUnreadChatMessage = true;
                }

                if (msg.Type == "Orgs")
                {
                    item.DetailAction = (() => GoToURl("/Organization/Detail/" + msg.ContextElementId));
                }
                else if(msg.Type == "Forms")
                {
                    item.DetailAction = (() => GoToURl("/Form/Application/UserDetails/" + msg.ContextElementId));
                }

                MessageList.Add(item);
            }

            return;
        }
        private Guid GetCurrentUserID()
        {
            Guid CurrentUserID = SessionWrapper.CurrentUser.ID;

            if (SessionWrapper.CurrentSubstituteUser != null)
            {
                CurrentUserID = SessionWrapper.CurrentSubstituteUser.ID;
            }

            return CurrentUserID;
        }       
        private void GoToURl(string URL)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo(URL);                    
            StateHasChanged();
        }
    }
}
