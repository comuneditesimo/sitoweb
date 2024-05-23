using AdobeSign.Rest;
using AdobeSign.Rest.Client;
using AdobeSign.Rest.Model.Agreements;
using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using sib_api_v3_sdk.Client;
using System.Diagnostics;
using System.Web;
using static System.Formats.Asn1.AsnWriter;

namespace ICWebApp.Application.Services
{
    public class SignService : ISignService
    {
        private readonly IFILEProvider _FILEProvider;
        private readonly ICONFProvider _CONFProvider;
        private readonly NavigationManager _navManager;
        private readonly ISessionWrapper _sessionWrapper;
        private readonly ITEXTProvider _textProvider;
        private readonly IAUTHProvider _authProvider;
        private readonly ISignResponseSessionless _SessionlessSignService;

        public SignService(ISessionWrapper _sessionWrapper, IFILEProvider _FILEProvider, NavigationManager _navManager,
                           ITEXTProvider _textProvider, ICONFProvider _CONFProvider, IAUTHProvider _authProvider, ISignResponseSessionless _SessionlessSignService)
        {
            this._sessionWrapper = _sessionWrapper;
            this._navManager = _navManager;
            this._FILEProvider = _FILEProvider;
            this._textProvider = _textProvider;
            this._CONFProvider = _CONFProvider;
            this._authProvider = _authProvider;
            this._SessionlessSignService = _SessionlessSignService;
        }
        public async Task<string?> StartLocalSignProcess(Guid FILE_Info_ID, bool NeedsMunicipalSign = false, SignerItem? SignerItem = null)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var conf = await _CONFProvider.GetSignConfiguration(null, _sessionWrapper.AUTH_Municipality_ID.Value);

                if (conf != null)
                {
                    var token = await GetToken();

                    if (token != null)
                    {
                        var authorization = "Bearer " + token;

                        var FileInfo = await _FILEProvider.GetFileInfoAsync(FILE_Info_ID);
                        bool freshlyCreated = false;

                        var docClient = new TransientDocumentsApi(conf.ApiAccessPoint + "/api/rest/v6");
                        

                        if (FileInfo != null)
                        {
                            //CREATE DOCUMENT ON ADOBE SERVER
                            if (FileInfo.AdobeSign_Document_ID == null)
                            {
                                var FileStorage = _FILEProvider.GetFileStorage(FILE_Info_ID);

                                if (FileStorage != null && FileStorage.FileImage != null && FileInfo.FileName != null)
                                {
                                    var response = docClient.CreateTransientDocument(authorization, new MemoryStream(FileStorage.FileImage), null, null, FileInfo.FileName + ".pdf", "application/PDF");

                                    if (response != null && response.TransientDocumentId != null)
                                    {
                                        FileInfo.AdobeSign_Document_ID = response.TransientDocumentId;

                                        await _FILEProvider.SetFileInfo(FileInfo);
                                    }
                                }
                            }

                            var agreementClient = new AgreementsApi(conf.ApiAccessPoint + "/api/rest/v6");

                            if (FileInfo.AdobeSign_Agreement_ID == null)
                            {

                                //CREATE AGGREEMENT ON ADOBE SERVER
                                if (SignerItem != null)
                                    freshlyCreated = await InitializeAgreement(agreementClient, authorization, FileInfo, new List<SignerItem>() { SignerItem }, NeedsMunicipalSign);
                                else
                                    freshlyCreated = await InitializeAgreement(agreementClient, authorization, FileInfo, null, NeedsMunicipalSign);
                            }

                            //GET SIGNINGURL
                            if (FileInfo != null && FileInfo.AdobeSign_Agreement_ID != null)
                            {
                                var SigningUrl = await InitializeSigningUrl(agreementClient, authorization, FileInfo, freshlyCreated);

                                if (SigningUrl != null)
                                {
                                    return SigningUrl;
                                }
                            }
                        }
                    }
                }
            }


