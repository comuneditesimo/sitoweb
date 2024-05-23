using Blazored.LocalStorage;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore;
using ICWebApp.Application.Helper;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Blazored.SessionStorage;
using ICWebApp.DataStore.MSSQL.Interfaces;
using Microsoft.EntityFrameworkCore;
using ICWebApp.Application.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using ICWebApp.Application.Interface.Helper;

namespace ICWebApp.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly ISessionWrapper _sessionWrapper;
        private readonly ILocalStorageService _localStorageService;
        private readonly IAUTHProvider _AuthProvider;
        private readonly IMailerService _mailerService;
        private readonly ITEXTProvider _textProvider;
        private readonly ICONFProvider _CONFProvider;
        private readonly IMSGProvider _msgProvider;
        private readonly ISMSService _smsService;
        private readonly IPushService _pushService;
        private readonly ISessionStorageService SessionStorage;
        private readonly IMessageService _messageService;
        private readonly AuthenticationStateProvider _authenticationHelper;
        private readonly NavigationManager _navManager;
        private IGenericRepository<V_AUTH_Citizens> _repositoryVCitizens;
        private readonly ICookieHelper _cookieHelper;


        public AccountService(ISessionWrapper sessionWrapper, IAUTHProvider AuthProvider, ILocalStorageService localStorageService, AuthenticationStateProvider authenticationHelper,
                              IMailerService mailerService, ITEXTProvider textProvider, ICONFProvider _CONFProvider, NavigationManager _navManager, IMSGProvider msgProvider, ISMSService _smsService, IPushService pushService,
                              ISessionStorageService SessionStorage, IMessageService _messageService,
                              IGenericRepository<V_AUTH_Citizens> repositoryVCitizens, ICookieHelper _cookieHelper)
        {
            this._sessionWrapper = sessionWrapper;
            this._AuthProvider = AuthProvider;
            this._localStorageService = localStorageService;
            this._authenticationHelper = authenticationHelper;
            this._mailerService = mailerService;
            this._textProvider = textProvider;
            this._CONFProvider = _CONFProvider;
            this._navManager = _navManager;
            this._msgProvider = msgProvider;
            this._smsService = _smsService;
            this._pushService = pushService;
            this.SessionStorage = SessionStorage;
            this._messageService = _messageService;
            this._repositoryVCitizens = repositoryVCitizens;
            this._cookieHelper = _cookieHelper;
        }

        public async Task<MSG_SystemMessages?> Login(Login UserLogin)
        {
            MSG_SystemMessages? result = await _AuthProvider.Verify(UserLogin.UserName, UserLogin.Password);

            if (result == null || result.Code != "LOGIN_SUCCESS")
            {
                return result;
            }

            var dbUser = await _AuthProvider.GetUser(UserLogin.UserName);
            var token = Guid.NewGuid();
            var timeStamp = DateTime.Now;

            await _cookieHelper.Write("Comunix.Login.AuthToken", token.ToString());

            if (UserLogin.RememberMe)
            {
                await _cookieHelper.Write("Comunix.Login.UserID", dbUser.ID.ToString());
            }

            _sessionWrapper.Login_AuthToken = token.ToString();
            _sessionWrapper.Login_AuthTimestamp = timeStamp.ToString();
            _sessionWrapper.Login_UserID = dbUser.ID.ToString(); 

            dbUser.LastLoginToken = token;
            dbUser.LastLoginTimeStamp = timeStamp;

            _sessionWrapper.CurrentUser = dbUser;
            _sessionWrapper.CurrentMunicipalUser = null;

            await _AuthProvider.UpdateUser(dbUser);

            (_authenticationHelper as AuthenticationHelper).Notify();

            return result;
        }
        public async Task<bool> Logout()
        {            
            await _cookieHelper.Remove("Comunix.Login.AuthToken");
            await _cookieHelper.Remove("Comunix.Login.UserID");
            await _cookieHelper.Remove("Comunix.Login.AuthTimeStamp");
            await _cookieHelper.Remove("Comunix.Login.SubstituteUserID");

            _sessionWrapper.Login_AuthToken = null;
            _sessionWrapper.Login_UserID = null;
            _sessionWrapper.CurrentUser = null;
            _sessionWrapper.CurrentSubstituteUser = null;
            _sessionWrapper.CurrentMunicipalUser = null;

            (_authenticationHelper as AuthenticationHelper).Notify();

            return true;
        }
        public async Task<MSG_SystemMessages?> PasswordForgotten(Login UserLogin, bool InTesting = false)
        {
            var enviromentConfig = await _CONFProvider.GetEnviromentConfiguration(null);

            if(enviromentConfig == null)
                return await _msgProvider.GetSystemMessage("INTERNAL_ERROR");

            AUTH_Users? User = await _AuthProvider.GetUser(UserLogin.UserName);

            if (User == null)
            {
                return await MunicipalPasswordForgotten(UserLogin, InTesting);
            }

            string MailText = _textProvider.Get("PASSWORDFORGOTTEN_EMAIL_TEXT");

            Guid PasswordToken = Guid.NewGuid();

            User.PasswordResetToken = PasswordToken;

            await _AuthProvider.UpdateUser(User);

            if (!InTesting)
            {
                string link = _navManager.BaseUri + "ResetPassword/" + PasswordToken.ToString();

                MailText = MailText.Replace("{Link}", link);
            }

            MailText = MailText.Replace("{FirstName}", User.Firstname);
            MailText = MailText.Replace("{ProgrammName}", enviromentConfig.ProgrammName);

            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = User.Email;
            mail.Subject = _textProvider.Get("PASSWORDFORGOTTEN_EMAIL_TITLE").Replace("{ProgrammName}", enviromentConfig.ProgrammName);
            mail.Body = MailText;
            mail.PlannedSendDate = DateTime.Now;

            if (!InTesting)
            {
                if (User.AUTH_Municipality_ID != null)
                {
                    bool result = await _mailerService.SendMail(mail, null, User.AUTH_Municipality_ID.Value, null);
                }
            }

            return await _msgProvider.GetSystemMessage("PASSWORDFORGOTTEN_SUCCESS");
        }
        private async Task<MSG_SystemMessages?> MunicipalPasswordForgotten(Login UserLogin, bool InTesting = false)
        {
            var enviromentConfig = await _CONFProvider.GetEnviromentConfiguration(null);

            if (enviromentConfig == null)
                return await _msgProvider.GetSystemMessage("INTERNAL_ERROR");

            AUTH_Municipal_Users? User = await _AuthProvider.GetMunicipalUser(UserLogin.UserName);

            if (User == null)
                return await _msgProvider.GetSystemMessage("PASSWORDFORGOTTEN_WRONG_USERNAME");

            string MailText = _textProvider.Get("PASSWORDFORGOTTEN_EMAIL_TEXT");

            Guid PasswordToken = Guid.NewGuid();

            User.PasswordResetToken = PasswordToken;

            await _AuthProvider.UpdateMunicipalUser(User);

            if (!InTesting)
            {
                string link = _navManager.BaseUri + "ResetPassword/" + PasswordToken.ToString();

                MailText = MailText.Replace("{Link}", link);
            }

            MailText = MailText.Replace("{FirstName}", User.Firstname);
            MailText = MailText.Replace("{ProgrammName}", enviromentConfig.ProgrammName);

            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = User.Email;
            mail.Subject = _textProvider.Get("PASSWORDFORGOTTEN_EMAIL_TITLE").Replace("{ProgrammName}", enviromentConfig.ProgrammName);
            mail.Body = MailText;
            mail.PlannedSendDate = DateTime.Now;

            if (!InTesting)
            {
                if (User.AUTH_Municipality_ID != null)
                {
                    bool result = await _mailerService.SendMail(mail, null, User.AUTH_Municipality_ID.Value, null);
                }
            }

            return await _msgProvider.GetSystemMessage("PASSWORDFORGOTTEN_SUCCESS");
        }
        public async Task<AUTH_Users?> Register(AUTH_Users NewUser, AUTH_Users_Anagrafic? anagrafic = null)
        {
            PasswordHelper pwHelper = new PasswordHelper();

            NewUser.PasswordHash = pwHelper.CreateMD5Hash(NewUser.Password);
            NewUser.Email = NewUser.DA_Email;

            if (anagrafic != null && anagrafic.FiscalNumber != null)
            {
                NewUser.Username = anagrafic.FiscalNumber;
            }
            else
            {
                NewUser.Email = NewUser.DA_Email;
            }

            var user = await _AuthProvider.RegisterUser(NewUser, anagrafic);

            return user;
        }
        public async Task<AUTH_Users?> RegistrationUpdate(AUTH_Users NewUser, AUTH_Users_Anagrafic? anagrafic = null)
        {
            PasswordHelper pwHelper = new PasswordHelper();

            if (NewUser.Password != null)
            {
                NewUser.PasswordHash = pwHelper.CreateMD5Hash(NewUser.Password);
            }
   
            NewUser.Email = NewUser.DA_Email;

            if (anagrafic != null && anagrafic.FiscalNumber != null && (NewUser.Username == null || NewUser.Username ==""))
            {
                NewUser.Username = anagrafic.FiscalNumber;
            }
            else
            {
                NewUser.Email = NewUser.DA_Email;
            }

            var user = await _AuthProvider.RegisterUser(NewUser, anagrafic);

            return user;
        }
        public bool IsEmailUnique(string Email, Guid AUTH_Users_ID)
        {
            return !_AuthProvider.EmailExists(Email, AUTH_Users_ID);
        }
        public bool IsFiscalNumberUnique(string FiscalNumber, bool WithManualInput = false)
        {
            return !_AuthProvider.FiscalNumberExists(FiscalNumber, WithManualInput);
        }
        public async Task<bool> SendVerificationEmail(AUTH_Users User, bool SelfClosing = false, bool InTesting = false)
        {
            if (User == null)
                return false;

            var enviromentConfig = await _CONFProvider.GetEnviromentConfiguration(null);

            if(enviromentConfig == null)
                return false;

            if (enviromentConfig.EnableMailConfirming == false)
                return true;

            var MailText = _textProvider.Get("VERIFYEMAIL_EMAIL_TEXT");

            var EmailToken = Guid.NewGuid();

            Random r = new Random();
            var x = r.Next(0, 999999);

            User.EmailConfirmToken = x.ToString("000000"); ;

            await _AuthProvider.UpdateUser(User);

            MailText = MailText.Replace("{Token}", User.EmailConfirmToken);

            MailText = MailText.Replace("{FirstName}", User.Firstname);
            MailText = MailText.Replace("{ProgrammName}", enviromentConfig.ProgrammName);

            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = User.Email;
            mail.Subject = _textProvider.Get("VERIFYEMAIL_EMAIL_TITLE").Replace("{ProgrammName}", enviromentConfig.ProgrammName);
            mail.Body = MailText;
            mail.PlannedSendDate = DateTime.Now;

            if (!InTesting)
            {
                if (User.AUTH_Municipality_ID != null)
                {
                    return await _mailerService.SendMail(mail, null, User.AUTH_Municipality_ID.Value, null);
                }
            }

            return true;
        }
        public async Task<MSG_SystemMessages?> VerifyEmail(Guid AUTH_Users_ID, string EmailCode)
        {
            return await _AuthProvider.VerifyEmail(AUTH_Users_ID, EmailCode);
        }
        public async Task<MSG_SystemMessages?> VerifyPasswordResetToken(Guid PasswordResetToken)
        {
            return await _AuthProvider.VerifyPasswordResetToken(PasswordResetToken);
        }
        public async Task<MSG_SystemMessages?> VerifyPasswordResetTokenMunicipalUser(Guid PasswordResetToken)
        {
            return await _AuthProvider.VerifyPasswordResetTokenMunicipalUser(PasswordResetToken);
        }
        public async Task<MSG_SystemMessages?> ChangePassword(AUTH_Users User)
        {
            try
            {
                PasswordHelper pwHelper = new PasswordHelper();

                User.PasswordHash = pwHelper.CreateMD5Hash(User.Password);
                User.PasswordResetToken = null;

                await _AuthProvider.UpdateUser(User);

                return await _msgProvider.GetSystemMessage("PASSWORDCHANGE_SUCCESSFULL");
            }
            catch
            {
                return await _msgProvider.GetSystemMessage("PASSWORDCHANGE_ERROR");
            }
        }
        public async Task<MSG_SystemMessages?> MunicipalChangePassword(AUTH_Municipal_Users User)
        {
            try
            {
                PasswordHelper pwHelper = new PasswordHelper();

                User.PasswordHash = pwHelper.CreateMD5Hash(User.Password);
                User.PasswordResetToken = null;

                await _AuthProvider.UpdateMunicipalUser(User);

                return await _msgProvider.GetSystemMessage("PASSWORDCHANGE_SUCCESSFULL");
            }
            catch
            {
                return await _msgProvider.GetSystemMessage("PASSWORDCHANGE_ERROR");
            }
        }
        public async Task<bool> HealthCheck()
        {
            if (_sessionWrapper.Login_AuthToken != null &&
                  _sessionWrapper.Login_AuthTimestamp != null &&
                  _sessionWrapper.Login_UserID != null)
            {
                var dbUser = await _AuthProvider.GetUser(Guid.Parse(_sessionWrapper.Login_UserID));

                if (dbUser != null)
                {
                    var token = Guid.NewGuid();
                    var timeStamp = DateTime.Now;

                    CookieOptions options = new CookieOptions();
                    options.Expires = DateTime.Now.AddDays(1);
                    await _cookieHelper.Write("Comunix.Login.AuthToken", token.ToString());
                    await SessionStorage.SetItemAsync("AuthToken", dbUser.ID);

                    _sessionWrapper.Login_AuthToken = token.ToString();
                    _sessionWrapper.Login_AuthTimestamp = timeStamp.ToString();
                    _sessionWrapper.CurrentUser = dbUser;

                    dbUser.LastLoginToken = token;
                    dbUser.LastLoginTimeStamp = timeStamp;

                    await _AuthProvider.UpdateUser(dbUser);
                }
                else
                {
                    var dbMunicipalUser = await _AuthProvider.GetMunicipalUser(Guid.Parse(_sessionWrapper.Login_UserID));

                    if (dbMunicipalUser != null)
                    {
                        var token = Guid.NewGuid();
                        var timeStamp = DateTime.Now;

                        CookieOptions options = new CookieOptions();
                        options.Expires = DateTime.Now.AddDays(1);
                        await _cookieHelper.Write("Comunix.Login.AuthToken", token.ToString());
                        await SessionStorage.SetItemAsync("AuthToken", dbMunicipalUser.ID);

                        _sessionWrapper.Login_AuthToken = token.ToString();
                        _sessionWrapper.Login_AuthTimestamp = timeStamp.ToString();
                        _sessionWrapper.CurrentMunicipalUser = dbMunicipalUser;

                        dbMunicipalUser.LastLoginToken = token;
                        dbMunicipalUser.LastLoginTimeStamp = timeStamp;

                        await _AuthProvider.UpdateMunicipalUser(dbMunicipalUser);
                    }
                }
            }

            return true;
        }
        public async Task<AUTH_Users?> RegisterExternal(AUTH_Users_Anagrafic Anagrafic, string RegistrationMode, Guid? AUTH_Municipality_ID = null)
        {
            if (Anagrafic != null && Anagrafic.FiscalNumber != null && RegistrationMode != null)
            {
                if(AUTH_Municipality_ID == null)
                {
                    AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }

                if (AUTH_Municipality_ID != null)
                {
                    var dbUser = await _AuthProvider.GetUser(Anagrafic.FiscalNumber, RegistrationMode, AUTH_Municipality_ID);
                    bool IsNew = false;

                    if(dbUser == null)
                    {
                        dbUser = await _AuthProvider.GetUser(Anagrafic.FiscalNumber, "custom", AUTH_Municipality_ID);
                    }

                    if (dbUser == null)
                    {
                        dbUser = await _AuthProvider.GetUser(Anagrafic.FiscalNumber, "Citizen Backend", AUTH_Municipality_ID);

                        if (dbUser != null)
                        {
                            dbUser.RegistrationMode = RegistrationMode;
                        }
                    }

                    if (dbUser == null)
                    {
                        var User = new AUTH_Users();

                        User.ID = Guid.NewGuid();
                        User.Firstname = Anagrafic.FirstName;
                        User.Lastname = Anagrafic.LastName;

                        User.RegistrationMode = RegistrationMode;

                        User.Username = Anagrafic.Email;
                        User.Email = Anagrafic.Email;

                        User.EmailConfirmed = true;
                        User.PhoneNumberConfirmed = true;
                        User.VeriffConfirmed = true;

                        User.PasswordHash = "";
                        User.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();

                        Anagrafic.AUTH_Users_ID = User.ID;

                        var ResultUser = await _AuthProvider.RegisterUser(User, Anagrafic);

                        if (ResultUser != null)
                        {
                            Anagrafic.AUTH_Users_ID = ResultUser.ID;

                            await _AuthProvider.SetAnagrafic(Anagrafic);
                        }

                        dbUser = await _AuthProvider.GetUser(Anagrafic.FiscalNumber, User.RegistrationMode);

                        IsNew = true;
                    }

                    if (dbUser != null && !IsNew)
                    {
                        var existingAnagrafic = await _AuthProvider.GetAnagraficByUserID(dbUser.ID);

                        if (existingAnagrafic != null)
                        {
                            if (Anagrafic.IgnoreDataVerification == true)
                                existingAnagrafic.IgnoreDataVerification = true;

                            await _AuthProvider.SetAnagrafic(existingAnagrafic);
                        }
                    }

                    if (!_AuthProvider.HasUserRole(AuthRoles.Citizen) && dbUser != null)
                    {
                        await _AuthProvider.SetRole(dbUser.ID, AuthRoles.Citizen);
                    }

                    if (dbUser != null)
                    {
                        dbUser.LastLoginToken = Guid.NewGuid();
                        dbUser.LastLoginTimeStamp = DateTime.Now;

                        await _AuthProvider.UpdateUser(dbUser);
                    }

                    return dbUser;
                }
            }

            return null;
        }
        public async Task<bool> LoginExternal(string UserID, string LoginToken)
        {
            var token = Guid.NewGuid();
            var timeStamp = DateTime.Now;

            var dbUser = await _AuthProvider.GetUserByLoginToken(Guid.Parse(LoginToken));

            if (dbUser != null && dbUser.ID == Guid.Parse(UserID))
            {
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(1);
                await _cookieHelper.Write("Comunix.Login.AuthToken", token.ToString());

                _sessionWrapper.Login_AuthToken = token.ToString();
                _sessionWrapper.Login_AuthTimestamp = timeStamp.ToString();
                _sessionWrapper.Login_UserID = dbUser.ID.ToString();

                dbUser.LastLoginToken = token;
                dbUser.LastLoginTimeStamp = timeStamp;

                if(dbUser.AUTH_Municipality_ID == null)
                {
                    dbUser.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }

                _sessionWrapper.CurrentUser = dbUser;

                await _AuthProvider.UpdateUser(dbUser);

                (_authenticationHelper as AuthenticationHelper).Notify();

                return true;
            }

            return false;
        }
        public async Task<bool> SendVerificationSMS(AUTH_Users User)
        {
            if (User == null)
                return false;

            var enviromentConfig = await _CONFProvider.GetEnviromentConfiguration(null);

            if (enviromentConfig == null)
                return false;

            if (User.AUTH_Municipality_ID == null)
                return false;

            if (enviromentConfig.EnablePhoneConfirming == false)
                return true;

            Random r = new Random();
            var x = r.Next(0, 999999);

            User.PhoneNumberConfirmToken = x.ToString("000000");

            await _AuthProvider.UpdateUser(User);

            var sms = new MSG_SMS();
            sms.ID = Guid.NewGuid();
            sms.PhoneNumber = User.PhoneNumber;
            sms.Message = _textProvider.Get("VERIFCIATION_CODE_TEXT") + User.PhoneNumberConfirmToken;
            sms.AUTH_Municipality_ID = User.AUTH_Municipality_ID;
            return await _smsService.SendSMS(sms, User.AUTH_Municipality_ID.Value);
        }
        public async Task<MSG_SystemMessages?> VerifyPhone(Guid AUTH_Users_ID, string PhoneCode)
        {
            return await _AuthProvider.VerifyPhone(AUTH_Users_ID, PhoneCode);
        }
        public async Task<bool> SendWelcomeMail(AUTH_Users User)
        {
            var enviromentConfig = await _CONFProvider.GetEnviromentConfiguration(null);

            if (enviromentConfig == null)
                return false;
                
            var msg = await _messageService.GetMessage(User.ID, User.AUTH_Municipality_ID.Value, "WELCOME_NOTIFICATION_TEXT", "WELCOME_NOTIFICATION_SHORT_TEXT", "WELCOME_NOTIFICATION_SUBJECT_TEXT", Guid.Parse("dcd04015-c1bd-4ad5-99e6-aeef7f35bfa4"), true);

            if (msg != null)
            {
                await _messageService.SendMessage(msg, _navManager.BaseUri + "/");

                return true;
            }

            return false;
        }
        public async Task<List<V_AUTH_Citizens>> GetRegistrationAdminList(Guid? municipalityID)
        {
            var result = await _repositoryVCitizens.Where(a => a.AUTH_Municipality_ID == municipalityID || municipalityID == null).ToListAsync();
            return result.ToList();
        }
        public async Task<MSG_SystemMessages?> LoginMunicipal(Login UserLogin)
        {
            MSG_SystemMessages? result = await _AuthProvider.VerifyMunicipal(UserLogin.UserName, UserLogin.Password);

            if (result == null || result.Code != "LOGIN_SUCCESS")
            {
                return result;
            }

            var dbUser = await _AuthProvider.GetMunicipalUser(UserLogin.UserName);
            var token = Guid.NewGuid();
            var timeStamp = DateTime.Now;

            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(1);
            await _cookieHelper.Write("Comunix.Login.AuthToken", token.ToString());

            if (UserLogin.RememberMe)
            {
                await _cookieHelper.Write("Comunix.Login.UserID", dbUser.ID.ToString());
            }

            _sessionWrapper.Login_AuthToken = token.ToString();
            _sessionWrapper.Login_AuthTimestamp = timeStamp.ToString();
            _sessionWrapper.Login_UserID = dbUser.ID.ToString();

            dbUser.LastLoginToken = token;
            dbUser.LastLoginTimeStamp = timeStamp;

            _sessionWrapper.CurrentUser = null;
            _sessionWrapper.CurrentSubstituteUser = null;
            _sessionWrapper.CurrentMunicipalUser = dbUser;

            await _AuthProvider.UpdateMunicipalUser(dbUser);

            (_authenticationHelper as AuthenticationHelper).Notify();

            return result;
        }
    }
}
