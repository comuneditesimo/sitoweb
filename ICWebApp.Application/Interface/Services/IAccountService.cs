using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Interface.Services
{
    public interface IAccountService
    {
        public Task<MSG_SystemMessages?> Login(Login UserLogin);
        public Task<MSG_SystemMessages?> LoginMunicipal(Login UserLogin);
        public Task<bool> Logout();
        public Task<MSG_SystemMessages?> PasswordForgotten(Login UserLogin, bool InTesting = false);
        public Task<AUTH_Users?> Register(AUTH_Users NewUser, AUTH_Users_Anagrafic? anagrafic = null);
        public Task<AUTH_Users?> RegistrationUpdate(AUTH_Users NewUser, AUTH_Users_Anagrafic? anagrafic = null);
        public Task<bool> SendVerificationEmail(AUTH_Users User, bool SelfClosing = false, bool InTesting = false);
        public Task<MSG_SystemMessages?> VerifyEmail(Guid AUTH_Users_ID, string EmailCode);
        public Task<MSG_SystemMessages?> VerifyPasswordResetToken(Guid PasswordResetToken);
        public Task<MSG_SystemMessages?> VerifyPasswordResetTokenMunicipalUser(Guid PasswordResetToken);
        public Task<bool> SendVerificationSMS(AUTH_Users User);
        public Task<MSG_SystemMessages?> VerifyPhone(Guid AUTH_Users_ID, string PhoneCode);
        public bool IsEmailUnique(string Email, Guid AUTH_Users_ID);
        public bool IsFiscalNumberUnique(string FiscalNumber, bool WithManualInput = false);
        public Task<MSG_SystemMessages?> ChangePassword(AUTH_Users User);
        public Task<MSG_SystemMessages?> MunicipalChangePassword(AUTH_Municipal_Users User);
        public Task<bool> HealthCheck();
        public Task<AUTH_Users?> RegisterExternal(AUTH_Users_Anagrafic Anagrafic, string RegistrationMode, Guid? AUTH_Municipality_ID = null);
        public Task<bool> LoginExternal(string UserID, string LoginToken);
        public Task<bool> SendWelcomeMail(AUTH_Users User);

        public Task<List<V_AUTH_Citizens>> GetRegistrationAdminList(Guid? municipalityID);
    }
}
