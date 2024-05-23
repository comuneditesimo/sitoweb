using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Components.Components.Chat
{
    public partial class ChatComponent : IAsyncDisposable
    {
        [Inject] IChatNotificationService? ChatNotificationService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IMessageService MessageService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IChatProvider ChatProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IAUTHProvider AUTHProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }

        [Parameter] public string ContextName { get; set; } = "Unknown";
        [Parameter] public string ContextElementId { get; set; }
        [Parameter] public bool AllowSend { get; set; } = true;
        [Parameter] public Guid? FrontendUserID { get; set; }

        private List<V_CHAT_Message> messages = new List<V_CHAT_Message>();
        private List<V_CHAT_Message_Document> documents = new List<V_CHAT_Message_Document>();

        private HubConnection? chatHub;
        private bool chatHubConnected { get => chatHub != null && chatHub.State == HubConnectionState.Connected; }

        private bool _busy = true;
        private bool _shouldRender = true;
        protected override bool ShouldRender()
        {
            return _shouldRender;
        }
        protected override async Task OnParametersSetAsync()
        {
            await RegisterToHub();
            await LoadMessages();

            _busy = false;

            await EnviromentService.ScrollToElement("chat-end");
            await base.OnParametersSetAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await EnviromentService.ScrollToElement("chat-end");
            await base.OnAfterRenderAsync(firstRender);
        }
        private async Task<bool> RegisterToHub()
        {
            try
            {
                if (chatHub != null)
                {
                    await chatHub.StopAsync();
                }

                chatHub = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/chathub"))
                .Build();

                chatHub.On<V_CHAT_Message, List<V_CHAT_Message_Document>>("ReceiveMessage", (msg, docs) =>
                {
                    InvokeAsync(() => MessageReceived(msg, docs));
                });

                await chatHub.StartAsync();
                await chatHub.SendAsync("RegisterToGroup", ContextElementId);
            }
            catch
            {
                return false;
            }

            return true;
        }
        private async Task MessageReceived(V_CHAT_Message msg, List<V_CHAT_Message_Document> docs)
        {
            _shouldRender = true;
            messages.Add(msg);
            documents.AddRange(docs);
            AllowSend = true;
            await InvokeAsync(StateHasChanged);
            await EnviromentService.ScrollToElement("chat-end");

            MarkMessagesAsRead();

        }
        private async Task<bool> LoadMessages()
        {
            messages = (await ChatProvider.GetChatMessages(ContextElementId)).OrderBy(p => p.SendDate).ToList();
            documents = await ChatProvider.GetChatDocuments(ContextElementId);

            MarkMessagesAsRead();

            return true;
        }
        private async Task DownloadFile(Guid fileInfoId)
        {
            var fileToDownload = await FileProvider.GetFileInfoAsync(fileInfoId);

            if (fileToDownload != null)
            {
                FILE_FileStorage? blob = null;
                if (fileToDownload.FILE_FileStorage != null && fileToDownload.FILE_FileStorage.Count() > 0)
                {
                    blob = fileToDownload.FILE_FileStorage.FirstOrDefault();
                }
                else
                {
                    blob = await FileProvider.GetFileStorageAsync(fileToDownload.ID);
                }

                if (blob != null && blob.FileImage != null)
                {
                    await EnviromentService.DownloadFile(blob.FileImage, fileToDownload.FileName + fileToDownload.FileExtension);
                }
            }
        }
        private async void SendMessage((V_CHAT_Message msg, List<V_CHAT_Message_Document> docs) data)
        {
            try
            {

                _shouldRender = false;
                if (chatHubConnected)
                {
                    await chatHub!.SendAsync("NotifyGroup", ContextElementId, data.msg, data.docs);
                }
                if (data.msg.AUTH_User_Type == "CitizenUser" && SessionWrapper.AUTH_Municipality_ID != null)
                {
                    if (ChatNotificationService != null)
                    {
                        ChatNotificationService.NotifyMunicipalStatusChange();                                                  // Aktualisiere neue Chat-Nachricht Komponenten im Backend
                    }
                    CreateMunicipalUserMessage(data.msg);                                                                       // Erstelle Benachrichtigung Backend Nutzer
                }
                else if (ChatNotificationService != null)
                {
                    ChatNotificationService.NotifyCitizenStatusChange();                                                        // Aktualisiere neue Chat-Nachricht Komponenten im Frontend
                }

            }
            catch
            {
            }
        }
        private async Task MarkMessagesAsRead()
        {
            try
            {
                if (SessionWrapper.CurrentUser != null || SessionWrapper.CurrentSubstituteUser != null) // Is CitizenUser - Update on Municipalside
                {
                    foreach (V_CHAT_Message _message in messages.Where(p => p.ReadDate == null && p.AUTH_User_Type == "MunicipalUser"))
                    {
                        V_CHAT_Message? _result = await ChatProvider.MarkMessageAsRead(_message);
                        if (_result != null && _message.ID == _result.ID)
                        {
                            _message.ReadDate = _result.ReadDate;
                        }
                        if (ChatNotificationService != null)
                        {

                            ChatNotificationService.NotifyCitizenStatusChange();

                        }
                    }
                }
                else if (SessionWrapper.CurrentMunicipalUser != null) // IsMunicipalUser - Update on CitizenSide and Municipalside
                {
                    foreach (V_CHAT_Message _message in messages.Where(p => p.ReadDate == null && p.AUTH_User_Type == "CitizenUser"))
                    {
                        V_CHAT_Message? _result = await ChatProvider.MarkMessageAsRead(_message);
                        if (_result != null && _message.ID == _result.ID)
                        {
                            _message.ReadDate = _result.ReadDate;
                        }
                        if (ChatNotificationService != null)
                        {

                            ChatNotificationService.NotifyMunicipalStatusChange();

                        }
                    }
                }
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }
        }
        private async Task CreateMunicipalUserMessage(V_CHAT_Message msg)
        {
            try
            {
                if (msg.AUTH_User_Type == "CitizenUser" && SessionWrapper.AUTH_Municipality_ID != null)
                {
                    Guid _messageType = MessageTypeSettings.CustomMessage; //CustomMessage
                    List<AUTH_Municipal_Users> _getMunicipalUsers = new List<AUTH_Municipal_Users>();
                    _getMunicipalUsers = await AUTHProvider.GetResponsibleChatMunicipalUser(msg, SessionWrapper.AUTH_Municipality_ID.Value);
                    foreach (AUTH_Municipal_Users _municipalUser in _getMunicipalUsers)
                    {
                        if (_municipalUser.AUTH_Municipality_ID != null && SessionWrapper.AUTH_Municipality_ID == _municipalUser.AUTH_Municipality_ID)
                        {
                            MSG_Message? _message = await MessageService.GetMessageForMunicipalUser(_municipalUser.ID, _municipalUser.AUTH_Municipality_ID.Value, "NOTIF_CHAT_NEW_MESSAGE_TEXT_MUNICIPALINLIST", "NOTIF_CHAT_NEW_MESSAGE_SHORTTEXT_MUNICIPALINLIST", "NOTIF_CHAT_NEW_MESSAGE_TITLE", _messageType, true);
                            if (_message != null)
                            {
                                string _link = "";
                                if (msg.ContextType == "FormApplication")
                                {
                                    _link = NavManager.BaseUri + "Backend/Form/Detail/" + msg.ContextElementId;
                                }
                                else if (msg.ContextType == "OrgRequests")
                                {
                                    _link = NavManager.BaseUri + "Organization/Backend/Application/Detail/" + msg.ContextElementId;
                                }
                                bool _result = await MessageService.SendMessage(_message, _link);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
        public async ValueTask DisposeAsync()
        {
            if (chatHub != null)
                await chatHub.DisposeAsync();
        }
    }
}
