using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using ICWebApp.DataStore;
using Microsoft.AspNetCore.Components.Authorization;
using ICWebApp.Application.Helper;
using ICWebApp.Domain.DBModels;
using Microsoft.JSInterop;
using ICWebApp.Application.Settings;
using Microsoft.AspNetCore.Components.Forms;

namespace ICWebApp.Components.Components.Authorization
{
    public partial class LoginComponent
    {
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }   
        [Inject] ISessionWrapper SessionWrapper { get; set; }   
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAccountService AccountService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IUrlService UrlService { get; set; }
        [Parameter] public string? RedirectUrl { get; set; }
        public bool IsBusy { get => _isBusy; set => _isBusy = value; }
        private Login login;
        private MSG_SystemMessages? Message;
        private bool _isBusy = true;
        private bool ShowResetPasswordButton = true;
        private EditForm LoginForm;

        protected override void OnInitialized()
        {
            login = new Login();

            EnviromentService.OnIsMobileChanged += EnviromentService_OnIsMobileChanged;

            IsBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnInitialized();
        }
        private void EnviromentService_OnIsMobileChanged()
        {
            StateHasChanged();
        }
        private async void HandleValidSubmit()
        {
            if (LoginForm == null || LoginForm.EditContext == null)
                return;

            if (!LoginForm.EditContext.Validate())
                return;

            IsBusy = true;
            StateHasChanged();

            Message = await AccountService.Login(login);

            if (Message != null && Message.Code == "LOGIN_SUCCESS")
            {
                if (!string.IsNullOrEmpty(RedirectUrl))
                {
                    BusyIndicatorService.IsBusy = true;
                    RedirectUrl = RedirectUrl.Replace("ReturnUrl=", "").Replace("^", "/").Replace("_", "^");
                    NavManager.NavigateTo(Uri.UnescapeDataString(RedirectUrl));
                    return;
                }

                NavManager.NavigateTo("/", true);
                StateHasChanged();
                return;                
            }
            else if(Message != null && Message.Code == "LOGIN_WRONG_USERNAME")
            {
                Message = await AccountService.LoginMunicipal(login);
                StateHasChanged();

                if (Message != null && Message.Code == "LOGIN_SUCCESS")
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Backend/Landing", true);
                    StateHasChanged();
                    return;
                }
            }

            IsBusy = false;
            StateHasChanged();
        }
        private async void HandleResetPassword()
        {
            if (login != null)
            {
                IsBusy = true;
                ShowResetPasswordButton = false;
                StateHasChanged();

                Message = await AccountService.PasswordForgotten(login);

                if(Message == null || Message.MSG_SystemMessageTypesID != Guid.Parse("9574B88A-DAC4-4101-A27A-E5306224A6C0"))
                {
                    ShowResetPasswordButton = true;
                }

                IsBusy = false;
                StateHasChanged();

                await Task.Delay(20000);
                Message = null;
                ShowResetPasswordButton = true;
                StateHasChanged();
            }
        }
        private void HandleRegisterPassword()
        {
            NavManager.NavigateTo("/Registration");
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //URLUPDATE --WORKS
            if (firstRender)
            {
                var baseUri = NavManager.BaseUri; 
                var matchedMun = await UrlService.GetMunicipalityByUrl(baseUri);
        
                if (matchedMun != null)
                {
                    SessionWrapper.AUTH_Municipality_ID = matchedMun.AUTH_Municipality_ID;
                } else if (!baseUri.ToLower().Contains("spid"))
                {
                    NavManager.NavigateTo("https://innovation-consulting.it/", true);
                }
                IsBusy = false;
                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
