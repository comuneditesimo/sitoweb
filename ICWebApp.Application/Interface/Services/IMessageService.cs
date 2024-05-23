using System.Net.Mail;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Interface.Services;

public interface IMessageService
{
    public Task<List<MSG_Message>>  GetMessages(Guid authUserID, int? limit);
    public Task<List<MSG_Message>> GetMessagesToRead(Guid authUserID, int? limit);
    public bool UserHasUnreadMessages(Guid authUserID);
    public Task<int> GetMessagesToReadCount(Guid authUserID);
    public Task<bool> SendMessage(MSG_Message messageToSend, string? Link = null, List<FILE_FileInfo>? Attachments = null);
    public Task<bool> SendMessage(List<Guid?> AUTH_Users_ID, MSG_Message messageToSend, string? Link = null, List<FILE_FileInfo>? Attachments = null);
    public Task<bool> SendMessageToAuthority(Guid AUTH_Authority_ID, MSG_Message messageToSend, string? Link = null);
    public Task<MSG_Message?> GetMessage(Guid AUTH_UsersID, Guid AUTH_Municipality_ID, string? BodyTextCode, string? ShortTextCode, string SubjectTextCode, Guid MSG_MessageType_ID, bool GetTextFromDb = true, List<MSG_Message_Parameters>? Parameters = null, string? FirstName = null, string? LastName = null);
    public Task<MSG_Message?> GetMessageForMunicipalUser(Guid AUTH_Municipal_UsersID, Guid AUTH_Municipality_ID, string? BodyTextCode, string? ShortTextCode, string SubjectTextCode, Guid MSG_MessageType_ID, bool GetTextFromDb = true, List<MSG_Message_Parameters>? Parameters = null, string? FirstName = null, string? LastName = null);
    public Task<bool> UpdateMessage(MSG_Message messageToSend);
    public Task<bool> RemoveMessage(MSG_Message messageToSend);
    public Task<bool> SendAllMessagesToSend(Guid? AuthMunicipalityID);
    public Task<bool> SetMessagesRead(List<MSG_Message> messages);
    public Task<bool> SetMessageRead(MSG_Message msg);
    public Task<bool> SetAllMessageRead(List<MSG_Message> msgs);
    public event Action RefreshRequested;
    public void CallRequestRefresh();
}