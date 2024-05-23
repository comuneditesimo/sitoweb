using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Authorization
{
    public partial class ResetPasswordComponent
    {
        [Parameter] public string ID { get; set; }
        [Inject] public ISessionWrapper SessionWrapper { get; set; }
        [Inject] public IAccountService AccountService { get; set; }
        [Inject] public IAUTHProvider AuthProvider { get; set; }
        [Inject] public ITEXTProvider TextProvider { get; set; }
        [Inject] public NavigationManager NavManager { get; set; }
        private MSG_SystemMessages? Message { get; set; }
        private AUTH_Users? User { get; set; }
        private AUTH_Municipal_Users? MunUser { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (string.IsNullOrEmpty(ID))
                {
                    return;
                }

                Guid IDGuid;

                if (!Guid.TryParse(ID, out IDGuid))
                {
                    return;
                }

                SessionWrapper.AUTH_Municipality_ID = await SessionWrapper.GetMunicipalityID();

                Message = await AccountService.VerifyPasswordResetToken(Guid.Parse(ID));
                User = await AuthProvider.GetUserByPasswordToken(Guid.Parse(ID));

                if (User == null)
                {
                    Message = await AccountService.VerifyPasswordResetTokenMunicipalUser(Guid.Parse(ID));
                    MunUser = await AuthProvider.GetMunicipalUserByPasswordToken(Guid.Parse(ID));

                    if (MunUser == null)
                    {
                        return;
                    }
                }

                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        private void BackToLogin()
        {
            NavManager.NavigateTo("/", true);
        }
    }
}
