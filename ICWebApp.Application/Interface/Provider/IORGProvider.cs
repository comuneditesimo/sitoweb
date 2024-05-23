using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IORGProvider
    {
        public Task<List<ORG_Request>?> GetRequests(Guid AUTH_Users_ID);
        public Task<ORG_Request?> GetRequest(Guid ID);
        public Task<V_ORG_Requests?> GetVRequest(Guid ID);
        public Task<ORG_Request?> SetRequest(ORG_Request Data);
        public Task<List<ORG_Request_Attachment>?> GetRequestAttachments(Guid ORG_Request_ID);
        public Task<ORG_Request_Attachment?> GetRequestAttachment(Guid ID);
        public Task<ORG_Request_Attachment?> SetRequestAttachment(ORG_Request_Attachment Data);
        public Task<List<ORG_Request_User>?> GetRequestUsers(Guid ORG_AUTH_Users_ID);
        public Task<ORG_Request_User?> GetRequestUser(Guid ID);
        public Task<ORG_Request_User?> SetRequestUser(ORG_Request_User Data);
        public Task<List<V_ORG_Request_Status>?> GetStatusList();
        public Task<List<V_ORG_Requests>?> GetRequestList(Guid AUTH_Users_ID);
        public Task<List<V_ORG_Requests>?> GetRequestList(Administration_Filter_Request Filter);
        public Task<List<V_ORG_Requests>?> GetRequestList();
        public Task<ORG_Request_Status_Log?> GetRequestStatusLog(Guid ID);
        public Task<List<ORG_Request_Status_Log>> GetRequestStatusLogListByRequest(Guid ORG_Request_ID);
        public Task<List<ORG_Request_Status_Log>> GetRequestStatusLogListByRequestUser(Guid ORG_Request_User_ID);
        public Task<ORG_Request_Status_Log?> SetRequestStatusLog(ORG_Request_Status_Log Data);
        public Task<ORG_Request_Status_Log_Extended?> GetRequestStatusLogExtended(Guid ID);
        public Task<ORG_Request_Status_Log_Extended?> SetRequestStatusLogExtended(ORG_Request_Status_Log_Extended Data);
        public Task<List<ORG_Request_Ressource>> GetRequestRessourceList(Guid ORG_Request_ID);
        public Task<ORG_Request_Ressource?> GetRequestRessource(Guid ID);
        public Task<ORG_Request_Ressource?> SetRequestRessource(ORG_Request_Ressource Data);
        public Task<bool> RemoveRequestRessource(Guid ID, bool force = false);
        public Task<List<V_ORG_Request_Users>> GetRequestUserList(Guid AUTH_Municipality_ID);
        public Task<FILE_FileInfo?> CreateFile(ORG_Request Data);
        public MemoryStream? CreatePDF(ORG_Request Data);
        public MemoryStream GetResponsePDF(string LanguageID, string MunicipalityID, string RequestID);
        public long GetLatestProgressivNumber(Guid AUTH_Municipality_ID, int Year);
        public Task<string> ReplaceKeywords(Guid ORG_Request_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null);

    }
}

