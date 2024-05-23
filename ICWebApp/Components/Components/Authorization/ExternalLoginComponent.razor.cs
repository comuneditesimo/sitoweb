using Microsoft.AspNetCore.Components.Authorization;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Helper;
using ICWebApp.Domain.DBModels;
using Blazored.LocalStorage;

namespace ICWebApp.Components.Components.Authorization
{
    public partial class ExternalLoginComponent
    {
        [Inject] AuthenticationStateProvider AuthenticationHelper { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IAUTHProvider AUTHProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ICookieHelper CookieHelper { get; set; }

        [Parameter] public string? ExternalLoginToken { get; set; }

        string _lastExternalLoginToken = string.Empty;
        bool _firstRenderComplete = false;

        protected override async Task OnParametersSetAsync()
        {
            await CheckExternalUserLogin();
            await base.OnParametersSetAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _firstRenderComplete = true;
                await CheckExternalUserLogin();
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        private async Task CheckExternalUserLogin()
        {
            if (_firstRenderComplete)
            {
                if (ExternalLoginToken != null &&
                    ExternalLoginToken != string.Empty &&
                    ExternalLoginToken.Trim().ToLower() != _lastExternalLoginToken.Trim().ToLower())
                {
                    _lastExternalLoginToken = ExternalLoginToken;
                    try
                    {
                        Guid? _externalLoginToken = Guid.Parse(ExternalLoginToken);
                        if (_externalLoginToken != null && _externalLoginToken != Guid.Empty)
                        {
                            AUTH_Users? _citizenUser = await AUTHProvider.GetUserByValidExternalLoginToken(_externalLoginToken.Value);
                            AUTH_Municipal_Users? _municipalUser = await AUTHProvider.GetMunicipalUserByValidExternalLoginToken(_externalLoginToken.Value);
                            if (_citizenUser != null &&
                                _citizenUser.ExternalLoginToken != null &&
                                _citizenUser.ExternalLoginToken != Guid.Empty &&
                                _citizenUser.ExternalLoginDate != null &&
                                _citizenUser.ExternalLoginToken == _externalLoginToken &&
                                _citizenUser.ExternalLoginDate.Value.AddMinutes(2) > DateTime.Now)
                            {
                                await LoginAsCitizenUser(_citizenUser);
                                return;
                            }
                            else if (_municipalUser != null &&
                                     _municipalUser.ExternalLoginToken != null &&
                                     _municipalUser.ExternalLoginToken != Guid.Empty &&
                                     _municipalUser.ExternalLoginDate != null &&
                                     _municipalUser.ExternalLoginToken == _externalLoginToken &&
                                     _municipalUser.ExternalLoginDate.Value.AddMinutes(2) > DateTime.Now)
                            {
                                await LoginAsMunicipalUser(_municipalUser);
                                return;
                            }
                            else
                            {
                                NavManager.NavigateTo("/Login");
                                BusyIndicatorService.IsBusy = true;
                                StateHasChanged();
                            }
                        }
                        else
                        {
                            NavManager.NavigateTo("/Login");
                            BusyIndicatorService.IsBusy = true;
                            StateHasChanged();
                        }
                    }
                    catch
                    {
                        NavManager.NavigateTo("/Login");
                        BusyIndicatorService.IsBusy = true;
                        StateHasChanged();
                    }
                }
            }
        }
        private async Task LoginAsCitizenUser(AUTH_Users? _user)
        {
            if (_user != null)
            {
                Guid _logintoken = Guid.NewGuid();
                DateTime _loginTimeStamp = DateTime.Now;

                await CookieHelper.Write("Comunix.Login.AuthToken", _logintoken.ToString());

                SessionWrapper.Login_AuthToken = _logintoken.ToString();
                SessionWrapper.Login_AuthTimestamp = _loginTimeStamp.ToString();
                SessionWrapper.Login_UserID = _user.ID.ToString();

                _user.LastLoginToken = _logintoken;
                _user.LastLoginTimeStamp = _loginTimeStamp;
                _user.ExternalLoginToken = null;
                _user.ExternalLoginDate = null;

                SessionWrapper.CurrentUser = _user;
                SessionWrapper.CurrentMunicipalUser = null;
                await AUTHProvider.UpdateUser(_user);

                AuthenticationHelper? _authenticationHelper = AuthenticationHelper as AuthenticationHelper;
                if (_authenticationHelper != null)
                {
                    _authenticationHelper.Notify();
                }

                NavManager.NavigateTo("/", true);
                StateHasChanged();
            }
        }
        private async Task LoginAsMunicipalUser(AUTH_Municipal_Users? _municipalUser)
        {
            if (_municipalUser != null)
            {
                Guid _logintoken = Guid.NewGuid();
                DateTime _loginTimeStamp = DateTime.Now;

                await CookieHelper.Write("Comunix.Login.AuthToken", _logintoken.ToString());

                SessionWrapper.Login_AuthToken = _logintoken.ToString();
                SessionWrapper.Login_AuthTimestamp = _loginTimeStamp.ToString();
                SessionWrapper.Login_UserID = _municipalUser.ID.ToString();

                _municipalUser.LastLoginToken = _logintoken;
                _municipalUser.LastLoginTimeStamp = _loginTimeStamp;
                _municipalUser.ExternalLoginToken = null;
                _municipalUser.ExternalLoginDate = null;

                SessionWrapper.CurrentUser = null;
                SessionWrapper.CurrentSubstituteUser = null;
                SessionWrapper.CurrentMunicipalUser = _municipalUser;
                await AUTHProvider.UpdateMunicipalUser(_municipalUser);

                AuthenticationHelper? _authenticationHelper = AuthenticationHelper as AuthenticationHelper;
                if (_authenticationHelper != null)
                {
                    _authenticationHelper.Notify();
                }

                NavManager.NavigateTo("/Backend/Landing", true);
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
        }
    }
}
