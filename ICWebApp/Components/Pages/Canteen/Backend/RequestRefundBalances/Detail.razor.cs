using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models.Textvorlagen;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using Telerik.Reporting.Processing;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Globalization;
using Telerik.Reporting;
using PdfSharp.Pdf.IO;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;
using PdfSharp.Pdf;
using System.Text;

namespace ICWebApp.Components.Pages.Canteen.Backend.RequestRefundBalances
{
    public partial class Detail : IDisposable
    {
        // Injections
        [Inject] ICanteenRequestsAdministrationHelper? CanteenRequestsAdministrationHelper { get; set; }
        [Inject] IFORM_ReportPrintHelper? FormReportPrintHelper { get; set; }
        [Inject] IBusyIndicatorService? BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService? BreadcrumbService { get; set; }
        [Inject] ICANTEENProvider? CanteenProvider { get; set; }
        [Inject] ISessionWrapper? SessionWrapper { get; set; }
        [Inject] IMessageService? MessageService { get; set; }
        [Inject] NavigationManager? NavManager { get; set; }
        [Inject] IFILEProvider? FileProvider { get; set; }
        [Inject] ITEXTProvider? TextProvider { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }
        [Inject] IAUTHProvider? AuthProvider { get; set; }
        // Parameter
        [Inject] public SfDialogService? Dialogs { get; set; }
        [Parameter] public string? ID { get; set; }
        // Lokale Variablen
        private Administration_Filter_CanteenRequestRefundBalances _filter = new Administration_Filter_CanteenRequestRefundBalances();
        private List<CANTEEN_RequestRefundBalances_Status> _statusList = new List<CANTEEN_RequestRefundBalances_Status>();
        private List<V_CANTEEN_RequestRefundBalances> _requests = new List<V_CANTEEN_RequestRefundBalances>();
        private AUTH_Municipality? _currentMunicipality = null;
        private CANTEEN_RequestRefundBalances? _currentRequest = null;
        private CANTEEN_Subscriber_Movements? _currentMovement = new CANTEEN_Subscriber_Movements();
        private List<CANTEEN_RequestRefundBalances_Status_Log> _currentStatusLogList = new List<CANTEEN_RequestRefundBalances_Status_Log>();
        private TextItem _textItem = new TextItem();
        private int _requestTabIndex = 0;
        private bool _showModifyMovementModalWindow = false;
        private bool _showModifyStatusModalWindow = false;
        // Eigenschaften
        private int RequestTabIndex
        {
            get
            {
                return _requestTabIndex;
            }
            set
            {
                _requestTabIndex = value;
                OnTabIndexChanged(_requestTabIndex);
            }
        }
        private string? CurrentWizardTitle { get; set; }
        private bool IsDataBusy { get; set; } = true;
        private bool IsWizardBusy { get; set; } = true;
        private Guid? StartStatus { get; set; }
        private bool FilterWindowVisible { get; set; } = false;
        // Override Methoden
        protected override async Task OnInitializedAsync()
        {
            // Setze Titel
            if (SessionWrapper != null && TextProvider != null)
            {
                SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS");
            }
            // Setze aktuelle Gemeinde
            if (SessionWrapper != null)
            {
                SessionWrapper.OnAUTHMunicipalityIDChanged += OnMunicipalityIDChanged;
                if (AuthProvider != null && SessionWrapper.AUTH_Municipality_ID != null)
                {
                    _currentMunicipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);
                }
            }
            if (_currentMunicipality == null)
            {
                BackToPreviousPage();
            }
            // Lese Status ein
            _statusList = await GetStatus();
            // Initialisiere Filter
            if (CanteenRequestsAdministrationHelper != null && CanteenRequestsAdministrationHelper.Filter != null)
            {
                _filter = CanteenRequestsAdministrationHelper.Filter;
            }
            else
            {
                _filter = new Administration_Filter_CanteenRequestRefundBalances();
                _filter.CANTEEN_RequestRefundBalances_Status_ID = new List<Guid>();

                foreach (CANTEEN_RequestRefundBalances_Status _status in _statusList)
                {
                    _filter.CANTEEN_RequestRefundBalances_Status_ID.Add(_status.ID);
                }
            }

            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            // Lokale Variablen für Istanzierung
            Guid? requestID = null;
            Guid? startStatus = null;
            Administration_Filter_CanteenRequestRefundBalances filter = _filter;
            List<V_CANTEEN_RequestRefundBalances> requests = new List<V_CANTEEN_RequestRefundBalances>();
            List<CANTEEN_RequestRefundBalances_Status_Log> requestStatusLog = new List<CANTEEN_RequestRefundBalances_Status_Log>();
            CANTEEN_RequestRefundBalances? currentRequest = null;
            CANTEEN_Subscriber_Movements? currentMovement = null;
            // BusyIndicators setzen
            IsDataBusy = true;
            IsWizardBusy = true;
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
            }
            StateHasChanged();
            // Setze Titel
            if (TextProvider != null)
            {
                CurrentWizardTitle = TextProvider.Get("BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS_TAB_OVERVIEW");
            }
            // Versuch ID in ein Guid umzuwandeln
            if (!string.IsNullOrEmpty(ID))
            {
                try
                {
                    requestID = Guid.Parse(ID);
                }
                catch
                {
                }
            }
            if (requestID == null)
            {
                BackToPreviousPage();
                return;
            }
            // Setze Breadcrumb
            SetBreadCrumb(requestID.Value.ToString());
            // Hole Daten
            requests = await GetRequests(filter);
            currentRequest = await GetCurrentRequest(requestID);
            if (currentRequest != null)
            {
                currentMovement = await GetCurrentMovement(currentRequest);
                requestStatusLog = await GetCurrentStatusLog(currentRequest);
                startStatus = currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID;
            }

