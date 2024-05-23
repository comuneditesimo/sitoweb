namespace ICWebApp.Application.Interface.Helper
{
    public interface IContactHelper
    {
        public string GetFilePath(string Name, bool? Gender = null, string? Stelle = null, string? Birthplace = null, DateTime? Birthdate = null, string? PhoneNumber = null, string? PhoneNumberPrivate = null, string? PhoneNumberMobile = null, string? EMail = null, string? PECMail = null, string? Base64Photo = null);
    }
}
