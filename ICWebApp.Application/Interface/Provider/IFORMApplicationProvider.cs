using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IFORMApplicationProvider
    {
        //APPLICATION
        public Task<List<FORM_Application>> GetApplicationListByDefinition(Guid FORM_Definition_ID);
        public Task<List<FORM_Application>> GetApplicationListByUser(Guid AUTH_Users_ID);
        public Task<List<FORM_Application>> GetApplicationListByUser(Guid AUTH_Users_ID, Guid FORM_Definition_ID);
        public Task<FORM_Application?> GetApplication(Guid ID);
        public Task<FORM_Application?> GetApplicationByFileID(Guid FILE_Fileinfo_ID);
        public Task<FORM_Application?> SetApplication(FORM_Application Data);
        public Task<bool> RemoveApplication(Guid ID, bool force = false);
        //APPLICATION FIELD DATA
        public Task<List<FORM_Application_Field_Data>> GetApplicationFieldDataList(Guid FORM_Application_ID);
        public Task<List<V_FORM_Application_Field>> GetVApplicationFieldDataList(Guid FORM_Application_ID);
        public Task<List<FORM_Application_Field_Data>> GetApplicationFieldDataMunicipalList(Guid FORM_Application_ID);
        public Task<FORM_Application_Field_Data?> GetApplicationFieldData(Guid ID);
        public Task<FORM_Application_Field_Data?> SetApplicationFieldData(FORM_Application_Field_Data Data);
        public Task<bool> RemoveApplicationFieldData(Guid ID, bool force = false);
        //APPLICATION FIELD SUB DATA
        public Task<List<FORM_Application_Field_SubData>> GetApplicationFieldSubDataList(Guid FORM_Application_Field_Data_ID);
        public Task<List<V_FORM_Application_ResponsibleMunicipalUser>> GetApplicationListOfActualMunicipalUser();
        public Task<FORM_Application_Field_SubData?> GetApplicationFieldSubData(Guid ID);
        public Task<FORM_Application_Field_SubData?> SetApplicationFieldSubData(FORM_Application_Field_SubData Data);
        public Task<bool> RemoveApplicationFieldSubData(Guid fieldID);
        public Task<bool> RemoveApplicationFieldSubData(Guid ID, bool force = false);
        //APPLICATION RESSOURCE
        public Task<List<FORM_Application_Ressource>> GetApplicationRessourceList(Guid FORM_Application_ID);
        public Task<List<V_FORM_Application_Ressource>> GetVApplicationRessourceList(Guid FORM_Application_ID);
        public Task<FORM_Application_Ressource?> GetApplicationRessource(Guid ID);
        public Task<FORM_Application_Ressource?> SetApplicationRessource(FORM_Application_Ressource Data);
        public Task<bool> RemoveApplicationRessource(Guid ID, bool force = false);
        //APPLICATION REPSONSIBLE
        public Task<List<FORM_Application_Responsible>> GetApplicationResponsibleListByApplication(Guid FORM_Application_ID);
        public Task<List<FORM_Application_Responsible>> GetApplicationResponsibleListByUser(Guid AUTH_Users_ID);
        public Task<FORM_Application_Responsible?> GetApplicationResponsible(Guid ID);
        public Task<FORM_Application_Responsible?> SetApplicationResponsible(FORM_Application_Responsible Data);
        public Task<bool> RemoveApplicationResponsible(Guid ID, bool force = false);

        //GET OR CREATE APPLICATION FIELDS
        public Task<List<FORM_Application_Field_Data>> GetOrCreateApplicationFieldData(FORM_Application FORM_Application);
        public Task<List<FORM_Application_Field_Data>> GetAdditionalContainerFieldData(Guid FORM_Application_Field_ID, FORM_Definition_Field FORM_Defintion_Field, FORM_Application FORM_Application, long? NextRepetitionCount);
        //APPLICATION UPLOAD
        public Task<List<FORM_Application_Upload>> GetApplicationUploadList(Guid FORM_Application_ID);
        public Task<FORM_Application_Upload?> GetApplicationUpload(Guid ID);
        public Task<FORM_Application_Upload?> GetApplicationUploadByDefinition(Guid FORM_Definition_Upload_ID, Guid FORM_Application_ID);
        public Task<FORM_Application_Upload?> SetApplicationUpload(FORM_Application_Upload Data);
        public Task<bool> RemoveApplicationUpload(Guid ID, bool force = false);
        //APPLICATION UPLOAD FILE
        public Task<List<FORM_Application_Upload_File>> GetApplicationUploadFileList(Guid FORM_Application_Upload_ID);
        public Task<FORM_Application_Upload_File?> GetApplicationUploadFile(Guid ID);
        public Task<FORM_Application_Upload_File?> GetApplicationUploadFileByFileID(Guid FILE_FileInfo_ID);
        public Task<FORM_Application_Upload_File?> SetApplicationUploadFile(FORM_Application_Upload_File Data);
        public Task<bool> RemoveApplicationUploadFile(Guid ID, bool force = false);
        //APPLICATION STATUS
        public List<FORM_Application_Status> GetStatusList();
        public List<FORM_Application_Status> GetStatusListByMunicipality(Guid AUTH_Municipality_ID);
        public FORM_Application_Status? GetStatus(Guid FORM_Application_Status_ID);
        public List<FORM_Application_Status_Municipal> GetOrCreateMunicipalStatusList(Guid AUTH_Municipality_ID);
        public Task<FORM_Application_Status_Municipal?> GetMunicipalStatus(Guid ID);
        public FORM_Application_Status_Municipal? SetMunicipalStatus(FORM_Application_Status_Municipal Data);
        //APPLICATION STATUS LOG
        public Task<List<FORM_Application_Status_Log>> GetApplicationStatusLogList(Guid FORM_Application_ID);
        public Task<FORM_Application_Status_Log?> GetApplicationStatusLog(Guid ID);
        public Task<FORM_Application_Status_Log?> SetApplicationStatusLog(FORM_Application_Status_Log Data);
        public Task<FORM_Application_Status_Log_Extended?> GetApplicationStatusLogExtended(Guid ID);
        public Task<FORM_Application_Status_Log_Extended?> SetApplicationStatusLogExtended(FORM_Application_Status_Log_Extended Data);
        //FILE
        public Task<FILE_FileInfo?> GetOrCreateFileInfo(Guid FORM_Application_ID, string ReportName);
        //APPLICATION GET LIST WITH FILTER
        public Task<List<V_FORM_Application>> GetApplications(Guid AUTH_Municipality_ID, Guid Current_AUTH_Users_ID, List<Guid> AllowedAuthorities, Administration_Filter_Item? Filter);
        public Task<List<V_FORM_Application>> GetApplications(Guid AUTH_Authority_ID, Guid AUTH_Municipality_ID, int Amount = 6);
        public Task<List<V_FORM_Application>> GetApplications(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID);
        public Task<List<V_FORM_Application>> GetMantainances(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID);
        public Task<List<V_FORM_Application_Users>> GetApplicationUsers(Guid AUTH_Municipality_ID);
        //APPLICATION PERSONAL AREA
        public Task<bool> CheckApplicationOpenPaymentsPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID);
        public Task<List<V_FORM_Application_Personal_Area>> GetApplicationsPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID);
        public Task<bool> CheckMantainancesOpenPaymentsPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID);
        public Task<List<V_FORM_Application_Personal_Area>> GetMantainancesPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID);
        //APPLICATION TRANSCATIONS
        public Task<List<FORM_Application_Transactions>> GetApplicationTransactionList(Guid FORM_Application_ID);
        public Task<FORM_Application_Transactions?> SetApplicationTransaction(FORM_Application_Transactions Data);
        public Task<bool> RemoveApplicationTransactions(Guid applicationID);
        //APPLICATION CHAT
        public Task<List<FORM_Application_Chat>> GetApplicationChatList(Guid FORM_Application_ID);
        public Task<FORM_Application_Chat?> GetApplicationChat(Guid ID);
        public Task<FORM_Application_Chat?> SetApplicationChat(FORM_Application_Chat Data);
        public Task<bool> RemoveApplicationChat(Guid ID);
        //APPLICATION CHAT DOKUMENTS
        public Task<List<FORM_Application_Chat_Dokument>> GetApplicationChatDokumentList(Guid FORM_Application_Chat_ID);
        public Task<List<FORM_Application_Chat_Dokument>> GetApplicationChatDokumentListByApplication(Guid FORM_Application_ID);
        public Task<FORM_Application_Chat_Dokument?> GetApplicationDokumentChat(Guid ID);
        public Task<FORM_Application_Chat_Dokument?> SetApplicationDokumentChat(FORM_Application_Chat_Dokument Data);
        public Task<bool> RemoveApplicationDokumentChat(Guid ID);
        //APPLICATION CHAT VIEW
        public Task<List<V_FORM_Application_Chat>> GetViewApplicationChatList(Guid FORM_Application_ID);
        //APPLICATION CHAT DOKUMENT VIEW
        public Task<List<V_FORM_Application_Chat_Dokument>> GetViewApplicationChatDokumentListByApplication(Guid FORM_Application_ID);
        //APPLICATION PRIORITYLIST
        public Task<List<FORM_Application_Priority>> GetPriorities();
        //APPLICATION STATISTIK
        public Task<List<V_FORM_Application_Priority_Statistik>> GetPriorityStatistik(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID);
        public Task<List<V_FORM_Application_Status_Statistik>> GetStatusStatistik(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID);
        //APPLICATION NOTE
        public Task<List<FORM_Application_Note>> GetApplicationNoteList(Guid FORM_Application_ID);
        public Task<FORM_Application_Note?> GetApplicationNote(Guid ID);
        public Task<FORM_Application_Note?> SetApplicationNote(FORM_Application_Note Data);
        public Task<bool> RemoveApplicationNote(Guid ID);
        //APPLICATION NOTE DOKUMENTS
        public Task<List<FORM_Application_Note_Dokument>> GetApplicationNoteDokumentList(Guid FORM_Application_Note_ID);
        public Task<FORM_Application_Note_Dokument?> GetApplicationDokumentNote(Guid ID);
        public Task<FORM_Application_Note_Dokument?> SetApplicationDokumentNote(FORM_Application_Note_Dokument Data);
        public Task<bool> RemoveApplicationDokumentNote(Guid ID);
        //APPLICATION NOTE VIEW
        public Task<List<V_FORM_Application_Note>> GetViewApplicationNoteList(Guid FORM_Application_ID);
        //APPLICATION NOTE DOKUMENT VIEW
        public Task<List<V_FORM_Application_Note_Dokument>> GetViewApplicationNoteDokumentList(Guid FORM_Application_ID);
        //APPLICATION MUNICIPALFIELD
        public Task<List<FORM_Application_Municipal_Field_Data>> GetFormApplicationMunicipalFieldList(Guid FORM_Application_ID);
        public Task<FORM_Application_Municipal_Field_Data?> GetFormApplicationMunicipalField(Guid ID);
        public Task<FORM_Application_Municipal_Field_Data?> SetFormApplicationMunicipalField(FORM_Application_Municipal_Field_Data Data);
        public Task<bool> RemoveFormApplicationMunicipalField(Guid ID);
        //Progressiv Number
        public long GetLatestProgressivNumber(Guid Form_Definition_ID, Guid AUTH_Municipality_ID, int Year);
        public Task<string> ReplaceKeywords(Guid FORM_Application_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null);
        //Signing
        public Task<List<SignerItem>> GetSigningFields(Guid FORM_Application_ID);
        public Task<List<FILE_FileInfo>> GetApplicationUploadFiles(Guid FORM_Application_ID, Guid FORM_Definition_ID);
        public Task<List<FORM_Application_Archive>> GetArchive(Guid FORM_Application_ID);
        public Task<FORM_Application_Archive?> SetArchive(FORM_Application_Archive Data);
        public Task<bool> RemoveArchive(Guid ID);
        public Task<bool> IsBolloFree(Guid FORM_Application_ID, Guid FORM_Definition_ID);
        public Task<bool> IsResponsibleForTask(Guid userId, Guid applicationId);

        //Logging
        public Task<FORM_Application_Log> SetApplicationLog(FORM_Application_Log Data);
    }
}
