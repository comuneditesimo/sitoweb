using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Helper;
using ICWebApp.Domain.DBModels;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.Models;
using System.Globalization;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Settings;
using Microsoft.EntityFrameworkCore;
using ICWebApp.DataStore.MSSQL.UnitOfWork;
using System.Linq;
using ICWebApp.Application.Interface.Cache;
using ICWebApp.Application.Cache;
using static System.Net.Mime.MediaTypeNames;

namespace ICWebApp.Application.Provider
{
    public class AUTHProvider : IAUTHProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMSGProvider _msgProvider;
        private readonly ITEXTProvider _textProvider;
        private readonly ISessionWrapper _sessionWrapper;
        private readonly NavigationManager _navManager;
        private readonly ILANGProvider _langProvider;
        private readonly IAUTHProviderCache _authProviderCache;
        private readonly ILANGProviderCache _langProviderCache;

        private AUTH_Municipality? MunicipalityCache;

        public AUTHProvider(IMSGProvider msgProvider, IUnitOfWork _unitOfWork, ITEXTProvider _textProvider, 
                            ISessionWrapper _sessionWrapper, NavigationManager _navManager, ILANGProvider _langProvider,
                            IAUTHProviderCache _authProviderCache, ILANGProviderCache _langProviderCache)
        {
            this._msgProvider = msgProvider;
            this._unitOfWork = _unitOfWork;
            this._textProvider = _textProvider;
            this._sessionWrapper = _sessionWrapper;
            this._navManager = _navManager;
            this._langProvider = _langProvider;
            this._authProviderCache = _authProviderCache;
            this._langProviderCache = _langProviderCache;
        }

        public async Task<MSG_SystemMessages?> Verify(string Username, string Password, Guid? AUTH_Municipality_ID = null)
        {
            if(AUTH_Municipality_ID == null)
            {
                AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();

                if (AUTH_Municipality_ID == null)
                {
                    return null;
                }
            }

            var dbUser = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.Username.ToLower() == Username.ToLower() && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null);

            if(dbUser == null)
            {
                return await _msgProvider.GetSystemMessage("LOGIN_WRONG_USERNAME");
            }

            if(dbUser.RegistrationMode != "custom" && dbUser.RegistrationMode != "Gemeinde Backend")
            {
                return await _msgProvider.GetSystemMessage("LOGIN_WRONG_USERNAME");
            }

            if(dbUser.IsOrganization == true)
            {
                return await _msgProvider.GetSystemMessage("LOGIN_WRONG_USERNAME");
            }

            if (dbUser.LockoutEnabled)
            {
                await LogLoginAttempt(dbUser, false, "USER_DISABLED");

                return await _msgProvider.GetSystemMessage("USER_DISABLED"); 
            }

            PasswordHelper pwHelper = new PasswordHelper();

            var md5Password = pwHelper.CreateMD5Hash(Password);

            if(dbUser.PasswordHash != md5Password)
            {
                await LogLoginAttempt(dbUser, false, "LOGIN_WRONG_PASSWORD");

                return await _msgProvider.GetSystemMessage("LOGIN_WRONG_PASSWORD");
            }

            await LogLoginAttempt(dbUser, true, "LOGIN_SUCCESS");

