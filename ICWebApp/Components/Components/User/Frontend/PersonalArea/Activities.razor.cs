using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Syncfusion.Blazor.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ICWebApp.Components.Components.User.Frontend.PersonalArea
{
    public partial class Activities
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }
        [Inject] IMessageService Messageservice { get; set; }
        [Inject] IMSGProvider MSGProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }

        private List<ServiceDataItem> MessageList = new List<ServiceDataItem>();
        private List<MSG_Message> UnreadMessages = new List<MSG_Message>();
        private bool IsBusy = true;
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
        private async Task GetMessages()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                MessageList.Clear();
                UnreadMessages.Clear();
                var Messages = await Messageservice.GetMessages(SessionWrapper.CurrentUser.ID, null);

                if (Messages != null)
                {
                    foreach (var item in Messages)
                    {
                        var messageIsRead = item.FirstReadDate != null;
                        var newItem = new ServiceDataItem()
                        {
                            CreationDate = item.CreationDate,
                            Title = item.Subject,
                            Description = item.Messagetext,
                            DetailAction = (() => GoToMessage(item)),
                            MessageIsRead = messageIsRead,
                        };
                        if (!messageIsRead)
                        {
                            newItem.ReadMessage = () => {
                                Messageservice.SetMessageRead(item);
                                UnreadMessages.Remove(item);
                                StateHasChanged();
                            };
                            UnreadMessages.Add(item);
                        }

                        MessageList.Add(newItem);
                    }
                }
            }

            return;
        }
        private async Task MarkMessagesRead()
        {
            if (UnreadMessages.Count > 0)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
                await Messageservice.SetMessagesRead(UnreadMessages);
                await GetMessages();
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
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

                    string targetLink = Message.Link;

                    var segments = Message.Link.Split("://");

                    if (segments.Count() > 1)
                    {
                        targetLink = segments[0] + "://" + segments[1].Replace("//", "/");
                    }


                    if (NavManager.Uri != targetLink)
                    {
                        BusyIndicatorService.IsBusy = true;
                        NavManager.NavigateTo(targetLink);
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
    }
}
