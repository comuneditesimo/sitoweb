using AdobeSign.Rest;
using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using RestSharp;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace ICWebApp.Application.Services
{
    public class SignResponseService : ISignResponseService
    {
        private readonly IFILEProvider _FILEProvider;
        private readonly ICONFProvider _CONFProvider;
        private readonly IAUTHProvider _AUTHProvider;
        private readonly ISignResponseSessionless _ResponseSessionless;

        public SignResponseService(IFILEProvider _FILEProvider, ICONFProvider _CONFProvider, IAUTHProvider _AUTHProvider, ISignResponseSessionless _ResponseSessionless)
        {
            this._FILEProvider = _FILEProvider;
            this._CONFProvider = _CONFProvider;
            this._AUTHProvider = _AUTHProvider;
            this._ResponseSessionless = _ResponseSessionless;
        }
        public async Task<bool> SetSignedAgreement(string AgreementID)
        {
            var FileInfo = await _FILEProvider.GetFileInfoByAgreementID(AgreementID);

            if (FileInfo != null)
            {
                if (FileInfo.AUTH_Users_ID != null)
                {
                    var User = await _AUTHProvider.GetUserWithoutMunicipality(FileInfo.AUTH_Users_ID.Value);

                    if (User != null && User.AUTH_Municipality_ID != null)
                    {
                        var data = await GetCombinedDocument(AgreementID, User.AUTH_Municipality_ID.Value);

                        if (data != null)
                        {
                            var FileStorage = await _FILEProvider.GetFileStorageAsync(FileInfo.ID);

                            if (FileStorage != null)
                            {
                                FileStorage.FileImage = data;

                                await _FILEProvider.SetFileStorage(FileStorage);

                                FileInfo.Signed = true;

                                await _FILEProvider.SetFileInfo(FileInfo);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        public async Task<bool> SetAgreementCreated(string AgreementID)
        {
            var FileInfo = await _FILEProvider.GetFileInfoByAgreementID(AgreementID);

            if (FileInfo != null)
            {
                if (FileInfo.AUTH_Users_ID != null)
                {
                    var User = await _AUTHProvider.GetUserWithoutMunicipality(FileInfo.AUTH_Users_ID.Value);

                    if (User != null && User.AUTH_Municipality_ID != null)
                    {
                        var data = await GetCombinedDocument(AgreementID, User.AUTH_Municipality_ID.Value);

                        if (data != null)
                        {
                            FileInfo.AgreementCreated = true;

                            await _FILEProvider.SetFileInfo(FileInfo);

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public async Task<bool> SetAgreementComitted(string AgreementID, string UserMail)
        {
            var FileInfo = await _FILEProvider.GetFileInfoByAgreementID(AgreementID);

            if (FileInfo != null)
            {
                if (FileInfo.AUTH_Users_ID != null)
                {
                    var User = await _AUTHProvider.GetUserWithoutMunicipality(FileInfo.AUTH_Users_ID.Value);

                    if (User != null && User.AUTH_Municipality_ID != null && User.Email == UserMail)
                    {
                        var data = await GetCombinedDocument(AgreementID, User.AUTH_Municipality_ID.Value);

                        if (data != null)
                        {
                            FileInfo.AgreementComitted = true;

                            await _FILEProvider.SetFileInfo(FileInfo);

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private async Task<string?> GetToken(Guid AUTH_Municipality_ID)
        {
            var conf = await _CONFProvider.GetSignConfiguration(null, AUTH_Municipality_ID);

            if (conf != null)
            {
                if (conf.AccessToken == null || conf.TokenExpirationDate == null || conf.TokenExpirationDate.Value < DateTime.Now)
                {
                    if (conf.AUTH_Municipality_ID != null)
                    {
                        await _ResponseSessionless.RefreshAccessToken(conf.ID, conf.AUTH_Municipality_ID.Value);
                    }
                }

                if (conf.AUTH_Municipality_ID != null)
                {
                    conf = await _CONFProvider.GetSignConfiguration(conf.ID, conf.AUTH_Municipality_ID.Value);
                }

                if (conf != null && conf.AccessToken != null)
                {
                    var token = DataEncryptor.DecryptString(conf.AccessToken, "7f2759355c2d43fdb12901d574915356");

                    return token;
                }
            }

            return null;
        }
        private async Task<byte[]?> GetCombinedDocument(string AgreementID, Guid AUTH_Municipality_ID)
        {
            var conf = await _CONFProvider.GetSignConfiguration(null, AUTH_Municipality_ID);
            var token = await GetToken(AUTH_Municipality_ID);

            if (conf != null && token != null)
            {
                var authorization = "Bearer " + token;

                var client = new AgreementsApi(conf.ApiAccessPoint + "/api/rest/v6");

                var file = client.GetCombinedDocument(authorization, AgreementID);

                return file;
            }

            return null;
        }
    }
}