            return await _msgProvider.GetSystemMessage("LOGIN_SUCCESS");
        }
        public async Task<MSG_SystemMessages?> VerifyMunicipal(string Username, string Password, Guid? AUTH_Municipality_ID = null)
        {
            if (AUTH_Municipality_ID == null)
            {
                AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();

                if (AUTH_Municipality_ID == null)
                {
                    return null;
                }
            }

            var dbUser = await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.Username.ToLower() == Username.ToLower() && p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (dbUser == null)
            {
                return await _msgProvider.GetSystemMessage("LOGIN_WRONG_USERNAME");
            }

            if (dbUser.LockoutEnabled)
            {
                await LogMunicipalLoginAttempt(dbUser, false, "USER_DISABLED");

                return await _msgProvider.GetSystemMessage("USER_DISABLED");
            }

            PasswordHelper pwHelper = new PasswordHelper();

            var md5Password = pwHelper.CreateMD5Hash(Password);

            if (dbUser.PasswordHash != md5Password)
            {
                await LogMunicipalLoginAttempt(dbUser, false, "LOGIN_WRONG_PASSWORD");

                return await _msgProvider.GetSystemMessage("LOGIN_WRONG_PASSWORD");
            }

            await LogMunicipalLoginAttempt(dbUser, true, "LOGIN_SUCCESS");

            return await _msgProvider.GetSystemMessage("LOGIN_SUCCESS");
        }
        public async Task<MSG_SystemMessages?> VerifyEmail(Guid AUTH_Users_ID, string EmailCode)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var dbUser = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == AUTH_Users_ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

                if (dbUser == null)
                {
                    return await _msgProvider.GetSystemMessage("VERIFYEMAIL_ERROR");
                }

                if (dbUser.EmailConfirmToken == EmailCode)
                {
                    dbUser.EmailConfirmed = true;
                    dbUser.ForceEmailVerification = false;

                    await _unitOfWork.Repository<AUTH_Users>().InsertOrUpdateAsync(dbUser);

                    return await _msgProvider.GetSystemMessage("VERIFYEMAIL_SUCCESS");
                }
                else
                {
                    return await _msgProvider.GetSystemMessage("VERIFYEMAIL_WRONG_CODE");
                }
            }

            return await _msgProvider.GetSystemMessage("VERIFYEMAIL_ERROR");
        }
        public async Task<MSG_SystemMessages?> VerifyPasswordResetToken(Guid PasswordResetToken)
        {
            var dbUser = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.PasswordResetToken == PasswordResetToken);

            if (dbUser == null || PasswordResetToken == Guid.Empty)
            {
                return await _msgProvider.GetSystemMessage("PASSWORDRESET_WRONG_TOKEN");
            }

            dbUser.EmailConfirmed = true;
            dbUser.ForceEmailVerification = false;

            await _unitOfWork.Repository<AUTH_Users>().InsertOrUpdateAsync(dbUser);

            return await _msgProvider.GetSystemMessage("PASSWORDRESET_SUCCESS");
        }
        public async Task<MSG_SystemMessages?> VerifyPasswordResetTokenMunicipalUser(Guid PasswordResetToken)
        {
            var dbUser = await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.PasswordResetToken == PasswordResetToken);

            if (dbUser == null || PasswordResetToken == Guid.Empty)
            {
                return await _msgProvider.GetSystemMessage("PASSWORDRESET_WRONG_TOKEN");
            }

            return await _msgProvider.GetSystemMessage("PASSWORDRESET_SUCCESS");
        }
        public async Task<AUTH_Users?> RegisterUser(AUTH_Users User, AUTH_Users_Anagrafic? Anagrafic)
        {
            if (Anagrafic != null && Anagrafic.FiscalNumber != null && User.AUTH_Municipality_ID != null) 
            {
                var anagraficData = await _unitOfWork.Repository<V_AUTH_Users_Anagrafic>().FirstOrDefaultAsync(p => p.FiscalNumber.ToLower() == Anagrafic.FiscalNumber.ToLower() && p.AUTH_Municipality_ID == User.AUTH_Municipality_ID.Value);

                if (anagraficData != null)
                {
                    AUTH_Users? user = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == anagraficData.AUTH_Users_ID && p.AUTH_Municipality_ID == User.AUTH_Municipality_ID.Value && p.RegistrationMode == "Citizen Backend");

                    if (user != null)
                    {
                        user.DA_Email = user.Email;

                        if (user.Firstname != null)
                            user.Firstname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.Firstname.ToLower());

                        if (user.Lastname != null)
                            user.Lastname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.Lastname.ToLower());

                        if (user.Email != null)
                            user.Email = user.Email.ToLower();

                        user.RegistrationMode = User.RegistrationMode;
                        user.PasswordHash = User.PasswordHash;

                        return await _unitOfWork.Repository<AUTH_Users>().InsertOrUpdateAsync(user);
                    }
                }
            }

            if (User.Firstname != null)
                User.Firstname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(User.Firstname.ToLower());

            if (User.Lastname != null)
                User.Lastname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(User.Lastname.ToLower());

            if (User.Email != null)
                User.Email = User.Email.ToLower();

            return await _unitOfWork.Repository<AUTH_Users>().InsertOrUpdateAsync(User);
        }
        public async Task<bool> UpdateUser(AUTH_Users User)
        {
            if (User.Firstname != null)
                User.Firstname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(User.Firstname.ToLower());

            if (User.Lastname != null)
                User.Lastname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(User.Lastname.ToLower());

            if (User.Email != null)
                User.Email = User.Email.ToLower();

            await _unitOfWork.Repository<AUTH_Users>().InsertOrUpdateAsync(User);

            return true;
        }
        public async Task<bool> DisableUser(AUTH_Users User)
        {
            User.LockoutEnabled = true;

            await _unitOfWork.Repository<AUTH_Users>().InsertOrUpdateAsync(User);

            return true;
        }
        public async Task<bool> RemoveUser(AUTH_Users User)
        {
            return await _unitOfWork.Repository<AUTH_Users>().DeleteAsync(User);
        }
        public async Task<bool> SetRole(AUTH_Users User, AUTH_Roles Roles)
        {
            if (User == null || Roles == null)
                return false;

            var itemexists = await _unitOfWork.Repository<AUTH_UserRoles>().FirstOrDefaultAsync(p => p.AUTH_UsersID == User.ID && p.AUTH_RolesID == Roles.ID) != null;

            if (!itemexists)
            {
                await _unitOfWork.Repository<AUTH_UserRoles>().InsertOrUpdateAsync(new AUTH_UserRoles()
                {
                    ID = Guid.NewGuid(),
                    AUTH_UsersID = User.ID,
                    AUTH_RolesID = Roles.ID
                });
            }

            return true;
        }
        public async Task<bool> RemoveRole(AUTH_Users User, AUTH_Roles Roles)
        {
            if (User == null || Roles == null)
                return false;

            var itemexists = await _unitOfWork.Repository<AUTH_UserRoles>().FirstOrDefaultAsync(p => p.AUTH_UsersID == User.ID && p.AUTH_RolesID == Roles.ID);

            if (itemexists != null)
            {
                return  await _unitOfWork.Repository<AUTH_UserRoles>().DeleteAsync(itemexists);
            }

            return false;
        }
        public async Task<AUTH_Users?> GetUser(string Username, Guid? AUTH_Municipality_ID = null)
        {
            if (string.IsNullOrEmpty(Username))
            {
                return null;
            }
            if (AUTH_Municipality_ID == null)
            {
                if (AUTH_Municipality_ID == null)
                {
                    AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }
            }

            return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.Username != null && p.Username.ToLower() == Username.ToLower() && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null);
        }
        public async Task<AUTH_Users?> GetUser(Guid ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.RemovedAt == null);
            }

            return null;
        }
        public AUTH_Users? GetUserSync(Guid ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                return _unitOfWork.Repository<AUTH_Users>().FirstOrDefault(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.RemovedAt == null);
            }

            return null;
        }
        public AUTH_Municipal_Users? GetMunicipalUserSync(Guid ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                return _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefault(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
            }

            return null;
        }
        public async Task<AUTH_Users?> GetUserWithoutMunicipality(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == ID && p.RemovedAt == null);
        }
        public async Task<AUTH_Users?> GetUser(string FiscalCode, string? RegistrationMode, Guid? AUTH_Municipality_ID = null, bool IgnoreRemovedAt = false)
        {
            if (AUTH_Municipality_ID == null)
            {
                if (AUTH_Municipality_ID == null)
                {
                    AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }
            }

            var anagraficData = await _unitOfWork.Repository<V_AUTH_Users_Anagrafic>().FirstOrDefaultAsync(p => p.FiscalNumber.ToLower() == FiscalCode.ToLower() && p.AUTH_Municipality_ID == AUTH_Municipality_ID.Value);

            if(anagraficData != null && RegistrationMode == null)
            {
                return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == anagraficData.AUTH_Users_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID.Value);
            }

            if (anagraficData != null)
            {
                return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == anagraficData.AUTH_Users_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID.Value && p.RegistrationMode == RegistrationMode);
            }

            return null;
        }
        public async Task<AUTH_Users?> GetUserByValidExternalLoginToken(Guid ExternalLoginToken)
        {
            return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ExternalLoginToken != null &&
                                                                                       p.ExternalLoginToken != Guid.Empty &&
                                                                                       p.ExternalLoginDate != null &&
                                                                                       p.ExternalLoginToken == ExternalLoginToken &&
                                                                                       p.ExternalLoginDate.Value.AddMinutes(2) > DateTime.Now);
        }
        public async Task<AUTH_Municipal_Users?> GetMunicipalUserByValidExternalLoginToken(Guid ExternalLoginToken)
        {
            return await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.ExternalLoginToken != null &&
                                                                                                 p.ExternalLoginToken != Guid.Empty &&
                                                                                                 p.ExternalLoginDate != null &&
                                                                                                 p.ExternalLoginToken == ExternalLoginToken &&
                                                                                                 p.ExternalLoginDate.Value.AddMinutes(2) > DateTime.Now);
        }
        public async Task<AUTH_Users?> GetUserByPasswordToken(Guid PasswordResetToken)
        {
            return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.PasswordResetToken == PasswordResetToken);
        }
        public async Task<AUTH_Municipal_Users?> GetMunicipalUserByPasswordToken(Guid PasswordResetToken)
        {
            return await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.PasswordResetToken == PasswordResetToken);
        }        
        public bool EmailExists(string Email, Guid AUTH_Users_ID)
        {
            var dbEmail = _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefault(p => p.Email == Email && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID && p.ID != AUTH_Users_ID);

            if (dbEmail != null && dbEmail.Email == Email)
                return true;

            return false;
        }
        public bool FiscalNumberExists(string FiscalNumber, bool WithManualInput = false)
        {
            var anagraficData = _unitOfWork.Repository<V_AUTH_Users_Anagrafic>().FirstOrDefault(p => p.FiscalNumber.ToLower() == FiscalNumber.ToLower() && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID);

            if (anagraficData != null)
            {
                var user = _unitOfWork.Repository<AUTH_Users>().FirstOrDefault(p => p.ID == anagraficData.AUTH_Users_ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID);

                if (user != null)
                {
                    if (!WithManualInput)
                    {
                        if (user.RegistrationMode == "Citizen Backend")
                        {
                            return false;
                        }
                    }

                    return FiscalNumber == anagraficData.FiscalNumber;
                }                
            }

            return false;
        }
        private async Task<bool> LogLoginAttempt(AUTH_Users User, bool Success, string Code)
        {
            if (User == null)
                return false;

            AUTH_UserLogins newLogin = new AUTH_UserLogins();

            newLogin.AUTH_UsersID = User.ID;
            newLogin.LoginTime = DateTime.Now;
            newLogin.Success = Success;
            newLogin.Code = Code;

            await _unitOfWork.Repository<AUTH_UserLogins>().InsertOrUpdateAsync(newLogin);

            return true;
        }
        private async Task<bool> LogMunicipalLoginAttempt(AUTH_Municipal_Users User, bool Success, string Code)
        {
            if (User == null)
                return false;

            AUTH_Municipal_Users_Logins newLogin = new AUTH_Municipal_Users_Logins();

            newLogin.AUTH_Municipal_Users_ID = User.ID;
            newLogin.LoginTime = DateTime.Now;
            newLogin.Success = Success;
            newLogin.Code = Code;

            await _unitOfWork.Repository<AUTH_Municipal_Users_Logins>().InsertOrUpdateAsync(newLogin);

            return true;
        }
        public async Task<AUTH_Users?> GetUserByLoginToken(Guid LoginToken)
        {
            var data = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.LastLoginToken == LoginToken);

            if (data != null)
            {
                data.DA_Email = data.Email;
            }

            if (data != null && data.LastLoginTimeStamp != null && data.LastLoginTimeStamp.Value.AddMinutes(30) >= DateTime.Now)
            {
                return data;
            }

            return null;
        }
        public async Task<AUTH_Municipal_Users?> GetMunicipalUserByLoginToken(Guid LoginToken)
        {
            var data = await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.LastLoginToken == LoginToken);

            if (data != null && data.LastLoginTimeStamp != null && data.LastLoginTimeStamp.Value.AddMinutes(30) >= DateTime.Now)
            {
                return data;
            }

            return null;
        }
        public async Task<AUTH_Users?> GetUserByForcePasswordResetToken(Guid resetToken)
        {
            return await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(e => e.ForcePwResetToken == resetToken);
        }
        public async Task<bool> SetUserForcePasswordResetSucceeded(Guid userId)
        {
            var user = await _unitOfWork.Repository<AUTH_Users>().GetByIDAsync(userId);
            if (user == null)
                return false;

            user.ForcePasswordReset = false;
            user.ForcePwResetToken = null;
            user.ForcePwResetTokenExpirationData = null;
            return await _unitOfWork.Repository<AUTH_Users>().UpdateAsync(user) != null;
        }
        public async Task<AUTH_Municipal_Users?> GetMunicipalUserByForcePasswordResetToken(Guid resetToken)
        {
            return await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(e => e.ForcePwResetToken == resetToken);
        }
        public async Task<bool> SetMunicipalUserForcePasswordResetSucceeded(Guid userId)
        {
            var user = await _unitOfWork.Repository<AUTH_Municipal_Users>().GetByIDAsync(userId);
            if (user == null)
                return false;

            user.ForcePasswordReset = false;
            user.ForcePwResetToken = null;
            user.ForcePwResetTokenExpirationData = null;
            return await _unitOfWork.Repository<AUTH_Municipal_Users>().UpdateAsync(user) != null;
        }
        public async Task<List<AUTH_Roles>> GetAllRolesByMunicipality(Guid AUTH_Municipality_ID)
        {
            var data = await _unitOfWork.Repository<AUTH_Roles>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Municipality_ID != null);

            foreach(var d in data)
            {
                if (d.TEXT_SystemText_Code != null)
                {
                    d.Description = _textProvider.Get(d.TEXT_SystemText_Code);
                }
            }

            return data;
        }
        public async Task<List<AUTH_Users>> GetUserList(Guid AUTH_Municipality_ID, Guid? AUTH_Role_ID = null, bool Organisations = false)
        {
            var dbData = _unitOfWork.Repository<AUTH_Users>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Municipality_ID != null && p.RemovedAt == null).Include(p => p.AUTH_Users_Anagrafic);

            if(Organisations == true)
            {
                var OrgData = await dbData.Where(p => p.IsOrganization == true).ToListAsync();

                foreach (var r in OrgData)
                {
                    if (r.AUTH_UserSettings != null)
                    {
                        var anagrafic = r.AUTH_Users_Anagrafic.FirstOrDefault();

                        if (anagrafic != null)
                        {
                            r.SearchName = r.Firstname + " " + r.Lastname;
                        }
                    }

                    r.Fullname = r.Firstname + " " + r.Lastname;
                }

                return OrgData;
            }

            var data = await dbData.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname).ToListAsync();

            var result = new List<AUTH_Users>();

            if(AUTH_Role_ID != null)
            {
                foreach(var d in data)
                {
                    var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == d.ID);

                    if(roles.Select(p => p.AUTH_RolesID).Contains(AUTH_Role_ID.Value))
                    {
                        result.Add(d);
                    }
                }
            }
            else
            {
                result = data;
            }

            foreach(var r in result)
            {
                if (r.AUTH_UserSettings != null)
                {
                    var anagrafic = r.AUTH_Users_Anagrafic.FirstOrDefault();

                    if (anagrafic != null)
                    {
                        r.SearchName = r.Firstname + " " + r.Lastname + " (" + anagrafic.FiscalNumber + ")";
                    }
                    else
                    {
                        r.SearchName = r.Firstname + " " + r.Lastname;
                    }
                }

                r.Fullname = r.Firstname + " " + r.Lastname;
            }

            return result;
        }
        public async Task<List<AUTH_UserRoles>> GetUserRolesByMunicipality(Guid AUTH_User_ID, Guid AUTH_Municipality_ID)
        {
            var data = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == AUTH_User_ID);
            var result = new List<AUTH_UserRoles>();

            foreach (var d in data)
            {
                var role = await _unitOfWork.Repository<AUTH_Roles>().GetByIDAsync(d.AUTH_RolesID);

                if (role != null && role.AUTH_Municipality_ID == AUTH_Municipality_ID)
                {
                    if (role.TEXT_SystemText_Code != null)
                    {
                        d.RoleName = _textProvider.Get(role.TEXT_SystemText_Code);
                    }
                    result.Add(d);
                }
            }

            result = result.OrderBy(p => p.RoleName).ToList();

            return result;
        }
        public List<AUTH_UserRoles> GetUserRoles()
        {
            if (_sessionWrapper != null && _sessionWrapper.CurrentUser != null)
            {
                var roles = _unitOfWork.Repository<AUTH_UserRoles>().ToList(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                return roles;
            }
            return new List<AUTH_UserRoles>();
        }
        public bool HasUserRole(Guid RoleID)
        {
            if (_sessionWrapper != null && _sessionWrapper.CurrentUser != null)
            {
                var roles = _unitOfWork.Repository<AUTH_UserRoles>().ToList(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (roles != null && roles.Select(p => p.AUTH_RolesID).Contains(RoleID))
                {
                    return true;
                }
            }
            return false;
        }
        public bool HasUserRole(Guid AUTH_Users_ID, Guid RoleID)
        {
            var roles = _unitOfWork.Repository<AUTH_UserRoles>().ToList(p => p.AUTH_UsersID == AUTH_Users_ID);

            if (roles != null && roles.Select(p => p.AUTH_RolesID).Contains(RoleID))
            {
                return true;
            }

            return false;
        }
        public async Task<List<AUTH_Roles>> GetRoles()
        {
            return await _unitOfWork.Repository<AUTH_Roles>().ToListAsync();
        }
        public async Task<List<AUTH_Municipality>> GetMunicipalityList()
        {
            var data = await _unitOfWork.Repository<AUTH_Municipality>().ToListAsync();

            foreach(var d in data)
            {
                d.Name = _textProvider.Get(d.Name_Text_SystemTexts_Code);
                d.Prefix = _textProvider.Get(d.Prefix_Text_SystemTexts_Code);
            }

            data = data.OrderBy(p => p.Name).ToList();

            return data;
        }
        public async Task<bool> SetRole(Guid AUTH_Users_ID, Guid AUTH_Roles_ID)
        {
            var itemexists = await _unitOfWork.Repository<AUTH_UserRoles>().FirstOrDefaultAsync(p => p.AUTH_UsersID == AUTH_Users_ID && p.AUTH_RolesID == AUTH_Roles_ID) != null;

            if (!itemexists)
            {
                await _unitOfWork.Repository<AUTH_UserRoles>().InsertOrUpdateAsync(new AUTH_UserRoles()
                {
                    ID = Guid.NewGuid(),
                    AUTH_UsersID = AUTH_Users_ID,
                    AUTH_RolesID = AUTH_Roles_ID
                });

                return true;
            }

            return false;
        }
        public async Task<bool> RemoveRole(Guid AUTH_Users_ID, Guid AUTH_Roles_ID)
        {
            var item = await _unitOfWork.Repository<AUTH_UserRoles>().FirstOrDefaultAsync(p => p.AUTH_UsersID == AUTH_Users_ID && p.AUTH_RolesID == AUTH_Roles_ID);

            if (item != null)
            {
                return await _unitOfWork.Repository<AUTH_UserRoles>().DeleteAsync(item);
            }

            return false;
        }
        public async Task<List<AUTH_Authority>> GetAuthorityList(Guid AUTH_Municipality_ID, bool? Official = true, bool? DynamicForms = true)
        {
            var data = _unitOfWork.Repository<AUTH_Authority>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (Official != null)
            {
                data = data.Where(p => p.IsOfficial == Official);
            }
            if (DynamicForms != null)
            {
                data = data.Where(p => p.DynamicForms == DynamicForms);
            }

            foreach (var d in data)
            {
                if (d.TEXT_SystemText_Code != null)
                {
                    d.Description = _textProvider.Get(d.TEXT_SystemText_Code);
                    d.ShortText = _textProvider.Get(d.TEXT_SystemText_Code_Description);
                }
            }

            return await data.OrderBy(p => p.Name).ToListAsync();
        }
        public async Task<List<V_HOME_Authority>> GetHomeAuthorityList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool? Official = true, bool? DynamicForms = true)
        {
            var data = _unitOfWork.Repository<V_HOME_Authority>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            if (Official != null)
            {
                data = data.Where(p => p.IsOfficial == Official);
            }
            if (DynamicForms != null)
            {
                data = data.Where(p => p.DynamicForms == DynamicForms);
            }

            return await data.OrderBy(p => p.Title).ToListAsync();
        }
        public async Task<AUTH_Authority?> GetAuthority(Guid ID)
        {
            var data = await _unitOfWork.Repository<AUTH_Authority>().GetByIDAsync(ID);

            if (data != null && data.TEXT_SystemText_Code != null)
            {
                data.Description = _textProvider.Get(data.TEXT_SystemText_Code);
                data.ShortText = _textProvider.Get(data.TEXT_SystemText_Code_Description);
            }

            return data;
        }
        public async Task<AUTH_Authority?> GetAuthorityMensa(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.IsMensa == true);
        }
        public async Task<AUTH_Authority?> GetAuthorityRooms(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.IsRooms == true);
        }
        public async Task<AUTH_Authority?> GetAuthorityMaintenance(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.IsMantainence == true);
        }
        public async Task<List<AUTH_Authority_Extended>> GetAuthorityExtended(Guid AUTH_Authority_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Extended>().ToListAsync(p => p.AUTH_Authority_ID == AUTH_Authority_ID);
        }
        public async Task<AUTH_Municipality?> GetMunicipality(Guid ID)
        {
            if (MunicipalityCache != null)
                return MunicipalityCache;

            var data = _unitOfWork.Repository<AUTH_Municipality>().Where(p => p.ID == ID).FirstOrDefault();

            if (data != null)
            {
                data.Name = _textProvider.Get(data.Name_Text_SystemTexts_Code);
                data.Prefix = _textProvider.Get(data.Prefix_Text_SystemTexts_Code);
            }

            MunicipalityCache = data;

            return data;
        }
        public AUTH_Municipality? GetMunicipalitySync(Guid ID)
        {
            if (MunicipalityCache != null)
                return MunicipalityCache;

            var data = _unitOfWork.Repository<AUTH_Municipality>().Where(p => p.ID == ID).FirstOrDefault();

            if (data != null)
            {
                data.Name = _textProvider.Get(data.Name_Text_SystemTexts_Code);
                data.Prefix = _textProvider.Get(data.Prefix_Text_SystemTexts_Code);
            }

            MunicipalityCache = data;

            return data;
        }
        public async Task<AUTH_Municipality?> SetMunicipality(AUTH_Municipality Data)
        {
            return await _unitOfWork.Repository<AUTH_Municipality>().InsertOrUpdateAsync(Data);
        }
        public AUTH_Municipality? SetMunicipalitySync(AUTH_Municipality Data)
        {
            return _unitOfWork.Repository<AUTH_Municipality>().InsertOrUpdate(Data);
        }
        public async Task<AUTH_Users_Anagrafic?> GetAnagraficByFiscalCode(Guid AUTH_Municipality_ID, string FiscalCode)
        {
            return await _unitOfWork.Repository<AUTH_Users_Anagrafic>().Where(p => p.FiscalNumber == FiscalCode && p.AUTH_Users != null 
                                                                                && p.AUTH_Users.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.AUTH_Users).FirstOrDefaultAsync();
        }
        public async Task<AUTH_Users_Anagrafic?> GetAnagraficByVatNumber(Guid AUTH_Municipality_ID, string VatNumber)
        {
            return await _unitOfWork.Repository<AUTH_Users_Anagrafic>().Where(p => p.VatNumber.ToLower() == VatNumber.ToLower() && p.AUTH_Users != null 
                                                                           && p.AUTH_Users.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.AUTH_Users).FirstOrDefaultAsync();
        }
        public async Task<AUTH_Users_Anagrafic?> GetAnagraficByID(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Users_Anagrafic>().GetByIDAsync(ID);
        }
        public async Task<AUTH_Users_Anagrafic?> GetAnagraficByUserID(Guid AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<AUTH_Users_Anagrafic>().FirstOrDefaultAsync(p => p.AUTH_Users_ID == AUTH_Users_ID);
        }
        public async Task<AUTH_Users_Anagrafic?> SetAnagrafic(AUTH_Users_Anagrafic Data)
        {
            if(Data != null)
            {

                if (Data.FirstName != null)
                {
                    Data.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.FirstName.ToLower());
                    Data.FirstName = Data.FirstName.Replace(" Gmbh", " GmbH");
                    Data.FirstName = Data.FirstName.Replace(" Eo ", " EO ");
                    if (Data.FirstName.EndsWith(" Eo")) ;
                        Data.FirstName = Data.FirstName.Replace(" Eo", " EO");

                }

                if (Data.LastName != null)
                {
                    Data.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.LastName.ToLower());
                    Data.LastName = Data.LastName.Replace(" Gmbh", " GmbH");
                    Data.LastName = Data.LastName.Replace(" Eo ", " EO ");
                    if (Data.LastName.EndsWith(" Eo")) ;
                    Data.LastName = Data.LastName.Replace(" Eo", " EO");
                }

                if (Data.CountyOfBirth != null)
                    Data.CountyOfBirth = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.CountyOfBirth.ToLower());

                if (Data.PlaceOfBirth != null)
                    Data.PlaceOfBirth = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.PlaceOfBirth.ToLower());

                if (Data.Address != null)
                    Data.Address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.Address.ToLower());

                if (Data.DomicileStreetAddress != null)
                    Data.DomicileStreetAddress = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.DomicileStreetAddress.ToLower());

                if (Data.Email != null)
                    Data.Email = Data.Email.ToLower();

                return await _unitOfWork.Repository<AUTH_Users_Anagrafic>().InsertOrUpdateAsync(Data);
            }

            return null;
        }
        public async Task<AUTH_External_Verification?> GetVerification(string FiscalCode, Guid LoginToken)
        {
            var SkipToken = Guid.Parse("D9812FDF-555F-42AE-B173-EF589C14F20B");
            return await _unitOfWork.Repository<AUTH_External_Verification>().FirstOrDefaultAsync(p => p.FiscalNumber == FiscalCode && p.LoginToken == LoginToken && ((p.CompletedAt == null && p.Timeout >= DateTime.Now) || LoginToken == SkipToken));
        }
        public async Task<AUTH_External_Verification?> GetVerification(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_External_Verification>().FirstOrDefaultAsync(p => p.ID == ID && p.CompletedAt == null && p.Timeout >= DateTime.Now);
        }
        public async Task<AUTH_External_Verification?> SetVerification(AUTH_External_Verification Data)
        {
            return await _unitOfWork.Repository<AUTH_External_Verification>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> CheckUserAnagraficInformation(Guid AUTH_Users_ID)
        {
            var dbUserAnagrafic = await GetAnagraficByUserID(AUTH_Users_ID);
            var user = await GetUser(AUTH_Users_ID);

            if (user != null && user.RegistrationMode != "Gemeinde Backend" && dbUserAnagrafic != null)
            {
                if (String.IsNullOrEmpty(dbUserAnagrafic.Email))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(dbUserAnagrafic.Phone) && String.IsNullOrEmpty(dbUserAnagrafic.MobilePhone))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(dbUserAnagrafic.Address))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(dbUserAnagrafic.DomicileStreetAddress))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(dbUserAnagrafic.DomicilePostalCode))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(dbUserAnagrafic.DomicileMunicipality))
                {
                    return false;
                }

            }

            return true;
        }
        public async Task<bool> CheckEmailVerification(Guid AUTH_Users_ID)
        {
            var dbUser = await GetUser(AUTH_Users_ID);

            if (dbUser != null)
            {
                if(dbUser.ForceEmailVerification == true)
                {
                    return false;
                }

                if (dbUser.RegistrationMode != "custom")
                {
                    return true;
                }

                if (dbUser.EmailConfirmed == true)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> CheckPhoneVerification(Guid AUTH_Users_ID)
        {
            var dbUser = await GetUser(AUTH_Users_ID);

            if (dbUser != null)
            {
                if (dbUser.ForcePhoneVerification == true)
                {
                    return false;
                }

                if (dbUser.RegistrationMode != "custom")
                {
                    return true;
                }

                if (dbUser.PhoneNumberConfirmed)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> CheckVeriffVerification(Guid AUTH_Users_ID)
        {
            var dbUser = await GetUser(AUTH_Users_ID);

            if (dbUser != null)
            {
                if (dbUser.RegistrationMode != "custom")
                {
                    return true;
                }

                if (dbUser.VeriffConfirmed)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> CheckPrivacyAccepted(Guid userId)
        {
            var dbUser = await GetUser(userId);

            if (dbUser != null)
            {
                if (dbUser.RegistrationMode != "spid")
                    return true;
                if (dbUser.PrivacyAccepted != null)
                    return true;
            }
            return false;
        }        
        public async Task<bool> CheckForcePasswordReset(Guid AUTH_Users_ID)
        {
            var dbUser = await GetUser(AUTH_Users_ID);
            if (dbUser != null)
            {
                return dbUser.ForcePasswordReset;
            }
            return false;
        }
        public async Task<bool> CheckMunicipalUserForcePasswordReset(Guid AUTH_Municipal_Users_ID)
        {
            var dbUser = await GetMunicipalUser(AUTH_Municipal_Users_ID);
            if (dbUser != null)
            {
                return dbUser.ForcePasswordReset;
            }
            return false;
        }
        public async void CheckUserVerifications(Guid? AUTH_Users_ID)
        {
            if (AUTH_Users_ID != null)
            {
                var authAnagrafic = await _unitOfWork.Repository<AUTH_Users_Anagrafic>().FirstOrDefaultAsync(p => p.AUTH_Users_ID == AUTH_Users_ID);

                if (authAnagrafic != null && authAnagrafic.IgnoreDataVerification == true)
                    return;

                if (!_navManager.Uri.Contains("VerifiedEmail") && !_navManager.Uri.Contains("/User/Profile") && !_navManager.Uri.Contains("AcceptRegistrationPrivacy"))
                {
                    if (!await CheckUserAnagraficInformation(AUTH_Users_ID.Value) && _sessionWrapper.CurrentSubstituteUser == null && !_navManager.Uri.Contains("/User/Profile"))
                    {
                        _navManager.NavigateTo("/User/Profile", true);

                        return;
                    }

                    if (!await CheckEmailVerification(AUTH_Users_ID.Value))
                    {
                        if (!_navManager.Uri.Contains("/VerifyEmail"))
                            _navManager.NavigateTo("/VerifyEmail/" + AUTH_Users_ID, true);

                        return;
                    }

                    if (!await CheckPhoneVerification(AUTH_Users_ID.Value))
                    {
                        if(!_navManager.Uri.Contains("/VerifyPhone"))
                            _navManager.NavigateTo("/VerifyPhone/" + AUTH_Users_ID, true);

                        return;
                    }

                    if (!await CheckVeriffVerification(AUTH_Users_ID.Value))
                    {
                        if(!_navManager.Uri.Contains("/Veriff"))
                            _navManager.NavigateTo("/Veriff/" + AUTH_Users_ID, true);

                        return;
                    }

                    if (!await CheckPrivacyAccepted(AUTH_Users_ID.Value))
                    {
                        if(!_navManager.Uri.Contains("/AcceptRegistrationPrivacy"))
                            _navManager.NavigateTo("/AcceptRegistrationPrivacy/" + AUTH_Users_ID, true);

                        return;
                    }
                    
                    if (await CheckForcePasswordReset(AUTH_Users_ID.Value))
                    {
                        var resetToken = await CreateForceResetTokenForUser(AUTH_Users_ID.Value, TimeSpan.FromMinutes(5));
                        if (resetToken != null)
                        {
                            if (!_navManager.Uri.Contains("/ForcePasswordReset/"))
                            {
                                _navManager.NavigateTo("/ForcePasswordReset/" + resetToken);
                                return;
                            }
                        }
                        
                    }
                }
            }
        }
        public async void CheckMunicipalUserVerifications(Guid? AUTH_Municipal_Users_ID)
        {
            if (AUTH_Municipal_Users_ID != null)
            {
                if (await CheckMunicipalUserForcePasswordReset(AUTH_Municipal_Users_ID.Value))
                {
                    var resetToken = await CreateForceResetTokenForMunicipalUser(AUTH_Municipal_Users_ID.Value, TimeSpan.FromMinutes(5));
                    if (resetToken != null)
                    {
                        if (!_navManager.Uri.Contains("/ForcePasswordReset/"))
                        {
                            _navManager.NavigateTo("/ForcePasswordReset/" + resetToken);
                            return;
                        }
                    }

                }
            }
        }
        public async Task<Guid?> CreateForceResetTokenForUser(Guid userId, TimeSpan validityPeriod)
        {
            var user = await _unitOfWork.Repository<AUTH_Users>().GetByIDAsync(userId);
            if (user != null)
            {
                user.ForcePasswordReset = true;
                user.ForcePwResetToken = Guid.NewGuid();
                user.ForcePwResetTokenExpirationData = DateTime.Now + validityPeriod;
                await _unitOfWork.Repository<AUTH_Users>().UpdateAsync(user);
                return user.ForcePwResetToken;
            }
            return null;
        }
        public async Task<Guid?> CreateForceResetTokenForMunicipalUser(Guid userId, TimeSpan validityPeriod)
        {
            var user = await _unitOfWork.Repository<AUTH_Municipal_Users>().GetByIDAsync(userId);
            if (user != null)
            {
                user.ForcePasswordReset = true;
                user.ForcePwResetToken = Guid.NewGuid();
                user.ForcePwResetTokenExpirationData = DateTime.Now + validityPeriod;
                await _unitOfWork.Repository<AUTH_Municipal_Users>().UpdateAsync(user);
                return user.ForcePwResetToken;
            }
            return null;
        }
        public async Task<bool> SetLastForceEmailSent(Guid userId, DateTime time)
        {
            var user = await _unitOfWork.Repository<AUTH_Users>().GetByIDAsync(userId);
            if (user != null)
            {
                user.LastForceResetMailSent = time;
                return await _unitOfWork.Repository<AUTH_Users>().UpdateAsync(user) != null;
            }
            return false;
        }
        public async Task<MSG_SystemMessages?> VerifyPhone(Guid AUTH_Users_ID, string PhoneCode)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null) 
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var dbUser = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == AUTH_Users_ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

                if (dbUser == null)
                {
                    return await _msgProvider.GetSystemMessage("VERIFYPHONE_ERROR");
                }

                if (dbUser.PhoneNumberConfirmToken == PhoneCode)
                {
                    dbUser.PhoneNumberConfirmed = true;
                    dbUser.ForcePhoneVerification = false;

                    await _unitOfWork.Repository<AUTH_Users>().InsertOrUpdateAsync(dbUser);

                    return await _msgProvider.GetSystemMessage("VERIFYPHONE_SUCCESS");
                }
                else
                {
                    return await _msgProvider.GetSystemMessage("VERIFYPHONE_WRONG_CODE");
                }
            }

            return await _msgProvider.GetSystemMessage("VERIFYPHONE_ERROR");
        }
        public async Task<AUTH_VeriffResponse?> SetVeriffResponse(AUTH_VeriffResponse Data)
        {
            return await _unitOfWork.Repository<AUTH_VeriffResponse>().InsertOrUpdateAsync(Data);
        }
        public async Task<AUTH_VeriffResponse?> GetVeriffResponse(Guid AUTH_Users_ID)
        {
            var dbUser = await _unitOfWork.Repository<AUTH_Users>().GetByIDAsync(AUTH_Users_ID);

            if (dbUser != null)
            {
                var data = await _unitOfWork.Repository<AUTH_VeriffResponse>().ToListAsync(p => p.CreationDate >= dbUser.VeriffStartDate && p.AUTH_Users_ID == AUTH_Users_ID);

                return data.OrderByDescending(p => p.CreationDate).FirstOrDefault();
            }

            return null;
        }
        public async Task<List<MunicipalityDomainSelectableItem>> GetProgrammPrefixes()
        {
            var cache = _authProviderCache.Get();

            if (!cache.Any())
            {
                var data = _unitOfWork.Repository<AUTH_Municipality>().ToList();

                var result = new List<MunicipalityDomainSelectableItem>();
                var Languages = await _langProvider.GetAll();

                if (Languages != null)
                {
                    foreach (var l in Languages)
                    {
                        foreach (var d in data)
                        {
                            result.Add(new MunicipalityDomainSelectableItem()
                            {
                                Prefix = _textProvider.Get(d.Prefix_Text_SystemTexts_Code, l.ID),
                                AUTH_Municipality_ID = d.ID
                            });
                        }
                    }
                }
                _authProviderCache.Set(result);

                return result;
            }

            return cache;
        }       
        public async Task<V_AUTH_Authority_Statistik?> GetAuthorityStatistik(Guid AUTH_Authority_ID, Guid? AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<V_AUTH_Authority_Statistik>().FirstOrDefaultAsync(p => p.ID == AUTH_Authority_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<List<AUTH_Users>> GetUserOrganizations(Guid UserID, Guid AUTH_Municipality_ID)
        {
            var orgList = await _unitOfWork.Repository<AUTH_ORG_Users>().ToListAsync(p => p.AUTH_Users_ID == UserID);

            return await _unitOfWork.Repository<AUTH_Users>().ToListAsync(p => orgList.Select(x => x.ORG_AUTH_Users_ID).Contains(p.ID) && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<AUTH_ORG_Users?> GetUserOrganization(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_ORG_Users>().GetByIDAsync(ID);
        }
        public async Task<AUTH_ORG_Users?> GetUserOrganization(Guid AUTH_ORG_ID, Guid AUTH_User_ID)
        {
            return await _unitOfWork.Repository<AUTH_ORG_Users>().FirstOrDefaultAsync(p => p.ORG_AUTH_Users_ID == AUTH_ORG_ID && p.AUTH_Users_ID == AUTH_User_ID);
        }
        public async Task<AUTH_ORG_Users?> SetUserOrganization(AUTH_ORG_Users Data)
        {
            return await _unitOfWork.Repository<AUTH_ORG_Users>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveUserOrganization(AUTH_ORG_Users Data)
        {
            return await _unitOfWork.Repository<AUTH_ORG_Users>().DeleteAsync(Data);
        }
        public async Task<List<AUTH_ORG_Roles>?> GetORGRoles(Guid AUTH_Company_Type_ID)
        {
            var data = await _unitOfWork.Repository<AUTH_ORG_Roles>().ToListAsync();

            if (data != null)
            {
                foreach (var d in data)
                {
                    if (d.TEXT_SystemTexts_Code != null)
                    {
                        d.Description = _textProvider.Get(d.TEXT_SystemTexts_Code);
                    }
                }
            }
            return data;
        }
        public async Task<List<AUTH_Company_Type>?> GetCompanyType()
        {
            var data = await _unitOfWork.Repository<AUTH_Company_Type>().ToListAsync();

            if (data != null)
            {
                foreach (var d in data)
                {
                    if (d.TEXT_System_Texts_Code != null)
                    {
                        d.Description = _textProvider.Get(d.TEXT_System_Texts_Code);
                    }
                }
            }
            return data;
        }
        public async Task<List<V_AUTH_Company_LegalForm>?> GetCompanyLegalForms()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            var data = await _unitOfWork.Repository<V_AUTH_Company_LegalForm>().ToListAsync(p => p.LANG_LanguagesID == Language.ID);

            return data;
        }
        public async Task<List<V_AUTH_Company_LegalForm>?> GetCompanyLegalForms(Guid AUTH_Company_Type_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            var data = await _unitOfWork.Repository<V_AUTH_Company_LegalForm>().ToListAsync(p => p.LANG_LanguagesID == Language.ID && p.AUTH_Company_Type_ID == AUTH_Company_Type_ID);

            return data;
        }
        public async Task<List<V_AUTH_Organizations>> GetUserOrganizations(string FiscalCode, Guid AUTH_Municipality_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<V_AUTH_Organizations>().ToListAsync(p => p.SearchText != null && p.SearchText.Contains(FiscalCode) &&
                                                                                       p.LANG_LanguagesID == Language.ID &&
                                                                                       p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<AUTH_External_Error?> SetVerificationError(AUTH_External_Error Data)
        {
            return await _unitOfWork.Repository<AUTH_External_Error>().InsertAsync(Data);
        }
        public async Task<AUTH_External_Error?> GetVerificationError(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_External_Error>().GetByIDAsync(ID);
        }
        public async Task<List<V_AUTH_Company_Type>> GetVCompanyType()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            var data = await _unitOfWork.Repository<V_AUTH_Company_Type>().ToListAsync(p => p.LANG_LanguagesID == Language.ID);

            return data.OrderBy(p => p.Text).ToList();
        }
        public async Task<List<V_AUTH_Users_Organizations>> GetUsersOrganizations(Guid AUTH_Users_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            var data = await _unitOfWork.Repository<V_AUTH_Users_Organizations>().ToListAsync(p => p.LANG_LanguagesID == Language.ID && p.AUTH_Users_ID == AUTH_Users_ID && p.DeaktivatedAt == null);

            return data.OrderBy(p => p.ORG_Fullname).ToList();
        }
        public async Task<List<V_AUTH_Users_Organizations>> GetOrganizationsUsers(Guid AUTH_ORG_Users_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            var data = await _unitOfWork.Repository<V_AUTH_Users_Organizations>().ToListAsync(p => p.LANG_LanguagesID == Language.ID && p.ORG_AUTH_Users_ID == AUTH_ORG_Users_ID && p.DeaktivatedAt == null);

            return data.OrderBy(p => p.ORG_Fullname).ToList();
        }
        public async Task<List<V_AUTH_Organizations>> GetOrganizations(Guid AUTH_Municipality_ID, Administration_Filter_Organization Filter)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Filter != null && !string.IsNullOrEmpty(Filter.Text))
            {
                var data = await _unitOfWork.Repository<V_AUTH_Organizations>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_LanguagesID == Language.ID
                                                                                     && p.SearchText != null && p.SearchText.ToLower().Contains(Filter.Text.ToLower()));

                return data.OrderBy(p => p.Fullname).ToList();
            }

            var result = await _unitOfWork.Repository<V_AUTH_Organizations>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_LanguagesID == Language.ID);

            return result.OrderBy(p => p.Fullname).ToList();
        }
        public async Task<List<V_AUTH_ORG_Role>> GetVOrgRoles()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<V_AUTH_ORG_Role>().ToListAsync(p => p.LANG_LanguagesID == Language.ID);
        }
        public async Task<List<AUTH_MunicipalityApps>> GetMunicipalityApps(bool IncludePreparations = false)
        {
            if(_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<AUTH_MunicipalityApps>();
            }

            if (_sessionWrapper.CurrentUser != null) 
            {
                var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (roles != null && (roles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Employee) || roles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Administrator)))
                {
                    return await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                }
            }

            if(IncludePreparations == true)
            {
                return await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || p.InPreparation == true));
            }

            return await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);
        }
        public async Task<List<V_AUTH_BolloFree_Reason>> GetVBolloFreeReasons()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            var data = await _unitOfWork.Repository<V_AUTH_BolloFree_Reason>().ToListAsync(p => p.LANG_LanguagesID == Language.ID);

            return data.OrderBy(p => p.SortOrder).ToList();
        }
        public async Task<AUTH_UserSettings?> GetSettings(Guid UserID)
        {
            var sett = await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(p => p.AUTH_UsersID == UserID);

            if(sett == null)
            {
                return new AUTH_UserSettings()
                {
                    AUTH_UsersID = UserID,
                    LANG_Languages_ID = LanguageSettings.German
                };
            }

            return sett;
        }
        public async Task<List<AUTH_Municipality_Page_Subtitles>?> GetPageSubtitles(Guid AUTH_Municipality_ID, int TypeID)
        {
            return await _unitOfWork.Repository<AUTH_Municipality_Page_Subtitles>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.TypeID == TypeID).ToListAsync();
        }
        public async Task<AUTH_Municipality_Page_Subtitles?> SetPageSubtitle(AUTH_Municipality_Page_Subtitles Data)
        {
            return await _unitOfWork.Repository<AUTH_Municipality_Page_Subtitles>().InsertOrUpdateAsync(Data);
        }
        public async Task<AUTH_Municipality_Page_Subtitles?> GetCurrentPageSubTitleAsync(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, int TypeID)
        {
            return await _unitOfWork.Repository<AUTH_Municipality_Page_Subtitles>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.TypeID == TypeID).FirstOrDefaultAsync();
        }
        public AUTH_Municipality_Page_Subtitles? GetCurrentPageSubTitle(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, int TypeID)
        {
            return _unitOfWork.Repository<AUTH_Municipality_Page_Subtitles>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.TypeID == TypeID).FirstOrDefault();
        }
        public async Task<AUTH_Spid_Log?> SetSpidLog(AUTH_Spid_Log Data)
        {
            return await _unitOfWork.Repository<AUTH_Spid_Log>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<AUTH_Authority>> GetAlllowedAuthoritiesByMunicipalUser(Guid AUTH_Municipal_User_ID)
        {
            var userAuthorities = await GetMunicipalUserAuthorities(AUTH_Municipal_User_ID);
            var guids = userAuthorities.Where(e => e.AUTH_Authority_ID != null).Select(e => e.AUTH_Authority_ID!.Value);
            return await _unitOfWork.Repository<AUTH_Authority>()
                .Where(e => guids.Any(g => g == e.ID)).ToListAsync();
        }
        public async Task<Guid?> GetLanguageIdFromDomain(string baseUri, Guid AUTH_Municipality_ID)
        {
            var data = _authProviderCache.GetPrefixes();

            if (data.Any())
            {
                var item = data.FirstOrDefault(p => baseUri.Contains("/" + p.Prefix + "."));

                if(item != null)
                {
                    return item.LangID;
                }

                if (!_authProviderCache.GetUrls().Any())
                    _authProviderCache.SetUrls(await _unitOfWork.Repository<AUTH_Comunix_Urls>().ToListAsync());

                var urls = _authProviderCache.GetUrls();

                if (urls.Any())
                {
                    var urlItem = urls.FirstOrDefault(p => baseUri.Contains(p.Url));

                    if(urlItem != null)
                    {
                        return urlItem.DefaultLanguage;
                    }
                }

                return LanguageSettings.Italian;
            }

            var municipalities = await _unitOfWork.Repository<AUTH_Municipality>().Where(p => p.ID == AUTH_Municipality_ID).ToListAsync();
            var results = new List<AUTH_Municipality>();

            var langs = _langProviderCache.Get();

            if (langs != null)
            {
                foreach (var mun in municipalities)
                {
                    foreach (var lang in langs)
                    {
                        var text = _textProvider.Get(mun.Prefix_Text_SystemTexts_Code, lang.ID);

                        mun.Prefix = text;
                        mun.LangID = lang.ID;

                        results.Add(mun);
                    }
                }
            }

            _authProviderCache.SetPrefixes(results);

            return null;
        }
        public AUTH_Municipality_Footer? GetMunicipalityFooter(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<AUTH_Municipality_Footer>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).FirstOrDefault();
        }
        public async Task<AUTH_Municipal_Users?> GetMunicipalUser(string Username, Guid? AUTH_Municipality_ID = null)
        {
            if (string.IsNullOrEmpty(Username))
            {
                return null;
            }
            if (AUTH_Municipality_ID == null)
            {
                if (AUTH_Municipality_ID == null)
                {
                    AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }
            }

            return await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.Username != null && p.Username.ToLower() == Username.ToLower() && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<AUTH_Municipal_Users?> GetMunicipalUser(Guid ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                return await _unitOfWork.Repository<AUTH_Municipal_Users>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
            }

            return null;
        }
        public async Task<bool> UpdateMunicipalUser(AUTH_Municipal_Users User)
        {
            if (User.Firstname != null)
                User.Firstname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(User.Firstname.ToLower());

            if (User.Lastname != null)
                User.Lastname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(User.Lastname.ToLower());

            if (User.Email != null)
                User.Email = User.Email.ToLower();

            await _unitOfWork.Repository<AUTH_Municipal_Users>().InsertOrUpdateAsync(User);

            return true;
        }
        public async Task<bool> DisableMunicipalUser(AUTH_Municipal_Users User)
        {
            User.LockoutEnabled = true;

            await _unitOfWork.Repository<AUTH_Municipal_Users>().InsertOrUpdateAsync(User);

            return true;
        }
        public async Task<bool> RemoveMunicipalUser(AUTH_Municipal_Users User)
        {
            return await _unitOfWork.Repository<AUTH_Municipal_Users>().DeleteAsync(User);
        }
        public async Task<bool> SetMunicipaRole(AUTH_Municipal_Users User, AUTH_Roles Roles)
        {
            if (User == null || Roles == null)
                return false;

            var itemexists = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().FirstOrDefaultAsync(p => p.AUTH_Municipal_Users_ID == User.ID && p.AUTH_Roles_ID == Roles.ID) != null;

            if (!itemexists)
            {
                await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().InsertOrUpdateAsync(new AUTH_Municipal_Users_Roles()
                {
                    ID = Guid.NewGuid(),
                    AUTH_Municipal_Users_ID = User.ID,
                    AUTH_Roles_ID = Roles.ID
                });
            }

            return true;
        }
        public async Task<bool> SetMunicipaRole(Guid AUTH_Municipal_Users_ID, Guid AUTH_Roles_ID)
        {
            var itemexists = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().FirstOrDefaultAsync(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID && p.AUTH_Roles_ID == AUTH_Roles_ID) != null;

            if (!itemexists)
            {
                await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().InsertOrUpdateAsync(new AUTH_Municipal_Users_Roles()
                {
                    ID = Guid.NewGuid(),
                    AUTH_Municipal_Users_ID = AUTH_Municipal_Users_ID,
                    AUTH_Roles_ID = AUTH_Roles_ID
                });

                return true;
            }

            return false;
        }
        public async Task<List<AUTH_Municipal_Users>> GetMunicipalUserList(Guid AUTH_Municipality_ID, Guid? AUTH_Role_ID = null, bool Organisations = false)
        {
            var dbData = _unitOfWork.Repository<AUTH_Municipal_Users>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Municipality_ID != null);

            var data = await dbData.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname).ToListAsync();

            var result = new List<AUTH_Municipal_Users>();

            if (AUTH_Role_ID != null)
            {
                foreach (var d in data)
                {
                    var roles = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToListAsync(p => p.AUTH_Municipal_Users_ID == d.ID);

                    if (roles.Select(p => p.AUTH_Roles_ID).Contains(AUTH_Role_ID.Value))
                    {
                        result.Add(d);
                    }
                }
            }
            else
            {
                result = data;
            }

            return result;
        }
        public List<AUTH_Municipal_Users_Roles> GetMunicipalUserRoles()
        {
            if (_sessionWrapper != null && _sessionWrapper.CurrentMunicipalUser != null)
            {
                var roles = _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToList(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                return roles;
            }
            return new List<AUTH_Municipal_Users_Roles>();
        }
        public async Task<List<AUTH_Municipal_Users_Authority>> GetMunicipalUserAuthorities(Guid AUTH_Municipal_User_ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == AUTH_Municipal_User_ID);

                var superroles = roles.Where(p => p.AUTH_RolesID == AuthRoles.Administrator).ToList();

                var authorities = await GetAuthorityList(_sessionWrapper.AUTH_Municipality_ID.Value, null, null);
                var auth = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().ToListAsync(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_User_ID);

                if (superroles != null && superroles.Count > 0 && (auth == null || auth.Count() == 0))
                {
                    var result = new List<AUTH_Municipal_Users_Authority>();

                    foreach (var authority in authorities)
                    {
                        var ua = new AUTH_Municipal_Users_Authority()
                        {
                            ID = Guid.NewGuid(),
                            AuthorityName = authority.Name,
                            AUTH_Municipal_Users_ID = AUTH_Municipal_User_ID,
                            AUTH_Authority_ID = authority.ID,
                            Removed = false
                        };

                        if (authority != null && authority.TEXT_SystemText_Code != null)
                        {
                            ua.AuthorityName = _textProvider.Get(authority.TEXT_SystemText_Code);
                        }

                        result.Add(ua);
                    }

                    return result;
                }


                foreach (var a in auth)
                {
                    var authority = authorities.FirstOrDefault(p => p.ID == a.AUTH_Authority_ID);

                    if (authority != null && authority.TEXT_SystemText_Code != null)
                    {
                        a.AuthorityName = _textProvider.Get(authority.TEXT_SystemText_Code);
                    }
                }

                return auth;
            }

            return new List<AUTH_Municipal_Users_Authority>();
        }
        public async Task<bool> SetMunicipalUserAuthority(Guid AUTH_Municipal_Users_ID, Guid AUTH_Authority_ID)
        {
            var itemexists = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().FirstOrDefaultAsync(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID && p.AUTH_Authority_ID == AUTH_Authority_ID) != null;

            if (!itemexists)
            {
                var newAuthority = new AUTH_Municipal_Users_Authority()
                {
                    ID = Guid.NewGuid(),
                    AUTH_Municipal_Users_ID = AUTH_Municipal_Users_ID,
                    AUTH_Authority_ID = AUTH_Authority_ID
                };

                await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().InsertOrUpdateAsync(newAuthority);

                return true;
            }

            return false;
        }
        public async Task<AUTH_Municipal_Users_Authority> SetMunicipalUserAuthority(AUTH_Municipal_Users_Authority Data)
        {
            return await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveMunicipalUserAuthority(Guid AUTH_Municipal_Users_ID, Guid AUTH_Authority_ID)
        {
            var itemexists = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().FirstOrDefaultAsync(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID && p.AUTH_Authority_ID == AUTH_Authority_ID);

            if (itemexists != null)
            {
                return await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().DeleteAsync(itemexists);
            }

            return false;
        }
        public async Task<AUTH_Municipal_Users_Settings?> GetMunicipalSettings(Guid AUTH_Municipal_Users_ID)
        {
            var sett = await _unitOfWork.Repository<AUTH_Municipal_Users_Settings>().FirstOrDefaultAsync(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID);

            if (sett == null)
            {
                return new AUTH_Municipal_Users_Settings()
                {
                    AUTH_Municipal_Users_ID = AUTH_Municipal_Users_ID,
                    LANG_Languages_ID = LanguageSettings.German
                };
            }

            return sett;
        }
        public bool HasMunicipalUserRole(Guid RoleID)
        {
            if (_sessionWrapper != null && _sessionWrapper.CurrentMunicipalUser != null)
            {
                var roles = _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToList(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                if (roles != null && roles.Select(p => p.AUTH_Roles_ID).Contains(RoleID))
                {
                    return true;
                }
            }

            return false;
        }
        public bool HasMunicipalUserRole(Guid AUTH_Municipal_Users_ID, Guid RoleID)
        {
            var roles = _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToList(p => p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID);

            if (roles != null && roles.Select(p => p.AUTH_Roles_ID).Contains(RoleID))
            {
                return true;
            }

            return false;
        }
        public async Task<List<AUTH_Municipal_Users>> GetResponsibleChatMunicipalUser(V_CHAT_Message _chatMessage, Guid AUTH_Municipality_ID)
        {
            LANG_Languages Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            List<AUTH_Municipal_Users> _matchingUser = new List<AUTH_Municipal_Users>();
            if (_chatMessage.ContextType == "FormApplication")
            {
                List<V_FORM_Application> _application = await _unitOfWork.Repository<V_FORM_Application>().Where(p => p.LANG_Language_ID == Language.ID && p.ID.ToString().ToLower().Trim() == _chatMessage.ContextElementId.ToLower().Trim() && p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
                List<string> _applicationID = _application.Select(p => p.ID.ToString().ToLower().Trim()).Distinct().ToList();
                List<Guid> _authorityID = _application.Where(p => p.AUTH_Authority_ID != null).Select(p => p.AUTH_Authority_ID.Value).Distinct().ToList();
                List<AUTH_Municipal_Users_Authority> _authorities = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().Where(p => p.AUTH_Authority_ID != null && _authorityID.Contains(p.AUTH_Authority_ID.Value)).Where(p => p.AUTH_Municipal_Users_ID != null).ToListAsync();
                List<Guid?> _authorizedMunicipalUser = _authorities.Select(p => p.AUTH_Municipal_Users_ID).Distinct().ToList();

                List<Guid> _taskIDs = await _unitOfWork.Repository<TASK_Task>().Where(p => p.ContextElementID != null && _applicationID.Contains(p.ContextElementID.ToLower().Trim())).Select(p => p.ID).ToListAsync();
                if (_taskIDs != null && _taskIDs.Any())
                {
                    List<TASK_Task_Responsible> _responsible = await _unitOfWork.Repository<TASK_Task_Responsible>().Where(p => p.TASK_Task_ID != null && _taskIDs.Contains(p.TASK_Task_ID.Value)).ToListAsync();
                    List<Guid?> _responsibleIDs = _responsible.Select(r => r.AUTH_Municipal_Users_ID).Distinct().Where(_authorizedMunicipalUser.Contains).ToList();
                    if (_responsibleIDs != null && _responsibleIDs.Any())
                    {
                        _matchingUser.AddRange(await _unitOfWork.Repository<AUTH_Municipal_Users>().Where(p => _responsibleIDs.Contains(p.ID)).ToListAsync());
                    }
                }
                if (_matchingUser == null || !_matchingUser.Any())
                {
                    _matchingUser = new List<AUTH_Municipal_Users>();
                    _matchingUser.AddRange(await _unitOfWork.Repository<AUTH_Municipal_Users>().Where(p => _authorizedMunicipalUser.Contains(p.ID) && p.AUTH_Municipality_ID == AUTH_Municipality_ID && !p.LockoutEnabled).ToListAsync());
                }
                return _matchingUser;
            }
            if (_chatMessage.ContextType == "OrgRequests")
            {
                _matchingUser.AddRange(await _unitOfWork.Repository<AUTH_Municipal_Users>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && !p.LockoutEnabled).ToListAsync());

            }
            return _matchingUser;
        }
        public async Task<AUTH_Authority> SetAuthority(AUTH_Authority Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority>().InsertOrUpdateAsync(Data);
        }
        public async Task<AUTH_Authority_Extended> SetAuthorityExtended(AUTH_Authority_Extended Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_AUTH_Authority_Reason>> GetAuthorityReasons(Guid AUTH_Authority_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_AUTH_Authority_Reason>().ToListAsync(p => p.AUTH_Authority_ID == AUTH_Authority_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<AUTH_Authority_Dates_Timeslot>> GetAuthorityTimes(Guid AUTH_Authority_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Dates_Timeslot>().ToListAsync(p => p.AUTH_Authority_ID == AUTH_Authority_ID);
        }
        public async Task<List<AUTH_Authority_Dates_Closed>> GetAuthorityClosedDates(Guid AUTH_Authority_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Dates_Closed>().ToListAsync(p => p.AUTH_Authority_ID == AUTH_Authority_ID);
        }
        public async Task<AUTH_Authority_Reason?> GetReason(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Reason>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<bool> RemoveReason(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Reason>().DeleteAsync(ID);
        }
        public async Task<AUTH_Authority_Reason?> SetReason(AUTH_Authority_Reason Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Reason>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveAuthorityTimes(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Dates_Timeslot>().DeleteAsync(ID);
        }
        public async Task<AUTH_Authority_Dates_Timeslot?> SetAuthorityTimes(AUTH_Authority_Dates_Timeslot Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Dates_Timeslot>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<AUTH_Authority_Reason_Extended>> GetReasonExtended(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Reason_Extended>().ToListAsync(p => p.AUTH_Authority_Reason_ID == ID);
        }
        public async Task<AUTH_Authority_Reason_Extended?> SetReasonExtended(AUTH_Authority_Reason_Extended Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Reason_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<AUTH_Authority_Dates_Closed?> SetAuthorityClosedDates(AUTH_Authority_Dates_Closed Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Dates_Closed>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveAuthorityClosedDates(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Dates_Closed>().DeleteAsync(ID);
        }
        public async Task<List<AUTH_Authority_Office_Hours>> GetOfficeHours(Guid AUTH_Authority_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Office_Hours>().ToListAsync(p => p.AUTH_Authority_ID == AUTH_Authority_ID);
        }
        public async Task<AUTH_Authority_Office_Hours?> SetOfficeHours(AUTH_Authority_Office_Hours Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Office_Hours>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveOfficeHours(Guid ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Office_Hours>().DeleteAsync(ID);
        }
    }
}
