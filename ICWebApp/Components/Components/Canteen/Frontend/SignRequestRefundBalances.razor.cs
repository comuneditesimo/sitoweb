using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using Telerik.Reporting.Processing;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using Telerik.Reporting;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Canteen.Frontend
{
    public partial class SignRequestRefundBalances : IDisposable
    {
        [Inject] IBusyIndicatorService? BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService? BreadCrumbService { get; set; }
        [Inject] ICANTEENProvider? CanteenProvider { get; set; }
        [Inject] ISessionWrapper? SessionWrapper { get; set; }
        [Inject] IMyCivisService? MyCivisService { get; set; }
        [Inject] IMessageService? MessageService { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }
        [Inject] ITEXTProvider? TextProvider { get; set; }
        [Inject] IFILEProvider? FileProvider { get; set; }
        [Inject] IAUTHProvider? AuthProvider { get; set; }
        [Inject] NavigationManager? NavManager { get; set; }
        [Parameter] public string? RequestRefundBalanceID { get; set; }

        private CANTEEN_RequestRefundBalances? Request;
        protected override async Task OnInitializedAsync()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
            if (SessionWrapper != null)
            {
                SessionWrapper.OnCurrentSubUserChanged += SessionWrapper_OnCurrentUserChanged;
                if (TextProvider != null)
                {
                    SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_SIGN_REQUESTREFUND_TITLE");
                    SessionWrapper.PageDescription = null;
                }
            }

            SetBreadCrumb();

            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }

            Telerik.Reporting.ReportParameter _reportParameterID = new Telerik.Reporting.ReportParameter();
            ReportPackager _reportPackager = new ReportPackager();
            LANG_Languages? _currentLanguage = null;
            string _reportFileName = "MensaRefundBalances.trdp";
            string _reportBasePath = "";
            CANTEEN_RequestRefundBalances? _request = null;

            if (CanteenProvider != null && FileProvider != null)
            {
                if (!string.IsNullOrEmpty(RequestRefundBalanceID))
                {
                    _request = await CanteenProvider.GetRequestRefundBalance(RequestRefundBalanceID);
                }
                if (_request != null && _request.FILE_FileInfoID == null && _request.SignedDate == null)
                {
                    _reportParameterID.Name = "RequestRefundBalanceID";
                    _reportParameterID.Text = _request.ID.ToString();
                    _reportParameterID.Value = _request.ID.ToString();
                    _reportParameterID.Type = Telerik.Reporting.ReportParameterType.String;
                    _reportParameterID.AllowBlank = false;
                    _reportParameterID.AllowNull = false;

                    if (LangProvider != null)
                    {
                        _currentLanguage = LangProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
                    }
                    if (_currentLanguage != null)
                    {
                        _reportFileName = "MensaRefundBalances" + _currentLanguage.Code + ".trdp";
                    }
                    _reportBasePath = @"D:\Comunix\Reports\" + _reportFileName;

                    if (NavManager != null && NavManager.BaseUri.Contains("localhost"))
                    {
                        _reportBasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\" + _reportFileName;
                    }
                    try
                    {
                        using (FileStream? _sourceStream = System.IO.File.OpenRead(_reportBasePath))
                        {
                            Telerik.Reporting.Report? _report = (Telerik.Reporting.Report)_reportPackager.UnpackageDocument(_sourceStream);
                            _report.ReportParameters.Clear();
                            _report.ReportParameters.Add(_reportParameterID);

                            InstanceReportSource _reportSource = new InstanceReportSource();
                            _reportSource.ReportDocument = _report;

                            System.Collections.Hashtable _deviceInfo = new System.Collections.Hashtable();
                            _deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

                            ReportProcessor _reportProcessor = new ReportProcessor();
                            RenderingResult _result = _reportProcessor.RenderReport("PDF", _reportSource, _deviceInfo);

                            MemoryStream _memoryStream = new MemoryStream(_result.DocumentBytes);
                            _memoryStream.Position = 0;

                            FILE_FileInfo? _fileInfo = new FILE_FileInfo();
                            _fileInfo.ID = Guid.NewGuid();
                            _fileInfo.FileName = TextProvider?.Get("CANTEEN_REQUESTREFUNDBALANCES_DOCTITLE");
                            _fileInfo.CreationDate = DateTime.Now;
                            if (SessionWrapper != null && SessionWrapper.CurrentUser != null)
                            {
                                _fileInfo.AUTH_Users_ID = SessionWrapper.CurrentUser.ID;
                            }
                            _fileInfo.FileExtension = ".pdf";
                            _fileInfo.Size = _memoryStream.Length;

                            FILE_FileStorage _fileStorage = new FILE_FileStorage();

                            _fileStorage.ID = Guid.NewGuid();
                            _fileStorage.CreationDate = DateTime.Now;
                            _fileStorage.FILE_FileInfo_ID = _fileInfo.ID;
                            _fileStorage.FileImage = _memoryStream.ToArray();

                            _fileInfo.FILE_FileStorage = new List<FILE_FileStorage>();
                            _fileInfo.FILE_FileStorage.Add(_fileStorage);
                            _fileInfo = await FileProvider.SetFileInfo(_fileInfo);

                            if (CanteenProvider != null && _fileInfo != null)
                            {
                                _request.FILE_FileInfoID = _fileInfo.ID;
                                _request = await CanteenProvider.SetRequestRefundBalance(_request);
                            }
                            else
                            {
                            }
                        }
                        Request = _request;
                    }
                    catch
                    {
                    }
                }
                else if (_request != null && _request.SignedDate == null)
                {
                    Request = _request;
                }
                else
                {
                    Request = null;
                }
            }
            else
            {
                Request = null;
            }

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }

            await base.OnParametersSetAsync();
        }
        private void SetBreadCrumb()
        {
            if (BreadCrumbService != null)
            {
                BreadCrumbService.ClearBreadCrumb();
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/SignRefundBalances", "CANTEEN_SIGN_REQUESTREFUND_SIGN", null, null, false);
                }
                else
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/SignRefundBalances", "CANTEEN_SIGN_REQUESTREFUND_SIGN", null, null, false);
                }
            }
        }
        private void SessionWrapper_OnCurrentUserChanged()
        {
            ReturnToPreviousPage();
        }
        private void ReturnToPreviousPage()
        {
            if (NavManager != null)
            {
                if (BusyIndicatorService != null)
                {
                    BusyIndicatorService.IsBusy = true;
                    StateHasChanged();
                }
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    NavManager.NavigateTo("/Canteen/MyCivis/Service");
                }
                else
                {
                    NavManager.NavigateTo("/Canteen/Service");
                }
            }
        }
        private async void DocumentSigned()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
            if (Request != null && CanteenProvider != null)
            {
                CANTEEN_RequestRefundBalances_Status? _signedStatus = await CanteenProvider.GetSignedRequestRefundBalanceStatus();
                if (_signedStatus != null)
                {
                    if (Request.AUTH_Municipality_ID != null)
                    {
                        Request.ProgressivYear = DateTime.Today.Year;
                        long _lastNumber = await CanteenProvider.GetLatestProgressivNumberRequestRefundBalances(Request.AUTH_Municipality_ID.Value, Request.ProgressivYear.Value);
                        Request.ProgressivNumber = _lastNumber + 1;
                    }
                    Request.CANTEEN_RequestRefundAllCharge_Status_ID = _signedStatus.ID;
                    Request.SignedDate = DateTime.Now;
                    Request = await CanteenProvider.SetRequestRefundBalance(Request);
                    if (Request != null)
                    {
                        await CanteenProvider.CreateRequestRefundBalanceStatusLogEntry(Request);
                    }
                }
            }

            if (Request != null && NavManager != null && MessageService != null && SessionWrapper != null && SessionWrapper.CurrentUser != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                List<MSG_Message_Parameters> _parameters = new List<MSG_Message_Parameters>();
                _parameters.Add(new MSG_Message_Parameters()
                {
                    Code = "{0}",
                    Message = string.Format("{0:C}", Request.Fee)
                });
                MSG_Message? _message = await MessageService.GetMessage(SessionWrapper.CurrentUser.ID, SessionWrapper.AUTH_Municipality_ID.Value, "NOTIF_CANTEEN_REQREFUNDBALANCE_TEXT", "NOTIF_CANTEEN_REQREFUNDBALANCE_SHORTTEXT", "NOTIF_CANTEEN_REQREFUNDBALANCE_TITLE", Guid.Parse("dcd04015-c1bd-4ad5-99e6-aeef7f35bfa4"), true, _parameters);

                if (_message != null)
                {
                    await MessageService.SendMessage(_message, NavManager.BaseUri + "/Canteen/Service");
                }
                // Parameter für E-Mail Gemeinde ausfüllen
                AUTH_Users_Anagrafic? _user = SessionWrapper.CurrentUser.AUTH_Users_Anagrafic.FirstOrDefault();
                if (_user == null && AuthProvider != null)
                {
                    _user = await AuthProvider.GetAnagraficByUserID(SessionWrapper.CurrentUser.ID);
                }
                string protocollNumber = "";
                string formName = "";
                if (AuthProvider != null)
                {
                    AUTH_Municipality? _municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);
                    if (_municipality != null)
                    {
                        protocollNumber = _municipality.MensaRefundPNPrefix;
                    }
                }
                protocollNumber += "-" + Request.ProgressivYear + "-" + Request.ProgressivNumber;
                if (TextProvider != null)
                {
                    formName = TextProvider.Get("NOTIF_CANTEEN_REQREFUNDBALANCE_FORMNAME");
                }

                _parameters = new List<MSG_Message_Parameters>();
                _parameters.Add(new MSG_Message_Parameters()
                {
                    Code = "{FormName}",
                    Message = formName
                });
                _parameters.Add(new MSG_Message_Parameters()
                {
                    Code = "{ProtocolNumber}",
                    Message = protocollNumber
                });
                _parameters.Add(new MSG_Message_Parameters()
                {
                    Code = "{ApplicantName}",
                    Message = SessionWrapper.CurrentUser.Lastname + " " + SessionWrapper.CurrentUser.Firstname
                });
                if (_user != null)
                {
                    _parameters.Add(new MSG_Message_Parameters()
                    {
                        Code = "{ApplicantTaxNumber}",
                        Message = _user.FiscalNumber
                    });
                }
                else
                {
                    _parameters.Add(new MSG_Message_Parameters()
                    {
                        Code = "{ApplicantTaxNumber}",
                        Message = ""
                    });
                }
                /*
                        msg.Subject = msg.Subject.Replace("{FormName}", definition.FORM_Name);
                        msg.Messagetext = msg.Messagetext.Replace("{FormName}", definition.FORM_Name);
                        msg.Messagetext = msg.Messagetext.Replace("{ProtocolNumber}", definition.FormCode + "-" + Application.ProgressivYear + "-" + Application.ProgressivNumber);
                        msg.Messagetext = msg.Messagetext.Replace("{ApplicantName}", Application.FirstName + " " + Application.LastName);
                        msg.Messagetext = msg.Messagetext.Replace("{ApplicantTaxNumber}", Application.FiscalNumber);
                    var msg = await _messageService.GetMessage(Application.AUTH_Users_ID.Value, Application.AUTH_Municipality_ID.Value, "MUN_APPLICATION_COMITTED_MAIL_TEXT", "MUN_APPLICATION_COMITTED_MAIL_SHORTTEXT", "MUN_APPLICATION_COMITTED_MAIL_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true);
                 */
                MSG_Message? _authorityMessage = await MessageService.GetMessage(SessionWrapper.CurrentUser.ID, SessionWrapper.AUTH_Municipality_ID.Value, "NOTIF_MUN_CANTEEN_REQREFUNDBALANCE_TEXT", "NOTIF_MUN_CANTEEN_REQREFUNDBALANCE_SHORTTEXT", "NOTIF_MUN_CANTEEN_REQREFUNDBALANCE_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true, _parameters);

                if (AuthProvider != null)
                {
                    List<AUTH_Authority> _authorityList = await AuthProvider.GetAuthorityList(SessionWrapper.AUTH_Municipality_ID.Value, null, null);

                    AUTH_Authority? _subsititutionAuthority = _authorityList.FirstOrDefault(p => p.IsMensa == true);

                    if (_authorityMessage != null && _subsititutionAuthority != null)
                    {
                        await MessageService.SendMessageToAuthority(_subsititutionAuthority.ID, _authorityMessage, NavManager.BaseUri + "/Backend/Canteen/RequestRefundBalances/Details/" + Request.ID.ToString());
                    }
                }
            }

            if (NavManager != null)
            {
                if (BusyIndicatorService != null)
                {
                    BusyIndicatorService.IsBusy = true;
                    StateHasChanged();
                }
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    NavManager.NavigateTo("/Canteen/MyCivis/RequestRefundBalances/Committed");
                }
                else
                {
                    NavManager.NavigateTo("/Canteen/RequestRefundBalances/Committed");
                }
            }

            StateHasChanged();
        }
        public void Dispose()
        {
            if (SessionWrapper != null)
            {
                SessionWrapper.OnCurrentSubUserChanged -= SessionWrapper_OnCurrentUserChanged;
            }
        }
    }
}
