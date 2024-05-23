using System.Diagnostics;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Reporting;

namespace ICWebApp.Components.Components.Chat;

public partial class ChatInputComponent
{
    [Inject] ISessionWrapper SessionWrapper { get; set; }
    [Inject] IChatProvider ChatProvider { get; set; }
    [Inject] ITEXTProvider TextProvider { get; set; }
    [Parameter] public string ContextElementId { get; set; }
    [Parameter] public string ContextName { get; set; } = "Test";
    [Parameter] public bool AllowSend { get; set; } = true;
    [Parameter] public EventCallback<(V_CHAT_Message, List<V_CHAT_Message_Document>)> OnSendMessage { get; set; }
    private bool _displayUpload = false;

    private ChatUploadComponent? _uploadComponent;
    
    private List<CHAT_Message_Document> currentMessageDocs = new List<CHAT_Message_Document>();
    private string currentMessage = "";
    private bool currentMessageHasContent { get => !string.IsNullOrEmpty(currentMessage) || currentMessageDocs.Count > 0; }
 
    private void ToggleUploadComponent()
    {
        _displayUpload = !_displayUpload;
    }

    private async Task SendMessage()
    {
        if (!currentMessageHasContent || !AllowSend)
            return;

        Guid? userID = null;
        string userFirstName = "";
        string userLastName = "";
        string userType = "CitizenUser";
        if (SessionWrapper.CurrentUser != null)
        {
            userID = SessionWrapper.CurrentUser.ID;
            userFirstName = SessionWrapper.CurrentUser.Firstname;
            userLastName = SessionWrapper.CurrentUser.Lastname;
        }
        else if (SessionWrapper.CurrentMunicipalUser != null)
        {
            userID = SessionWrapper.CurrentMunicipalUser.ID;
            userFirstName = SessionWrapper.CurrentMunicipalUser.Firstname;
            userLastName = SessionWrapper.CurrentMunicipalUser.Lastname;
            userType = "MunicipalUser";
        }
        if (userID != null)
        {
            var chatMessage = new CHAT_Message()
            {
                ID = Guid.NewGuid(),
                AUTH_Users_ID = userID.Value,
                ContextElementId = ContextElementId,
                ContextType = ContextName,
                Message = currentMessage,
                SendDate = DateTime.Now
            };

            V_CHAT_Message view = new V_CHAT_Message()
            {
                ID = chatMessage.ID,
                AUTH_Users_ID = userID.Value,
                Firstname = userFirstName,
                Lastname = userLastName,
                ContextElementId = ContextElementId,
                ContextType = ContextName,
                AUTH_User_Type = userType,
                Message = currentMessage,
                SendDate = DateTime.Now
            };

            List<CHAT_Message_Document> currentDocs = new List<CHAT_Message_Document>();
            currentDocs.AddRange(currentMessageDocs);

            List<V_CHAT_Message_Document> docs = new List<V_CHAT_Message_Document>();
            foreach (var doc in currentDocs)
            {
                doc.CHAT_Message_ID = view.ID;
                if (doc.FILE_FileInfo != null)
                {
                    V_CHAT_Message_Document newDoc = new V_CHAT_Message_Document()
                    {
                        ID = doc.ID,
                        CHAT_Message_ID = doc.CHAT_Message_ID,
                        FILE_FileInfo_ID = doc.FILE_FileInfo_ID,
                        ContextElementId = doc.ContextElementId,
                        FileName = doc.FILE_FileInfo.FileName,
                        FileExtension = doc.FILE_FileInfo.FileExtension,
                        Size = doc.FILE_FileInfo.Size
                    };
                    docs.Add(newDoc);
                }
            }

            await ChatProvider.SaveMessage(chatMessage, currentDocs);

            _displayUpload = false;
            currentMessage = "";
            _uploadComponent?.ClearData();
        
            await OnSendMessage.InvokeAsync((view, docs));
        }
    }
    
    private void OnUploadsChanged(List<FILE_FileInfo> files)
    {
        currentMessageDocs.Clear();
        foreach (var file in files)
        {
            currentMessageDocs.Add(new CHAT_Message_Document()
            {
                ID = Guid.NewGuid(),
                ContextElementId = ContextElementId,
                CHAT_Message_ID = Guid.Empty,
                FILE_FileInfo_ID = file.ID,
                FILE_FileInfo = file
            });
        }
    }
   
}