            // Schreibe istanzierte Variablen in Klassen-Variablen
            _filter = filter;
            _requests = requests;
            _currentRequest = currentRequest;
            _currentStatusLogList = requestStatusLog;
            _currentMovement = currentMovement;
            StartStatus = startStatus;
            // BusyIndicators rücksetzen
            IsDataBusy = false;
            IsWizardBusy = false;
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
            }
            StateHasChanged();
            await base.OnParametersSetAsync();
        }
        // Event Triggered Method
        private void OnMunicipalityIDChanged()
        {
            if (_currentRequest != null && SessionWrapper != null && _currentRequest.AUTH_Municipality_ID != SessionWrapper.AUTH_Municipality_ID)
            {
                BackToPreviousPage();
            }
        }
        // Methoden
        private void SetBreadCrumb(string RequestID)
        {
            if (BreadcrumbService != null)
            {
                BreadcrumbService.ClearBreadCrumb();
                BreadcrumbService.AddBreadCrumb("/Backend/Canteen/RequestRefundBalances/Administration", "MAINMENU_BACKEND_CANTEEN_REQUESTREFUNDBALANCES_ADMINISTRATION", null, null);
                BreadcrumbService.AddBreadCrumb("/Backend/Canteen/RequestRefundBalances/Details/" + RequestID, "MAINMENU_BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS", null, null, true);
            }
        }
        private async Task<List<CANTEEN_RequestRefundBalances_Status>> GetStatus()
        {
            List<CANTEEN_RequestRefundBalances_Status> statusList = new List<CANTEEN_RequestRefundBalances_Status>();
            List<CANTEEN_RequestRefundBalances_Status> result = new List<CANTEEN_RequestRefundBalances_Status>();

            if (CanteenProvider != null)
            {
                statusList = await CanteenProvider.GetAllRequestRefundBalanceStatus();
                foreach (CANTEEN_RequestRefundBalances_Status status in statusList)
                {
                    if (TextProvider != null)
                    {
                        status.Name = TextProvider.Get(status.TEXT_SystemTexts_Code);
                    }
                    result.Add(status);
                }
            }

            return result;
        }
        private async Task<CANTEEN_RequestRefundBalances?> GetCurrentRequest(Guid? RequestID)
        {
            CANTEEN_RequestRefundBalances? currentRequest = null;

            if (CanteenProvider != null && RequestID != null)
            {
                currentRequest = await CanteenProvider.GetRequestRefundBalance(RequestID.Value.ToString());
            }

            return currentRequest;
        }
        private async Task<List<V_CANTEEN_RequestRefundBalances>> GetRequests(Administration_Filter_CanteenRequestRefundBalances? Filter)
        {
            List<V_CANTEEN_RequestRefundBalances> requests = new List<V_CANTEEN_RequestRefundBalances>();

            if (_currentMunicipality != null && CanteenProvider != null)
            {
                if (LangProvider != null)
                {
                    requests = await CanteenProvider.GetRequestRefundBalances(_currentMunicipality.ID, Filter, LangProvider.GetCurrentLanguageID());
                }
                else
                {
                    requests = await CanteenProvider.GetRequestRefundBalances(_currentMunicipality.ID, Filter, LanguageSettings.German);
                }
            }

            if (CanteenRequestsAdministrationHelper != null)
            {
                CanteenRequestsAdministrationHelper.Filter = Filter;
            }

            return requests;
        }
        private async Task<List<CANTEEN_RequestRefundBalances_Status_Log>> GetCurrentStatusLog(CANTEEN_RequestRefundBalances? Request)
        {
            List<CANTEEN_RequestRefundBalances_Status_Log> requestLog = new List<CANTEEN_RequestRefundBalances_Status_Log>();

            if (CanteenProvider != null && Request != null)
            {
                if (LangProvider != null)
                {
                    requestLog.AddRange(await CanteenProvider.GetAllRequestRefundBalanceStatusLogEntries(Request.ID, LangProvider.GetCurrentLanguageID()));
                }
                else
                {
                    requestLog.AddRange(await CanteenProvider.GetAllRequestRefundBalanceStatusLogEntries(Request.ID, LanguageSettings.German));
                }
                if (Request.AUTH_User_ID != null && Request.Date != null && TextProvider != null)
                {
                    requestLog.Add(new CANTEEN_RequestRefundBalances_Status_Log()
                    {
                        ID = Guid.NewGuid(),
                        AUTH_Users_ID = Request.AUTH_User_ID.Value,
                        User = Request.UserFirstName + " " + Request.UserLastName,
                        ChangeDate = Request.Date.Value,
                        StatusIcon = "fa-regular fa-file-plus",
                        Status = TextProvider?.Get("FORM_USER_DETAIL_CREATED_STATUS")
                    });
                }
            }

            return requestLog;
        }
        private async Task<CANTEEN_Subscriber_Movements?> GetCurrentMovement(CANTEEN_RequestRefundBalances? Request)
        {
            CANTEEN_Subscriber_Movements? currentMovement = null;

            if (CanteenProvider != null && Request != null)
            {
                currentMovement = await CanteenProvider.GetSubscriberMovementByRequestID(Request.ID);
            }

            return currentMovement;
        }

        private async void FilterSearch(Administration_Filter_CanteenRequestRefundBalances Filter)
        {
            FilterWindowVisible = false;
            IsDataBusy = true;
            IsWizardBusy = true;
            StateHasChanged();

            _requests = await GetRequests(Filter);
            this._filter = Filter;

            IsDataBusy = false;
            IsWizardBusy = false;
            StateHasChanged();
        }
        private void ShowFilter()
        {
            FilterWindowVisible = true;
            StateHasChanged();
        }
        private void FilterClose()
        {
            FilterWindowVisible = false;
            StateHasChanged();
        }

        private void SelectRequest(Guid? RequestID)
        {
            if (RequestID != null && (_currentRequest == null || RequestID != _currentRequest.ID))
            {
                if (BusyIndicatorService != null)
                {
                    BusyIndicatorService.IsBusy = true;
                    StateHasChanged();
                }
                if (NavManager != null)
                {
                    NavManager.NavigateTo("/Backend/Canteen/RequestRefundBalances/Details/" + RequestID.ToString());
                }
            }
        }

        private void OnStepChanged()
        {
            IsWizardBusy = true;
            StateHasChanged();
        }
        private void OnTabIndexChanged(int Index)
        {
            if (TextProvider != null)
            {
                switch (Index)
                {
                    case 0:
                        CurrentWizardTitle = TextProvider.Get("BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS_TAB_OVERVIEW");
                        break;
                    case 1:
                        CurrentWizardTitle = TextProvider.Get("BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS_TAB_ANAGRAFIC");
                        break;
                    case 2:
                        CurrentWizardTitle = TextProvider.Get("BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS_TAB_PREVIEW");
                        break;
                    case 3:
                        CurrentWizardTitle = TextProvider.Get("BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS_TAB_MOVEMENT");
                        break;
                    case 4:
                        CurrentWizardTitle = TextProvider.Get("BACKEND_CANTEEN_REQUESTREFUNDBALANCES_DETAILS_TAB_STATUS_LOG");
                        break;
                }
            }

            IsWizardBusy = false;
            StateHasChanged();
        }
        private async void ArchivedChanged()
        {
            if (_currentRequest != null)
            {
                if (BusyIndicatorService != null)
                {
                    BusyIndicatorService.IsBusy = true;
                    StateHasChanged();
                }

                if (_currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID != StartStatus)
                {
                    _currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID = StartStatus;
                }

                if (CanteenProvider != null)
                {
                    _currentRequest = await CanteenProvider.SetRequestRefundBalance(_currentRequest);
                    _requests = await GetRequests(_filter);
                }

                if (BusyIndicatorService != null)
                {
                    BusyIndicatorService.IsBusy = false;
                }
                StateHasChanged();
            }
        }

        private async Task<bool> ChangeStatus()
        {
            // BusyIndicator setzen
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
            if (_currentRequest != null && CanteenProvider != null && (_currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID == Canteen_Request_Status.Accepted || _currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID == Canteen_Request_Status.Declined))
            {
                // Definiere istanzierte Variablen
                TextItem textItem = _textItem;
                AUTH_Municipality? municipality = _currentMunicipality;
                CANTEEN_RequestRefundBalances? request = _currentRequest;
                CANTEEN_RequestRefundBalances_Status? status = _statusList.FirstOrDefault(p => p.ID == request.CANTEEN_RequestRefundAllCharge_Status_ID);
                CANTEEN_RequestRefundBalances_Status? previousStatus = null;
                CANTEEN_Subscriber_Movements? currentMovement = null;
                List<CANTEEN_RequestRefundBalances_Status_Log> requestStatusLog = new List<CANTEEN_RequestRefundBalances_Status_Log>();
                List<LANG_Languages> languages = new List<LANG_Languages>();
                // Status wurde geändert
                if (status != null && status.ID != StartStatus)
                {
                    // Vorherigen Status setzen
                    CANTEEN_RequestRefundBalances? previousRequest = await CanteenProvider.GetRequestRefundBalance(request.ID.ToString());
                    if (previousRequest != null)
                    {
                        previousStatus = _statusList.FirstOrDefault(p => p.ID == previousRequest.CANTEEN_RequestRefundAllCharge_Status_ID);
                    }
                    // Bestätigung Nutzer einholen
                    if (Dialogs != null && !await Dialogs.ConfirmAsync(TextProvider?.Get("APPLICATION_STATUS_CHANGE_ARE_YOU_SURE"), TextProvider?.Get("WARNING")))
                    {
                        if (BusyIndicatorService != null)
                        {
                            BusyIndicatorService.IsBusy = false;
                            StateHasChanged();
                        }
                        return false;
                    }
                    _showModifyStatusModalWindow = false;
                    StateHasChanged();
                    // Neuen Status speichern
                    request = await CanteenProvider.SetRequestRefundBalance(request);
                    if (request != null)
                    {
                        if (LangProvider != null)
                        {
                            languages = await LangProvider.GetAll() ?? new List<LANG_Languages>();
                        }
                        // Status Logs setzen
                        if (SessionWrapper != null && SessionWrapper.CurrentMunicipalUser != null && request.CANTEEN_RequestRefundAllCharge_Status_ID != null)
                        {
                            CANTEEN_RequestRefundBalances_Status_Log? requestLogs = new CANTEEN_RequestRefundBalances_Status_Log();

                            requestLogs.ID = Guid.NewGuid();
                            requestLogs.ChangeDate = DateTime.Now;
                            requestLogs.AUTH_Users_ID = SessionWrapper.CurrentMunicipalUser.ID;
                            requestLogs.CANTEEN_RequestRefundBalances_ID = request.ID;
                            requestLogs.CANTEEN_RequestRefundBalances_Status_ID = request.CANTEEN_RequestRefundAllCharge_Status_ID.Value;

                            requestLogs = await CanteenProvider.SetRequestRefundBalanceStatusLog(requestLogs);

                            if (requestLogs != null)
                            {
                                foreach (LANG_Languages language in languages)
                                {
                                    CANTEEN_RequestRefundBalances_Status_Log_Extended? requestLogExtended = new CANTEEN_RequestRefundBalances_Status_Log_Extended();

                                    requestLogExtended.ID = Guid.NewGuid();
                                    requestLogExtended.CANTEEN_RequestRefundBalances_StatusLog_ID = requestLogs.ID;
                                    requestLogExtended.LANG_Languages_ID = language.ID;
                                    if (!string.IsNullOrEmpty(textItem.German) && language.ID == LanguageSettings.German)
                                    {
                                        requestLogExtended.Reason = ReplaceKeywords(textItem.German, request, status, previousStatus);
                                    }
                                    else if (!string.IsNullOrEmpty(textItem.Italian) && language.ID == LanguageSettings.Italian)
                                    {
                                        requestLogExtended.Reason = ReplaceKeywords(textItem.Italian, request, status, previousStatus);
                                    }
                                    else if (!string.IsNullOrEmpty(textItem.German))
                                    {
                                        requestLogExtended.Reason = ReplaceKeywords(textItem.German, request, status, previousStatus);
                                    }
                                    else if (!string.IsNullOrEmpty(textItem.Italian))
                                    {
                                        requestLogExtended.Reason = ReplaceKeywords(textItem.Italian, request, status, previousStatus);
                                    }

                                    requestLogExtended = await CanteenProvider.SetRequestRefundBalanceStatusLogExtended(requestLogExtended);
                                }
                            }

                            requestStatusLog = await GetCurrentStatusLog(request);
                        }

                        // Betrag buchen
                        if (status.ID == Canteen_Request_Status.Accepted && request.RequestAcceptedDate == null)
                        {
                            await CreateRefundBalancesMovement(request);
                        }
                        currentMovement = await GetCurrentMovement(request);
                        // Antwort-PDF erzeugen
                        FILE_FileInfo? fileDE = null;
                        FILE_FileInfo? fileIT = null;

                        if ((status.ID == Canteen_Request_Status.Accepted || status.ID == Canteen_Request_Status.Declined) && request.FILE_FileInfoID != null && municipality != null && TextProvider != null)
                        {
                            if (!string.IsNullOrEmpty(textItem.German))
                            {
                                fileDE = await CreateResponseFile(request, LanguageSettings.German, TextProvider.Get(status.TEXT_SystemTexts_Code, LanguageSettings.German));
                            }
                            if (!string.IsNullOrEmpty(textItem.Italian))
                            {
                                fileIT = await CreateResponseFile(request, LanguageSettings.Italian, TextProvider.Get(status.TEXT_SystemTexts_Code, LanguageSettings.Italian));
                            }
                        }
                        // Antrag auf Archiviert setzen
                        if (status.ID == Canteen_Request_Status.Accepted || status.ID == Canteen_Request_Status.Declined)
                        {
                            if (status.ID == Canteen_Request_Status.Accepted && request.RequestAcceptedDate == null)
                            {
                                request.RequestAcceptedDate = DateTime.Now;
                            }
                            request.ArchivedBool = true;
                            await CanteenProvider.SetRequestRefundBalance(request);
                            _requests = await GetRequests(_filter);
                        }
                        // Sende Rückmeldung an Bürger
                        if (request.AUTH_User_ID != null)
                        {
                            Guid userLanguageID = LanguageSettings.German;
                            string body = "";

                            if (AuthProvider != null)
                            {
                                AUTH_UserSettings? setting = await AuthProvider.GetSettings(request.AUTH_User_ID.Value);
                                if (setting != null && setting.LANG_Languages_ID == LanguageSettings.Italian)
                                {
                                    if (TextProvider != null && !string.IsNullOrEmpty(textItem.Italian))
                                    {
                                        body = await TextProvider.ReplaceGeneralKeyWords(request.AUTH_User_ID.Value, textItem.Italian);
                                    }
                                    body = ReplaceKeywords(body, request, status, previousStatus);
                                    userLanguageID = setting.LANG_Languages_ID.Value;
                                }
                                else
                                {
                                    if (TextProvider != null && !string.IsNullOrEmpty(textItem.German))
                                    {
                                        body = await TextProvider.ReplaceGeneralKeyWords(request.AUTH_User_ID.Value, textItem.German);
                                    }
                                    body = ReplaceKeywords(body, request, status, previousStatus);
                                }
                            }

                            if (MessageService != null && TextProvider != null && municipality != null)
                            {
                                MSG_Message? message = await MessageService.GetMessage(request.AUTH_User_ID.Value,
                                                                                       municipality.ID,
                                                                                       body,
                                                                                       TextProvider.Get("NOTIF_CANTEEN_REQUESTREFUNDBALANCES_STATUS_CHANGED_SHORTTEXT", userLanguageID),
                                                                                       TextProvider.Get("NOTIF_CANTEEN_REQUESTREFUNDBALANCES_STATUS_CHANGED_TITLE", userLanguageID),
                                                                                       MessageTypeSettings.MailPushList,
                                                                                       false,
                                                                                       null);
                                if (message != null && AuthProvider != null)
                                {
                                    string? link = null;
                                    List<FILE_FileInfo> attachmentList = new List<FILE_FileInfo>();
                                    AUTH_UserSettings? setting = await AuthProvider.GetSettings(request.AUTH_User_ID.Value);

                                    if (NavManager != null)
                                    {
                                        link = NavManager.BaseUri + "/Canteen/RequestRefundBalances/Details/" + request.ID;
                                    }
                                    //if (fileIT != null && setting != null && setting.LANG_Languages_ID == LanguageSettings.Italian)
                                    //{
                                    //    attachmentList.Add(fileIT);
                                    //}
                                    //else if (fileDE != null)
                                    //{
                                    //    attachmentList.Add(fileDE);
                                    //}

                                    await MessageService.SendMessage(message, link, attachmentList);
                                }
                            }
                        }
                        // Schreibe istanzierte Variablen in Klassen-Variablen
                        _currentRequest = request;
                        _currentStatusLogList = requestStatusLog;
                        _currentMovement = currentMovement;
                        StartStatus = request.CANTEEN_RequestRefundAllCharge_Status_ID;
                        _textItem = new TextItem();
                    }
                }
            }

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
            }
            StateHasChanged();
            return true;
        }
        private async Task<FILE_FileInfo?> CreateResponseFile(CANTEEN_RequestRefundBalances Request, Guid LANG_Language_ID, string FileName)
        {
            if (Request == null)
            {
                return null;
            }
            List<LANG_Languages> languages = new List<LANG_Languages>();
            LANG_Languages? language = null;
            if (LangProvider != null)
            {
                languages = await LangProvider.GetAll() ?? new List<LANG_Languages>();
            }
            language = languages.FirstOrDefault(p => p.ID == LANG_Language_ID);
            if (FileProvider != null && CanteenProvider != null && FormReportPrintHelper != null && language != null)
            {
                MemoryStream resultStream = new MemoryStream();
                MemoryStream? ApplicationPDFStream = CreatePDF(Request, language);
                MemoryStream? ResponsePDFStream = CreateResultPDF(Request, language);

                using (PdfDocument one = PdfReader.Open(ResponsePDFStream, PdfDocumentOpenMode.Import))
                {
                    using (PdfDocument two = PdfReader.Open(ApplicationPDFStream, PdfDocumentOpenMode.Import))
                    {
                        using (PdfDocument outPdf = new PdfDocument())
                        {
                            FormReportPrintHelper.CopyPages(one, outPdf);
                            FormReportPrintHelper.CopyPages(two, outPdf);

                            outPdf.Save(resultStream);
                        }
                    }
                }

                FILE_FileInfo fileInfo = new FILE_FileInfo();

                fileInfo.ID = Guid.NewGuid();
                fileInfo.AUTH_Users_ID = Request.AUTH_User_ID;
                fileInfo.CreationDate = DateTime.Now;
                fileInfo.FileName = FileName;
                fileInfo.FileExtension = ".pdf";
                fileInfo.Size = resultStream.Length;

                FILE_FileStorage fileStorage = new FILE_FileStorage();

                fileStorage.ID = Guid.NewGuid();
                fileStorage.FILE_FileInfo_ID = fileInfo.ID;
                fileStorage.FileImage = resultStream.ToArray();
                fileStorage.CreationDate = DateTime.Now;

                fileInfo.FILE_FileStorage = new List<FILE_FileStorage>() { fileStorage };

                await FileProvider.SetFileInfo(fileInfo);

                CANTEEN_RequestRefundBalances_Resource ressource = new CANTEEN_RequestRefundBalances_Resource();
                ressource.ID = Guid.NewGuid();
                ressource.CANTEEN_RequestRefundBalances_ID = Request.ID;
                ressource.CreationDate = DateTime.Now;
                ressource.FILE_FileInfo_ID = fileInfo.ID;
                ressource.UserUpload = false;

                await CanteenProvider.SetRequestRessource(ressource);

                return fileInfo;
            }

            return null;
        }
        private MemoryStream? CreatePDF(CANTEEN_RequestRefundBalances Request, LANG_Languages Language)
        {
            ReportPackager reportPackager = new ReportPackager();
            string reportFileName = "MensaRefundBalances" + Language.Code + ".trdp";
            string reportBasePath = @"D:\Comunix\Reports\" + reportFileName;

            if (NavManager != null && NavManager.BaseUri.Contains("localhost"))
            {
                reportBasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\" + reportFileName;
            }

            try
            {
                using (FileStream? sourceStream = System.IO.File.OpenRead(reportBasePath))
                {
                    Telerik.Reporting.Report? report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);

                    report.ReportParameters.Clear();

                    ReportParameter reportParameter = new ReportParameter();
                    reportParameter.Name = "RequestRefundBalanceID";
                    reportParameter.Text = Request.ID.ToString();
                    reportParameter.Value = Request.ID.ToString();
                    reportParameter.Type = Telerik.Reporting.ReportParameterType.String;
                    reportParameter.AllowBlank = false;
                    reportParameter.AllowNull = false;

                    report.ReportParameters.Add(reportParameter);

                    InstanceReportSource reportSource = new InstanceReportSource();
                    reportSource.ReportDocument = report;

                    ReportProcessor reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
                    System.Collections.Hashtable deviceInfo = new System.Collections.Hashtable();

                    deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

                    Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

                    return new MemoryStream(result.DocumentBytes);
                }
            }
            catch
            {
            }

            return null;
        }
        private MemoryStream? CreateResultPDF(CANTEEN_RequestRefundBalances Request, LANG_Languages Language)
        {
            ReportPackager reportPackager = new ReportPackager();
            string reportFileName = "MensaRefundBalances_ResponseTemplate.trdp";
            string reportBasePath = @"D:\Comunix\Reports\" + reportFileName;

            if (NavManager != null && NavManager.BaseUri.Contains("localhost"))
            {
                reportBasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\" + reportFileName;
            }

            try
            {
                using (FileStream? sourceStream = System.IO.File.OpenRead(reportBasePath))
                {
                    Telerik.Reporting.Report? report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);

                    report.ReportParameters.Clear();

                    ReportParameter reportParameter = new ReportParameter();
                    reportParameter.Name = "RequestRefundBalanceID";
                    reportParameter.Text = Request.ID.ToString();
                    reportParameter.Value = Request.ID.ToString();
                    reportParameter.Type = Telerik.Reporting.ReportParameterType.String;
                    reportParameter.AllowBlank = false;
                    reportParameter.AllowNull = false;

                    report.ReportParameters.Add(reportParameter);

                    reportParameter = new ReportParameter();
                    reportParameter.Name = "LanguageID";
                    reportParameter.Text = Language.ID.ToString();
                    reportParameter.Value = Language.ID.ToString();
                    reportParameter.Type = Telerik.Reporting.ReportParameterType.String;
                    reportParameter.AllowBlank = false;
                    reportParameter.AllowNull = false;

                    report.ReportParameters.Add(reportParameter);

                    InstanceReportSource reportSource = new InstanceReportSource();
                    reportSource.ReportDocument = report;

                    ReportProcessor reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
                    System.Collections.Hashtable deviceInfo = new System.Collections.Hashtable();

                    deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

                    Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

                    return new MemoryStream(result.DocumentBytes);
                }
            }
            catch
            {
            }

            return null;
        }

        private async Task<bool> CreateRefundBalancesMovement(CANTEEN_RequestRefundBalances Request)
        {
            if (TextProvider != null && CanteenProvider != null && Request.CANTEEN_RequestRefundAllCharge_Status_ID == Canteen_Request_Status.Accepted && Request.Fee > 0 && _currentMovement == null)
            {
                StringBuilder _progressivNumber = new StringBuilder();
                if (_currentMunicipality != null)
                {
                    _progressivNumber.Append(_currentMunicipality.MensaRefundPNPrefix);
                }
                if (!string.IsNullOrEmpty(_progressivNumber.ToString()) && !_progressivNumber.ToString().EndsWith("-"))
                {
                    _progressivNumber.Append("-");
                }
                _progressivNumber.Append(Request.ProgressivYear);
                if (!string.IsNullOrEmpty(_progressivNumber.ToString()) && !_progressivNumber.ToString().EndsWith("-"))
                {
                    _progressivNumber.Append("-");
                }
                _progressivNumber.Append(Request.ProgressivNumber);

                CANTEEN_Subscriber_Movements newPayment = new CANTEEN_Subscriber_Movements();
                newPayment.ID = Guid.NewGuid();
                newPayment.CANTEEN_Subscriber_ID = null;
                newPayment.Date = Request.PaymentRefundUntil;
                newPayment.Fee = Request.Fee * -1;
                newPayment.Description = TextProvider.Get("CANTEEN_REQUESTREFUNDBALANCES_MOVEMENTDESCRIPTION").Replace("{Fee}", string.Format("{0:C}", Request.Fee)).Replace("{RequestNumber}", _progressivNumber.ToString());
                newPayment.CANTEEN_Subscriber_Movement_Type_ID = Guid.Parse("276F6A9E-4685-4BF2-9485-E3A23EE6B210");
                newPayment.AUTH_User_ID = Request.AUTH_User_ID;
                newPayment.IsMunicipalInput = true;
                newPayment.CANTEEN_RequestRefundBalancesID = Request.ID;

                await CanteenProvider.SetSubscriberMovement(newPayment);
            }

            return false;
        }
        private void ShowModifyRefundBalanceMovementWindow()
        {
            _showModifyMovementModalWindow = true;
            StateHasChanged();
        }
        private async void CloseModifyRefundBalanceMovementWindow()
        {
            _currentMovement = await GetCurrentMovement(_currentRequest);
            _showModifyMovementModalWindow = false;
            StateHasChanged();
        }
        private void ShowChangeStatusWindow()
        {
            if (_currentRequest != null)
            {
                StartStatus = _currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID;
                CANTEEN_RequestRefundBalances_Status? _firstSelectableStatus = _statusList.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.StatusSelectable == true);
                if (_firstSelectableStatus != null)
                {
                    _currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID = _firstSelectableStatus.ID;
                }
                _textItem = new TextItem();

                _showModifyStatusModalWindow = true;
                StateHasChanged();
            }
        }
        private void CloseChangeStatusWindow()
        {
            _showModifyStatusModalWindow = false;
            if (_currentRequest != null)
            {
                _currentRequest.CANTEEN_RequestRefundAllCharge_Status_ID = StartStatus;
            }
            StateHasChanged();
        }
        private async Task ModifyDateRefundBalancesMovement(CANTEEN_Subscriber_Movements RequestMovement, CANTEEN_RequestRefundBalances Request)
        {
            DateTime? _movementDate = RequestMovement.Date;
            if (CanteenProvider != null && _movementDate != null && Request.RequestAcceptedDate != null && _movementDate.Value.Date >= Request.RequestAcceptedDate.Value.Date && _movementDate.Value.Date <= Request.RequestAcceptedDate.Value.Date.AddMonths(6))
            {
                CANTEEN_Subscriber_Movements? matchingMovement = await CanteenProvider.GetSubscriberMovementByRequestID(Request.ID);
                if (matchingMovement != null)
                {
                    matchingMovement.Date = _movementDate;
                    await CanteenProvider.SetSubscriberMovement(matchingMovement);
                    _currentMovement = await GetCurrentMovement(Request);
                    _showModifyMovementModalWindow = false;
                    StateHasChanged();
                }
            }
        }
        private string ReplaceKeywords(string Text, CANTEEN_RequestRefundBalances Request, CANTEEN_RequestRefundBalances_Status? Status, CANTEEN_RequestRefundBalances_Status? PreviousStatus)
        {
            if (Request != null)
            {
                StringBuilder _progressivNumber = new StringBuilder();
                if (_currentMunicipality != null)
                {
                    _progressivNumber.Append(_currentMunicipality.MensaRefundPNPrefix);
                }
                if (Request.ProgressivYear != null)
                {
                    if (!string.IsNullOrEmpty(_progressivNumber.ToString()) && !_progressivNumber.ToString().EndsWith("-"))
                    {
                        _progressivNumber.Append("-");
                    }
                    _progressivNumber.Append(Request.ProgressivYear.ToString());
                }
                if (Request.ProgressivNumber != null)
                {
                    if (!string.IsNullOrEmpty(_progressivNumber.ToString()) && !_progressivNumber.ToString().EndsWith("-"))
                    {
                        _progressivNumber.Append("-");
                    }
                    _progressivNumber.Append(Request.ProgressivNumber.ToString());
                }
                Text = Text.Replace("{Protokollnummer}", _progressivNumber.ToString());
                Text = Text.Replace("{Numero di protocollo}", _progressivNumber.ToString());

                if (TextProvider != null && Status != null)
                {
                    Text = Text.Replace("{Neuer Status}", TextProvider.Get(Status.TEXT_SystemTexts_Code));
                    Text = Text.Replace("{Nuovo stato}", TextProvider.Get(Status.TEXT_SystemTexts_Code));
                }
                if (TextProvider != null && PreviousStatus != null)
                {
                    Text = Text.Replace("{Bisheriger Status}", TextProvider.Get(PreviousStatus.TEXT_SystemTexts_Code));
                    Text = Text.Replace("{Stato precedente}", TextProvider.Get(PreviousStatus.TEXT_SystemTexts_Code));
                }
            }

            return Text;
        }

        private void BackToPreviousPage()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
            if (NavManager != null)
            {
                NavManager.NavigateTo("/Backend/Canteen/RequestRefundBalances/Administration");
            }
        }
        public void Dispose()
        {
            if (SessionWrapper != null)
            {
                SessionWrapper.OnAUTHMunicipalityIDChanged -= OnMunicipalityIDChanged;
            }
        }
    }
}
