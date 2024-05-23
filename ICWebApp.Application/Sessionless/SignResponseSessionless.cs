using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.AdobeSign;
using Newtonsoft.Json;
using static SQLite.SQLite3;

namespace ICWebApp.Application.Sessionless
{
    public class SignResponseSessionless : ISignResponseSessionless
    {
        private IUnitOfWork _unitOfWork;
        public SignResponseSessionless(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        public async Task<Token?> SetAccessToken(Guid AUTH_Municipality, string code)
        {
            //TODO //URLUPDATE How de wo do this???
            var conf = await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality);
            var mun = await _unitOfWork.Repository<AUTH_Municipality>().GetByIDAsync(AUTH_Municipality);

            if (mun != null)
            {
                
                var prefix = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == mun.Prefix_Text_SystemTexts_Code && p.LANG_LanguagesID == LanguageSettings.German);
                
                if (conf != null && prefix != null && conf != null)
                {
                    //TODO urlupdate!!!!!!!!!!!!!
                    //var baseUrl = await UrlService.GetDefaultUrlByMunicipalityStatic(_unitOfWork, mun.ID); //need to consider languages aswell
                    var baseUrl = $"{prefix.Text}.comunix.bz.it";
                    //var token = await AdobeSignApiHelper.GetAccessToken("https://" + prefix.Text + ".comunix.bz.it/Sign/TokenResponse", code, conf.ClientID, conf.ClientSecret, conf.ApiAccessPoint);
                    var token = await AdobeSignApiHelper.GetAccessToken($"https://{baseUrl}/Sign/TokenResponse", code, conf.ClientID, conf.ClientSecret, conf.ApiAccessPoint);

                    if (token != null && token.access_token != null)
                    {
                        await SetAccessToken(AUTH_Municipality, token);
                    }
                }
            }

            return null;
        }
        public async Task<Token?> RefreshAccessToken(Guid CONF_ID, Guid AUTH_Municipality_ID)
        {
            var conf = await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.ID == CONF_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (conf != null)
            {
                if (conf.RefreshToken != null && conf.ClientID != null && conf.ClientSecret != null && conf.ApiAccessPoint != null)
                {
                    var RefreshToken = DataEncryptor.DecryptString(conf.RefreshToken, "5288073882064a8196a046ffcdd3b10a");

                    var token = await AdobeSignApiHelper.RefreshAccessToken(RefreshToken, conf.ClientID, conf.ClientSecret, conf.ApiAccessPoint);

                    if (token != null)
                    {
                        await SetAccessToken(AUTH_Municipality_ID, token);
                    }

                    return token;
                }
            }

            return null;
        }
        public async Task<Token?> RefreshAccessToken(Guid AUTH_Municipality_ID)
        {
            var conf = await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (conf != null)
            {
                if (conf.RefreshToken != null && conf.ClientID != null && conf.ClientSecret != null && conf.ApiAccessPoint != null)
                {
                    var RefreshToken = DataEncryptor.DecryptString(conf.RefreshToken, "5288073882064a8196a046ffcdd3b10a");

                    var token = await AdobeSignApiHelper.RefreshAccessToken(RefreshToken, conf.ClientID, conf.ClientSecret, conf.ApiAccessPoint);

                    if (token != null && token.access_token != null)
                    {
                        await SetAccessToken(AUTH_Municipality_ID, token);
                    }

                    return token;
                }
            }

            return null;
        }
        public async Task<bool> RevokeAccessToken(Guid AUTH_Municipality_ID)
        {
            var conf = await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (conf != null)
            {
                if (conf.AccessToken != null && conf.RefreshToken != null && conf.ClientID != null && conf.ClientSecret != null && conf.ApiAccessPoint != null)
                {
                    var AccessToken = DataEncryptor.DecryptString(conf.AccessToken, "7f2759355c2d43fdb12901d574915356");

                    var token = await AdobeSignApiHelper.RevokeAccessToken(AccessToken, conf.ClientID, conf.ClientSecret, conf.ApiAccessPoint);

                    if (token)
                    {
                        return await ClearAccessToken(AUTH_Municipality_ID);
                    }

                    return false;
                }
            }

            return false;
        }
        private async Task<bool> SetAccessToken(Guid AUTH_Municipality, Token Token)
        {
            var conf = await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality);

            if (conf != null)
            {
                if (Token != null && Token.access_token != null && Token.expires_in != null)
                {
                    conf.TokenExpirationDate = DateTime.Now.AddSeconds(double.Parse(Token.expires_in));
                    conf.AccessToken = DataEncryptor.EncryptString(Token.access_token, "7f2759355c2d43fdb12901d574915356");
                    conf.RefreshTokenExpirationDate = DateTime.Now.AddDays(60);
                    
                    if(Token.refresh_token != null)
                    {
                        conf.RefreshToken = DataEncryptor.EncryptString(Token.refresh_token, "5288073882064a8196a046ffcdd3b10a");
                    }

                    conf.RefreshTokenExpirationDate = DateTime.Now.AddDays(60);
                }
                

                await _unitOfWork.Repository<CONF_Sign>().InsertOrUpdateAsync(conf);
            }

            return true;
        }
        private async Task<bool> ClearAccessToken(Guid AUTH_Municipality_ID)
        {
            var conf = await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (conf != null)
            {
                conf.TokenExpirationDate = null;
                conf.AccessToken = null;

                await _unitOfWork.Repository<CONF_Sign>().InsertOrUpdateAsync(conf);
            }

            return true;
        }
    }
}
