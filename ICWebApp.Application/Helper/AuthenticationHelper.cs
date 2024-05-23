using Blazored.LocalStorage;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Http;
using ICWebApp.Application.Interface.Helper;

namespace ICWebApp.Application.Helper
{
    public class AuthenticationHelper : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly ISessionWrapper _sessionWrapper;
        private readonly IAUTHProvider _authProvider;
        private readonly IAUTHSettingsProvider _authSettingsProvider;
        private readonly ILANGProvider _langProvider;
        private readonly ISessionStorageService SessionStorage;
        private readonly ICookieHelper _cookieHelper;

        public AuthenticationHelper(ILocalStorageService localStorageService, ISessionWrapper sessionWrapper, IAUTHProvider authProvider,
                                    IAUTHSettingsProvider _authSettingsProvider, ILANGProvider langProvider, ISessionStorageService sessionStorage,
                                    ICookieHelper _cookieHelper)
        {
            this._localStorageService = localStorageService;
            this._sessionWrapper = sessionWrapper;
            this._authProvider = authProvider;
            this._authSettingsProvider = _authSettingsProvider;
            this._langProvider = langProvider;
            this.SessionStorage = sessionStorage;
            this._cookieHelper = _cookieHelper;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_sessionWrapper.PageIsRendered)
            {
                var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity() { }));
                return anonymous;
            }

            bool tokenVerification = false;
            bool userVerification = false;
            bool validLogin = false;

            string token = _sessionWrapper.Login_AuthToken;
            string userID = _sessionWrapper.Login_UserID;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userID))
            {
                token = await _cookieHelper.ReadCookies("Comunix.Login.AuthToken");
                userID = await _cookieHelper.ReadCookies("Comunix.Login.UserID");
            }

            if(string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(token))
            {
                tokenVerification = true;
            }

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(token))
            {
                userVerification = true;
            }

            AUTH_Users? dbUser = null;
            AUTH_Municipal_Users? dbMunicipalUser = null;
            
            if (userVerification)
            {
                dbUser = await _authProvider.GetUser(Guid.Parse(userID));

                if (dbUser != null && dbUser.LastLoginToken == Guid.Parse(token))
                {
                    validLogin = true;
                }
            }

            if(userVerification && dbUser == null)
            {
                dbMunicipalUser = await _authProvider.GetMunicipalUser(Guid.Parse(userID));

                if (dbMunicipalUser != null && dbMunicipalUser.LastLoginToken == Guid.Parse(token))
                {
                    validLogin = true;
                }
            }

            if (tokenVerification)
            {
                dbUser = await _authProvider.GetUserByLoginToken(Guid.Parse(token));

                if (dbUser != null && dbUser.LastLoginToken == Guid.Parse(token))
                {
                    validLogin = true;
                }
            }

            if (tokenVerification && dbUser == null)
            {
                dbMunicipalUser = await _authProvider.GetMunicipalUserByLoginToken(Guid.Parse(token));

                if (dbMunicipalUser != null && dbMunicipalUser.LastLoginToken == Guid.Parse(token))
                {
                    validLogin = true;
                }
            }

            if (!validLogin || (dbUser == null && dbMunicipalUser == null)) //IF FALSE NO LOGIN
            {
                _sessionWrapper.Initializing = false;
                var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity() { }));
                return anonymous;
            }

            if (dbUser != null)
            {
                _sessionWrapper.CurrentUser = dbUser;

                var SubuserID = await _cookieHelper.ReadCookies("Comunix.Login.SubstituteUserID");

                if (_sessionWrapper.CurrentUser != null && !string.IsNullOrEmpty(SubuserID))
                {
                    var subUser = await _authProvider.GetUser(Guid.Parse(SubuserID));

                    var orgs = await _authProvider.GetUsersOrganizations(_sessionWrapper.CurrentUser.ID);

                    if (subUser != null && orgs.Select(p => p.ORG_AUTH_Users_ID).Contains(subUser.ID))
                    {
                        _sessionWrapper.CurrentSubstituteUser = subUser;
                    }
                }

                _sessionWrapper.Login_UserID = dbUser.ID.ToString();
                _sessionWrapper.Login_AuthToken = token;
                if (dbUser.LastLoginTimeStamp != null)
                {
                    _sessionWrapper.Login_AuthTimestamp = dbUser.LastLoginTimeStamp.Value.ToString();
                }
                else
                {
                    _sessionWrapper.Login_AuthTimestamp = DateTime.Now.ToString();
                }

                var settings = await _authSettingsProvider.GetSettings(_sessionWrapper.CurrentUser.ID);

                if (settings != null)
                {
                    var languageList = await _langProvider.GetAll();

                    if (languageList != null)
                    {
                        var languageCode = languageList.FirstOrDefault(p => p.ID == settings.LANG_Languages_ID);

                        if (languageCode != null)
                        {
                            await _langProvider.SetLanguage(languageCode.Code);
                        }
                    }
                }
            }
            if (dbMunicipalUser != null)
            {
                _sessionWrapper.CurrentMunicipalUser = dbMunicipalUser;

                _sessionWrapper.Login_UserID = dbMunicipalUser.ID.ToString();
                _sessionWrapper.Login_AuthToken = token;

                if (dbMunicipalUser.LastLoginTimeStamp != null)
                {
                    _sessionWrapper.Login_AuthTimestamp = dbMunicipalUser.LastLoginTimeStamp.Value.ToString();
                }
                else
                {
                    _sessionWrapper.Login_AuthTimestamp = DateTime.Now.ToString();
                }

                var settings = await _authSettingsProvider.GetMunicipalSettings(_sessionWrapper.CurrentMunicipalUser.ID);

                if (settings != null)
                {
                    var languageList = await _langProvider.GetAll();

                    if (languageList != null)
                    {
                        var languageCode = languageList.FirstOrDefault(p => p.ID == settings.LANG_Languages_ID);

                        if (languageCode != null)
                        {
                            await _langProvider.SetLanguage(languageCode.Code);
                        }
                    }
                }
            }

            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, token)
            };

            var userIdentity = new ClaimsIdentity(userClaims, "comunix.CookieAuth");
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            var loginUser = new AuthenticationState(userPrincipal);

            if (dbUser != null)
            {
                await SessionStorage.SetItemAsync("userID", dbUser.ID);
            }
            if(dbMunicipalUser != null)
            {
                await SessionStorage.SetItemAsync("userID", dbMunicipalUser.ID);
            }

            if (dbUser != null)
            {
                _authProvider.CheckUserVerifications(dbUser.ID);
            }
            if (dbMunicipalUser != null)
            {
                _authProvider.CheckMunicipalUserVerifications(dbMunicipalUser.ID);
            }

            _sessionWrapper.Initializing = false;

            return loginUser;
        }
        public void Notify()
        {
            try
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }catch { }
        }
    }
}