            return null;
        }
        private async Task<string?> GetToken()
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var conf = await _CONFProvider.GetSignConfiguration(null, _sessionWrapper.AUTH_Municipality_ID.Value);

                if (conf != null)
                {
                    if (conf.AccessToken == null || conf.TokenExpirationDate == null || conf.TokenExpirationDate.Value < DateTime.Now)
                    {
                        if (conf.AUTH_Municipality_ID != null)
                        {
                            await _SessionlessSignService.RefreshAccessToken(conf.ID, conf.AUTH_Municipality_ID.Value);
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

            }

            return null;
        }
        public async Task<string?> StartMultiSignProcess(Guid FILE_Info_ID, List<SignerItem> Signers, bool NeedsMunicipalSign = false)
        {

            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var conf = await _CONFProvider.GetSignConfiguration(null, _sessionWrapper.AUTH_Municipality_ID.Value);

                if (conf != null)
                {
                    var token = await GetToken();

                    if (token != null)
                    {
                        var FileInfo = await _FILEProvider.GetFileInfoAsync(FILE_Info_ID);

                        if (FileInfo != null)
                        {
                            bool freshlyCreated = false;
                            var authorization = "Bearer " + token;

                            var docClient = new TransientDocumentsApi(conf.ApiAccessPoint + "/api/rest/v6");

                            //CREATE DOCUMENT ON ADOBE SERVER
                            if (FileInfo.AdobeSign_Document_ID == null)
                            {
                                var FileStorage = _FILEProvider.GetFileStorage(FILE_Info_ID);

                                if (FileStorage != null && FileStorage.FileImage != null && FileInfo.FileName != null)
                                {
                                    var documentID = await docClient.CreateTransientDocumentAsync(authorization, new MemoryStream(FileStorage.FileImage), null, null, FileInfo.FileName + ".pdf", "application/PDF");

                                    if (documentID != null && documentID.TransientDocumentId != null)
                                    {
                                        FileInfo.AdobeSign_Document_ID = documentID.TransientDocumentId;

                                        await _FILEProvider.SetFileInfo(FileInfo);
                                    }
                                }
                            }

                            var agreementClient = new AgreementsApi(conf.ApiAccessPoint + "/api/rest/v6");


                            if (FileInfo.AdobeSign_Agreement_ID == null)
                            {
                                //CREATE AGGREEMENT ON ADOBE SERVER
                                freshlyCreated = await InitializeAgreement(agreementClient, authorization, FileInfo, Signers, NeedsMunicipalSign, true);
                            }
                            //GET SIGNINGURL
                            if (FileInfo != null && FileInfo.AdobeSign_Agreement_ID != null)
                            {
                                var SigningUrl = await InitializeSigningUrl(agreementClient, authorization, FileInfo, freshlyCreated);

                                if (SigningUrl != null)
                                {
                                    return SigningUrl;
                                }
                            }
                        }
                    }
                }
            }

            //await RevokeAccessToken();

            return null;
        }
        private async Task<bool> InitializeAgreement(AgreementsApi ApiClient, string Authorization, FILE_FileInfo FileInfo, List<SignerItem>? Signers = null, bool NeedsMunicipalSign = false, bool SendMails = false)
        {
            if (FileInfo.AdobeSign_Document_ID != null && FileInfo.AdobeSign_Agreement_ID == null)
            {
                var User = _sessionWrapper.CurrentUser;

                if (User != null)
                {                    
                    var pariticipants = new List<ParticipantSetInfo>();

                    pariticipants.Add(new ParticipantSetInfo()
                    {
                        MemberInfos = new List<ParticipantSetMemberInfo>()
                        {
                                new ParticipantSetMemberInfo()
                                {
                                    Email = User.Email
                                }
                        },
                        Name = User.Firstname + " " + User.Lastname,
                        Role = ParticipantSetInfo.RoleEnum.SIGNER,
                        Order = 1
                    });
                    

                    int count = 2;

                    if (Signers != null)
                    {
                        foreach (var sign in Signers.Where(p => p.SelfSign != true))
                        {
                            pariticipants.Add(
                            new ParticipantSetInfo()
                            {
                                MemberInfos = new List<ParticipantSetMemberInfo>()
                                {
                                        new ParticipantSetMemberInfo()
                                        {
                                            Email = sign.Mail
                                        }
                                },
                                Name = sign.Name,
                                Role = ParticipantSetInfo.RoleEnum.SIGNER,
                                Label = _textProvider.Get("NOTIF_MULTISIGN_LABEL").Replace("{FirstName}", User.Firstname).Replace("{LastName}", User.Lastname),
                                Order = count
                            });
                        }
                    }

                    if (NeedsMunicipalSign)
                    {
                        var conf = await _CONFProvider.GetSignConfiguration(null);

                        if (conf != null && conf.AUTH_Municipality_ID != null)
                        {
                            var municipal = await _authProvider.GetMunicipality(conf.AUTH_Municipality_ID.Value);

                            if (municipal != null)
                            {
                                pariticipants.Add(
                                new ParticipantSetInfo()
                                {
                                    MemberInfos = new List<ParticipantSetMemberInfo>()
                                    {
                                        new ParticipantSetMemberInfo()
                                        {
                                            Email = conf.MunicipalSigningMail
                                        }
                                    },
                                    Name = municipal.Name,
                                    Role = ParticipantSetInfo.RoleEnum.SIGNER,
                                    Order = count
                                });
                            }
                        }
                    }

                    if (FileInfo.FileName != null)
                    {
                        var aggrementID = await CreateAgreement(ApiClient, Authorization, FileInfo.ID, FileInfo.AdobeSign_Document_ID, null, FileInfo.FileName, pariticipants, Signers, SendMails);

                        if (aggrementID != null)
                        {
                            FileInfo.AdobeSign_Agreement_ID = aggrementID;

                            await _FILEProvider.SetFileInfo(FileInfo);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private async Task<string?> InitializeSigningUrl(AgreementsApi ApiClient, string Authorization, FILE_FileInfo FileInfo, bool FreshlyCreated = false)
        {
            if (FileInfo != null)
            {
                if (FreshlyCreated)
                {
                    int count = 0;
                    while (count < 5)
                    {
                        FileInfo = await _FILEProvider.GetFileInfoAsync(FileInfo.ID);

                        if (FileInfo != null && FileInfo.AgreementCreated == true)
                        {
                            break;
                        }

                        Thread.Sleep(1000);
                        count++;
                    }
                }

                var User = _sessionWrapper.CurrentUser;

                if (User != null)
                {
                    try
                    {
                        var SigningUrl = GetSigningUrl(ApiClient, Authorization, FileInfo.AdobeSign_Agreement_ID, User.Email);

                        if (SigningUrl != null)
                        {
                            return SigningUrl;
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }

            return null;
        }
        public async Task<string?> GetSignOnURL(string State)
        {
            //URLUPDATE --TOTEST using current base uri should be fine
            var conf = await _CONFProvider.GetSignConfiguration(null, _sessionWrapper.AUTH_Municipality_ID);

            if (conf != null)
            {
                string requestURL = "";

                var municipality = await _authProvider.GetMunicipality(_sessionWrapper.AUTH_Municipality_ID.Value);

                var baseUrl = _navManager.BaseUri;
                requestURL = $"{conf.ApiAccessPoint}/public/oauth/v2?redirect_uri={HttpUtility.UrlEncode(baseUrl + "Sign/ApiRespons")}&response_type=code&client_id={conf.ClientID}&scope={HttpUtility.UrlEncode("agreement_read:account agreement_write:account agreement_send:account")}&state={HttpUtility.UrlEncode(State)}";

                return requestURL;
            }

            return null;
        }
        private async Task<string?> CreateAgreement(AgreementsApi ApiClient, string Authorization, Guid ExternalID, string DocumentID, string? Message, string AgreementName, List<ParticipantSetInfo> Participants, List<SignerItem>? Signers, bool SendMails = false)
        {
            if (ApiClient == null)
                return null;

            var AgreementCreationInfo = new AgreementCreationInfo();
            AgreementCreationInfo.ExternalId = new ExternalId() { Id = ExternalID.ToString() };
            AgreementCreationInfo.FileInfos = new List<AdobeSign.Rest.Model.Agreements.FileInfo>()
            {
                new AdobeSign.Rest.Model.Agreements.FileInfo()
                {
                        TransientDocumentId = DocumentID
                }
            };
            AgreementCreationInfo.Message = Message;
            AgreementCreationInfo.Name = AgreementName;
            AgreementCreationInfo.ParticipantSetsInfo = Participants;        
            AgreementCreationInfo.EmailOption = new EmailOption()
            {
                SendOptions = new SendOptions()
                {
                    InitEmails = SendOptions.InitEmailsEnum.NONE,
                    InFlightEmails = SendOptions.InFlightEmailsEnum.NONE,
                    CompletionEmails = SendOptions.CompletionEmailsEnum.NONE
                }
            };

            if (SendMails)
            {
                AgreementCreationInfo.EmailOption.SendOptions.InitEmails = SendOptions.InitEmailsEnum.ALL;
            }

            AgreementCreationInfo.SignatureType = AgreementCreationInfo.SignatureTypeEnum.ESIGN;

            if (Signers != null && Signers.Count() > 0 && Signers.FirstOrDefault(p => p.XPosition != null) != null)
            {
                AgreementCreationInfo.State = AgreementCreationInfo.StateEnum.AUTHORING;
            }
            else 
            {
                AgreementCreationInfo.State = AgreementCreationInfo.StateEnum.INPROCESS;
            }

            var agreement = await ApiClient.CreateAgreementAsyncWithHttpInfo(Authorization, AgreementCreationInfo);

            if (Signers != null && Signers.Count() > 0 && Signers.FirstOrDefault(p => p.XPosition != null) != null)
            {
                var memebers = await ApiClient.GetAllMembersAsync(Authorization, agreement.Data.Id);
                List<FormField> FormFields = new List<FormField>();

                if(memebers != null)
                {
                    foreach (var signItem in Signers)
                    {
                        var memeberID = memebers.ParticipantSets.Where(p => p.Name == signItem.Name).FirstOrDefault().Id;

                        if (signItem.Height != null && signItem.Width != null && signItem.XPosition != null && signItem.YPosition != null && signItem.PageNumber != null)
                        {
                            var formField = new FormField();

                            formField.Name = signItem.Name;
                            formField.InputType = FormField.InputTypeEnum.SIGNATURE;
                            formField.Locations = new List<FormFieldLocation>()
                            {
                                        new FormFieldLocation()
                                        {
                                            Height = double.Parse(signItem.Height),
                                            Width = double.Parse(signItem.Width),
                                            Left = double.Parse(signItem.XPosition),
                                            Top = double.Parse(signItem.YPosition),
                                            PageNumber = int.Parse(signItem.PageNumber)
                                        }
                            };
                            formField.ContentType = FormField.ContentTypeEnum.SIGNATURE;
                            formField.Assignee = memeberID;
                            formField.Visible = true;
                            formField.Required = true;

                            FormFields.Add(formField);
                        }
                    }
                }

                await GetFormFieldsAsync(ApiClient, Authorization, agreement.Data.Id);

                var fetchedFields = ApiClient.GetFormFieldsWithHttpInfo(Authorization, agreement.Data.Id);

                var etag = fetchedFields.Headers["ETag"];

                if (etag != null)
                {
                    var allFields = FormFields;
                    allFields.AddRange(fetchedFields.Data.Fields);

                    ApiClient.UpdateFormFields(Authorization, etag, agreement.Data.Id, new FormFieldPutInfo(allFields));
                }
            }

            return agreement.Data.Id;
        }
        private async Task<AdobeSign.Rest.Client.ApiResponse<AgreementFormFields>?> GetFormFieldsAsync(AgreementsApi ApiClient, string Authorization, string AgreementID, int count = 0)
        {
            if (count > 20)
                return null;

            AdobeSign.Rest.Client.ApiResponse<AgreementFormFields>? fields = null;

            try
            {
                fields = await ApiClient.GetFormFieldsAsyncWithHttpInfo(Authorization, AgreementID);
            }
            catch
            {
                await Task.Delay(800);
                return await GetFormFieldsAsync(ApiClient, Authorization, AgreementID, count++);
            }

            return fields;
        }
        public string? GetSigningUrl(AgreementsApi ApiClient, string Authorization, string AgreementID, string TargetMail)
        {
            if (ApiClient == null)
                return null;

            var s = ApiClient.GetSigningUrl(Authorization, AgreementID);

            if (s.SigningUrlSetInfos != null && s.SigningUrlSetInfos.Count() > 0)
            {
                var urlInfo = s.SigningUrlSetInfos.FirstOrDefault();

                if (urlInfo != null && urlInfo.SigningUrls != null && urlInfo.SigningUrls.Count() > 0)
                {
                    var SigningUrl = urlInfo.SigningUrls.FirstOrDefault(p => p.Email.ToLower() == TargetMail.ToLower());

                    if (SigningUrl != null)
                    {
                        return SigningUrl.EsignUrl;
                    }
                }
            }

            return null;
        }
        public async Task<string?> GetSigningUrl(string AgreementID, string TargetMail)
        {
            var conf = await _CONFProvider.GetSignConfiguration(null, _sessionWrapper.AUTH_Municipality_ID.Value);

            if (conf != null)
            {
                var token = await GetToken();

                if (token != null)
                {
                    var authorization = "Bearer " + token;

                    var agreementClient = new AgreementsApi(conf.ApiAccessPoint + "/api/rest/v6");

                    var s = agreementClient.GetSigningUrl(authorization, AgreementID);

                    if (s.SigningUrlSetInfos != null && s.SigningUrlSetInfos.Count() > 0)
                    {
                        var urlInfo = s.SigningUrlSetInfos.FirstOrDefault();

                        if (urlInfo != null && urlInfo.SigningUrls != null && urlInfo.SigningUrls.Count() > 0)
                        {
                            var SigningUrl = urlInfo.SigningUrls.FirstOrDefault(p => p.Email.ToLower() == TargetMail.ToLower());

                            if (SigningUrl != null)
                            {
                                return SigningUrl.EsignUrl;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}