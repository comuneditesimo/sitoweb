﻿using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IAUTHProvider
    {
        public Task<MSG_SystemMessages?> Verify(string Username, string Password, Guid? AUTH_Municipality_ID = null);
        public Task<MSG_SystemMessages?> VerifyMunicipal(string Username, string Password, Guid? AUTH_Municipality_ID = null);
        public Task<MSG_SystemMessages?> VerifyEmail(Guid AUTH_Users_ID, string EmailCode);
        public Task<MSG_SystemMessages?> VerifyPhone(Guid AUTH_Users_ID, string PhoneCode);
        public Task<MSG_SystemMessages?> VerifyPasswordResetToken(Guid PasswordResetToken);
        public Task<MSG_SystemMessages?> VerifyPasswordResetTokenMunicipalUser(Guid PasswordResetToken);
        public Task<AUTH_Users?> GetUser(string Username, Guid? AUTH_Municipality_ID = null);
        public Task<AUTH_Users?> GetUser(Guid ID);
        public Task<AUTH_Municipal_Users?> GetMunicipalUser(string Username, Guid? AUTH_Municipality_ID = null);
        public Task<AUTH_Municipal_Users?> GetMunicipalUser(Guid ID);
        public AUTH_Users? GetUserSync(Guid ID);
        public AUTH_Municipal_Users? GetMunicipalUserSync(Guid ID);
        public Task<AUTH_Users?> GetUser(string FiscalCode, string? RegistrationMode, Guid? AUTH_Municipality_ID = null, bool IgnoreRemovedAt = false);
        public Task<AUTH_Users?> GetUserWithoutMunicipality(Guid ID);
        public Task<AUTH_Users?> GetUserByValidExternalLoginToken(Guid ExternalLoginToken);
        public Task<AUTH_Municipal_Users?> GetMunicipalUserByValidExternalLoginToken(Guid ExternalLoginToken);
        public Task<AUTH_Users?> GetUserByPasswordToken(Guid PasswordResetToken);
        public Task<AUTH_Municipal_Users?> GetMunicipalUserByPasswordToken(Guid PasswordResetToken);
        public Task<AUTH_Users?> GetUserByLoginToken(Guid LoginToken);
        public Task<AUTH_Municipal_Users?> GetMunicipalUserByLoginToken(Guid LoginToken);
        public Task<Guid?> CreateForceResetTokenForUser(Guid userId, TimeSpan validityPeriod);
        public Task<Guid?> CreateForceResetTokenForMunicipalUser(Guid userId, TimeSpan validityPeriod);
        public Task<bool> SetLastForceEmailSent(Guid userId, DateTime time);
        public Task<AUTH_Users?> GetUserByForcePasswordResetToken(Guid resetToken);
        public Task<AUTH_Municipal_Users?> GetMunicipalUserByForcePasswordResetToken(Guid resetToken);
        public Task<bool> SetUserForcePasswordResetSucceeded(Guid userId);
        public Task<bool> SetMunicipalUserForcePasswordResetSucceeded(Guid userId);
        public Task<AUTH_Users?> RegisterUser(AUTH_Users User, AUTH_Users_Anagrafic? Anagrafic);
        public Task<bool> UpdateUser(AUTH_Users User);
        public Task<bool> UpdateMunicipalUser(AUTH_Municipal_Users User);
        public Task<bool> DisableMunicipalUser(AUTH_Municipal_Users User);
        public Task<bool> DisableUser(AUTH_Users User);
        public Task<bool> RemoveMunicipalUser(AUTH_Municipal_Users User);
        public Task<bool> RemoveUser(AUTH_Users User);
        public Task<bool> SetMunicipaRole(AUTH_Municipal_Users User, AUTH_Roles Roles);
        public Task<bool> SetMunicipaRole(Guid AUTH_Municipal_Users_ID, Guid AUTH_Roles_ID);
        public Task<bool> SetRole(AUTH_Users User, AUTH_Roles Roles);
        public Task<bool> SetRole(Guid AUTH_Users_ID, Guid AUTH_Roles_ID);
        public Task<bool> RemoveRole(AUTH_Users User, AUTH_Roles Roles);
        public Task<bool> RemoveRole(Guid AUTH_Users_ID, Guid AUTH_Roles_ID);
        public bool EmailExists(string Email, Guid AUTH_Users_ID);
        public bool FiscalNumberExists(string FiscalNumber, bool WithManualInput = false);
        public Task<List<AUTH_Roles>> GetAllRolesByMunicipality(Guid AUTH_Municipality_ID);
        public Task<List<AUTH_Municipal_Users>> GetMunicipalUserList(Guid AUTH_Municipality_ID, Guid? AUTH_Role_ID = null, bool Organisations = false);
        public Task<List<AUTH_Users>> GetUserList(Guid AUTH_Municipality_ID, Guid? AUTH_Role_ID = null, bool Organisations = false);
        public Task<List<AUTH_UserRoles>> GetUserRolesByMunicipality(Guid AUTH_User_ID, Guid AUTH_Municipality_ID);
        public bool HasUserRole(Guid RoleID);
        public bool HasUserRole(Guid AUTH_Users_ID, Guid RoleID);
        public List<AUTH_UserRoles> GetUserRoles();
        public List<AUTH_Municipal_Users_Roles> GetMunicipalUserRoles();
        public Task<List<AUTH_Roles>> GetRoles();
        public Task<List<AUTH_Municipality>> GetMunicipalityList();
        public Task<AUTH_Municipality?> GetMunicipality(Guid ID);
        public AUTH_Municipality? GetMunicipalitySync(Guid ID);
        public Task<AUTH_Municipality?> SetMunicipality(AUTH_Municipality Data);
        public AUTH_Municipality? SetMunicipalitySync(AUTH_Municipality Data);
        public Task<List<AUTH_Authority>> GetAuthorityList(Guid AUTH_Municipality_ID, bool? Official = true, bool ? DynamicForms = true);
        public Task<List<V_HOME_Authority>> GetHomeAuthorityList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool? Official = true, bool? DynamicForms = true);
        public Task<AUTH_Authority?> GetAuthority(Guid ID);
        public Task<AUTH_Authority?> GetAuthorityMensa(Guid AUTH_Municipality_ID);
        public Task<AUTH_Authority?> GetAuthorityRooms(Guid AUTH_Municipality_ID);
        public Task<AUTH_Authority?> GetAuthorityMaintenance(Guid AUTH_Municipality_ID);
        public Task<List<AUTH_Authority_Extended>> GetAuthorityExtended(Guid AUTH_Authority_ID);
        public Task<AUTH_Users_Anagrafic?> GetAnagraficByFiscalCode(Guid AUTH_Municipality_ID, string FiscalCode);
        public Task<AUTH_Users_Anagrafic?> GetAnagraficByVatNumber(Guid AUTH_Municipality_ID, string VatNumber);
        public Task<AUTH_Users_Anagrafic?> GetAnagraficByID(Guid ID);
        public Task<AUTH_Users_Anagrafic?> GetAnagraficByUserID(Guid AUTH_Users_ID);
        public Task<AUTH_Users_Anagrafic?> SetAnagrafic(AUTH_Users_Anagrafic Data);
        public Task<AUTH_External_Verification?> GetVerification(string FiscalCode, Guid LoginToken);
        public Task<AUTH_External_Verification?> GetVerification(Guid ID);
        public Task<AUTH_External_Verification?> SetVerification(AUTH_External_Verification Data);
        public Task<AUTH_External_Error?> SetVerificationError(AUTH_External_Error Data);
        public Task<AUTH_External_Error?> GetVerificationError(Guid ID);
        public Task<bool> CheckUserAnagraficInformation(Guid AUTH_Users_ID);
        public Task<bool> CheckEmailVerification(Guid AUTH_Users_ID);
        public Task<bool> CheckPhoneVerification(Guid AUTH_Users_ID);
        public Task<bool> CheckVeriffVerification(Guid AUTH_Users_ID);
        public Task<bool> CheckForcePasswordReset(Guid AUTH_User_ID);
        public Task<bool> CheckMunicipalUserForcePasswordReset(Guid AUTH_Municipal_Users_ID);
        public void CheckUserVerifications(Guid? AUTH_Users_ID);
        public void CheckMunicipalUserVerifications(Guid? AUTH_Municipal_Users_ID);
        public Task<AUTH_VeriffResponse?> SetVeriffResponse(AUTH_VeriffResponse Data);
        public Task<AUTH_VeriffResponse?> GetVeriffResponse(Guid AUTH_Users_ID);
        public Task<List<MunicipalityDomainSelectableItem>> GetProgrammPrefixes();
        public Task<List<AUTH_Municipal_Users_Authority>> GetMunicipalUserAuthorities(Guid AUTH_Municipal_User_ID);
        public Task<bool> SetMunicipalUserAuthority(Guid AUTH_Municipal_Users_ID, Guid AUTH_Authority_ID);
        public Task<AUTH_Municipal_Users_Authority> SetMunicipalUserAuthority(AUTH_Municipal_Users_Authority Data);
        public Task<bool> RemoveMunicipalUserAuthority(Guid AUTH_Municipal_Users_ID, Guid AUTH_Authority_ID);
        public Task<V_AUTH_Authority_Statistik?> GetAuthorityStatistik(Guid AUTH_Authority_ID, Guid? AUTH_Municipality_ID);
        //Organization
        public Task<List<AUTH_Users>> GetUserOrganizations(Guid UserID, Guid AUTH_Municipality_ID);
        public Task<List<V_AUTH_Organizations>> GetUserOrganizations(string FiscalCode, Guid AUTH_Municipality_ID);
        public Task<AUTH_ORG_Users?> GetUserOrganization(Guid ID);
        public Task<AUTH_ORG_Users?> GetUserOrganization(Guid AUTH_ORG_ID, Guid AUTH_User_ID);
        public Task<AUTH_ORG_Users?> SetUserOrganization(AUTH_ORG_Users Data);
        public Task<bool> RemoveUserOrganization(AUTH_ORG_Users Data);
        public Task<List<AUTH_ORG_Roles>?> GetORGRoles(Guid AUTH_Company_Type_ID);
        public Task<List<AUTH_Company_Type>?> GetCompanyType();
        public Task<List<V_AUTH_Company_LegalForm>?> GetCompanyLegalForms(Guid AUTH_Company_Type_ID);
        public Task<List<V_AUTH_Company_LegalForm>?> GetCompanyLegalForms();
        public Task<List<V_AUTH_Company_Type>> GetVCompanyType();
        public Task<List<V_AUTH_Users_Organizations>> GetUsersOrganizations(Guid AUTH_Users_ID);
        public Task<List<V_AUTH_Users_Organizations>> GetOrganizationsUsers(Guid AUTH_ORG_Users_ID);
        public Task<List<V_AUTH_Organizations>> GetOrganizations(Guid AUTH_Municipality_ID, Administration_Filter_Organization Filter);
        public Task<List<V_AUTH_ORG_Role>> GetVOrgRoles();
        public Task<List<AUTH_MunicipalityApps>> GetMunicipalityApps(bool IncludePreparations = false);
        public Task<List<V_AUTH_BolloFree_Reason>> GetVBolloFreeReasons();
        public Task<AUTH_UserSettings?> GetSettings(Guid UserID);
        public Task<AUTH_Municipal_Users_Settings?> GetMunicipalSettings(Guid AUTH_Municipal_Users_ID);
        public Task<List<AUTH_Municipality_Page_Subtitles>?> GetPageSubtitles(Guid AUTH_Municipality_ID, int TypeID);
        public Task<AUTH_Municipality_Page_Subtitles?> SetPageSubtitle(AUTH_Municipality_Page_Subtitles Data);
        public Task<AUTH_Municipality_Page_Subtitles?> GetCurrentPageSubTitleAsync(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, int TypeID);
        public AUTH_Municipality_Page_Subtitles? GetCurrentPageSubTitle(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, int TypeID);
        public Task<AUTH_Spid_Log?> SetSpidLog(AUTH_Spid_Log Data);
        public Task<List<AUTH_Authority>> GetAlllowedAuthoritiesByMunicipalUser(Guid AUTH_Municipal_Users_ID);
        public Task<Guid?> GetLanguageIdFromDomain(string baseUri, Guid AUTH_Municipality_ID);
        public AUTH_Municipality_Footer? GetMunicipalityFooter(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
        public bool HasMunicipalUserRole(Guid RoleID);
        public bool HasMunicipalUserRole(Guid AUTH_Municipal_Users_ID, Guid RoleID);
        public Task<List<AUTH_Municipal_Users>> GetResponsibleChatMunicipalUser(V_CHAT_Message _chatMessage, Guid AUTH_Municipality_ID);
        public Task<AUTH_Authority> SetAuthority(AUTH_Authority Data);
        public Task<AUTH_Authority_Extended> SetAuthorityExtended(AUTH_Authority_Extended Data);
        public Task<List<V_AUTH_Authority_Reason>> GetAuthorityReasons(Guid AUTH_Authority_ID, Guid LANG_Language_ID);
        public Task<List<AUTH_Authority_Dates_Timeslot>> GetAuthorityTimes(Guid AUTH_Authority_ID);
        public Task<List<AUTH_Authority_Dates_Closed>> GetAuthorityClosedDates(Guid AUTH_Authority_ID);
        public Task<AUTH_Authority_Reason?> GetReason(Guid ID);
        public Task<bool> RemoveReason(Guid ID);
        public Task<AUTH_Authority_Reason?> SetReason(AUTH_Authority_Reason Data);
        public Task<bool> RemoveAuthorityTimes(Guid ID);
        public Task<AUTH_Authority_Dates_Timeslot?> SetAuthorityTimes(AUTH_Authority_Dates_Timeslot Data);
        public Task<AUTH_Authority_Reason_Extended?> SetReasonExtended(AUTH_Authority_Reason_Extended Data);
        public Task<List<AUTH_Authority_Reason_Extended>> GetReasonExtended(Guid ID);
        public Task<AUTH_Authority_Dates_Closed?> SetAuthorityClosedDates(AUTH_Authority_Dates_Closed Data);
        public Task<bool> RemoveAuthorityClosedDates(Guid ID);
        public Task<List<AUTH_Authority_Office_Hours>> GetOfficeHours(Guid AUTH_Authority_ID);
        public Task<AUTH_Authority_Office_Hours?> SetOfficeHours(AUTH_Authority_Office_Hours Data);
        public Task<bool> RemoveOfficeHours(Guid ID);

    }
}
