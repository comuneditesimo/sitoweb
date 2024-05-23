using ICWebApp.Application.Settings;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models.ActionBar;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.Models.User;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using Telerik.Blazor;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Syncfusion.Blazor.Popups;
using ICWebApp.Domain.Enums.Homepage.Settings;

namespace ICWebApp.Components.Components.Canteen.Frontend
{
    public partial class Dashboard
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IMyCivisService MyCivisService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }

        [Inject] public SfDialogService Dialogs { get; set; }

        private decimal CurrentBalance = 0;
        private List<V_CANTEEN_Subscriber_Movements> LatestMovements = new List<V_CANTEEN_Subscriber_Movements>();
        private List<V_CANTEEN_Subscriber> Subscribers = new List<V_CANTEEN_Subscriber>();
        private V_CANTEEN_Schoolyear_Current? CurrentSchoolyear;
        private List<AUTH_MunicipalityApps>? AktiveApps = new List<AUTH_MunicipalityApps>();
        private List<ActionBarItem> ActionItems = new List<ActionBarItem>();
        private List<ServiceDataItem> SubscribersList = new List<ServiceDataItem>();
        private List<ServiceDataItem> RequestsRefundBalances = new List<ServiceDataItem>();
        private List<ServiceDataItem> POSCardOverview = new List<ServiceDataItem>();
        private V_CANTEEN_User? CurrentCanteenUser = null;
        private int ShowAmount = 5;
        private decimal OpenBalance = 0;

        protected override async Task OnInitializedAsync()
        {
            BusyIndicatorService.IsBusy = true;

            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_CANTEEN_SERVICE");

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Canteen);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageSubTitle = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageSubTitle = TextProvider.Get("MAINMENU_CANTEEN_SERVICE_DESCRIPTION");
                }
            }

            SessionWrapper.PageDescription = null;

            CrumbService.ClearBreadCrumb();

            if (MyCivisService.Enabled == true)
            {
                CrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
            }
            else
            {
                CrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
            }

            SessionWrapper.OnCurrentSubUserChanged += SessionWrapper_OnCurrentUserChanged;

            StateHasChanged();

            AktiveApps = await AuthProvider.GetMunicipalityApps();

            if (SessionWrapper != null && SessionWrapper.CurrentUser != null)
            {
                await LoadData();
            }

            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                SessionWrapper.AUTH_Municipality_ID = await SessionWrapper.GetMunicipalityID();

            }

            await CreateActionBar();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task CreateActionBar()
        {
            ActionItems.Clear();

            ActionItems.Add(new ActionBarItem()
            {
                SortOrder = 0,
                Action = InfoPage,
                Icon = "css/bootstrap-italia/svg/sprites.svg#it-info-circle",
                Title = TextProvider.Get("CANTEEN_DASHBOARD_INFOPAGE")
            });

            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null && SessionWrapper.CurrentUser != null)
            {
                bool _requestRefundBalancesAlreadyComitted = await CanteenProvider.UserHasOpenRequest(SessionWrapper.AUTH_Municipality_ID.Value, SessionWrapper.CurrentUser.ID);

                if (CurrentBalance > 0 && !_requestRefundBalancesAlreadyComitted)
                {
                    ActionItems.Add(new ActionBarItem()
                    {
                        SortOrder = 6,
                        Action = RequestRefundBalances,
                        Icon = "css/bootstrap-italia/svg/sprites.svg#it-restore",
                        Title = TextProvider.Get("CANTEEN_DASHBOARD_REQUEST_REST_BALANCE")
                    });
                }
            }

            ActionItems.Add(new ActionBarItem()
            {
                SortOrder = 7,
                Action = NavigateToTaxReportPage,
                Icon = "css/bootstrap-italia/svg/sprites.svg#it-inbox",
                Title = TextProvider.Get("CANTEEN_TAX_REPORTS")
            });

            StateHasChanged();
        }
        private void SessionWrapper_OnCurrentUserChanged()
        {
            if (SessionWrapper != null && SessionWrapper.CurrentUser != null)
            {
                BusyIndicatorService.IsBusy = true;

                if (MyCivisService.Enabled == true)
                {
                    NavManager.NavigateTo("/Canteen/MyCivis");
                }
                else
                {
                    NavManager.NavigateTo("/Canteen");
                }

                StateHasChanged();
            }
        }
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                EnviromentService.ScrollToTop();
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }
        private async Task<bool> LoadData()
        {
            CurrentCanteenUser = await CanteenProvider.GetVUser(SessionWrapper.CurrentUser.ID);
            CurrentSchoolyear = (await CanteenProvider.GetSchoolsyearsCurrent(SessionWrapper.AUTH_Municipality_ID.Value, true)).FirstOrDefault();
            CurrentBalance = CanteenProvider.GetUserBalance(SessionWrapper.CurrentUser.ID);
            OpenBalance = CanteenProvider.GetOpenPayent(SessionWrapper.CurrentUser.ID) * -1;

            await LoadMovementData();

            List<V_CANTEEN_RequestRefundBalances> _requests = await CanteenProvider.GetRequestRefundBalances(SessionWrapper.AUTH_Municipality_ID.Value, SessionWrapper.CurrentUser.ID, LangProvider.GetCurrentLanguageID());
            
            RequestsRefundBalances.Clear();
            
            foreach (V_CANTEEN_RequestRefundBalances _request in _requests)
            {
                ServiceDataItem? _serviceItem = new ServiceDataItem();
                _serviceItem.CreationDate = _request.Date;
                _serviceItem.Title = TextProvider.Get("CANTEEN_REQUESTREFUNDBALANCE_TITLE").Replace("{0}", string.Format("{0:C}", _request.Fee));
                _serviceItem.File_FileInfo_ID = _request.FILE_FileInfoID;
                _serviceItem.ProtocollNumber = _request.ProgressivNumberCombined;
                _serviceItem.Description = TextProvider.Get("CANTEEN_REQUESTREFUNDBALANCE_DESCRIPTION_REFUND_TEXT").Replace("{0}", string.Format("{0:C}", _request.Fee));
                _serviceItem.StatusIcon = _request.Status_Icon;
                _serviceItem.StatusText = _request.Status_Text;
                _serviceItem.ServiceItemID = _request.ID;
                _serviceItem.DetailAction = (async () => await GoToRequestRefundBalanceMensa(_request.ID));
                RequestsRefundBalances.Add(_serviceItem);
            }

            Subscribers = await CanteenProvider.GetVSubscribersByUserID(SessionWrapper.CurrentUser.ID, LangProvider.GetCurrentLanguageID());
            Subscribers = Subscribers.Where(a => a.Archived == null).ToList();

            SubscribersList.Clear();
            POSCardOverview.Clear();

            List<V_CANTEEN_Subscriber_Card> _cards = await CanteenProvider.GetAllCards(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

            foreach (var d in Subscribers)
            {
                var item = new ServiceDataItem()
                {
                    CreationDate = d.CreationDate,
                    File_FileInfo_ID = d.FILE_FileInfo_ID,
                    ProtocollNumber = d.ProgressivNumber,
                    StatusIcon = d.StatusIcon,
                    StatusText = d.StatusText,
                };

                if (d.InChange == true)
                {
                    item.Title = TextProvider.Get("CANTEEN_SUBSCRIPTION_EDIT_TITLE") + " - " + d.FirstName + " " + d.LastName;
                }
                else
                {
                    item.Title = d.FirstName + " " + d.LastName;
                }

                if (d.CANTEEN_Subscriber_Status_ID == CanteenStatus.Incomplete) //INCOMPLETE
                {
                    if (CurrentSchoolyear != null && CurrentSchoolyear.RegisterBeginDate <= DateTime.Now && CurrentSchoolyear.RegisterEndDate.Value.AddDays(1) >= DateTime.Now)
                    {
                        item.DetailAction = (() => EditSubscription(d));
                        item.DetailTextCode = "CANTEEN_DASHBOARD_COMPLETE";

                        if(d.InChange == true)
                        {
                            item.CancelAction = (() => CancelChangeRequest(d));
                            item.CancelTextCode = "CANTEEN_CANCEL_CHANGE_SUBSCRIPTION";
                        }
                    }
                }
                else if (d.CANTEEN_Subscriber_Status_ID == CanteenStatus.ToSign)
                {
                    if (CurrentSchoolyear != null && CurrentSchoolyear.RegisterBeginDate <= DateTime.Now && CurrentSchoolyear.RegisterEndDate.Value.AddDays(1) >= DateTime.Now)
                    {
                        item.DetailAction = (() => SignSubscription(d));
                        item.DetailTextCode = "CANTEEN_DASHBOARD_SIGN";
                    }
                }
                else if (d.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted || d.CANTEEN_Subscriber_Status_ID == CanteenStatus.Comitted) //ACCEPTED
                {
                    item.DetailAction = (() => GoToSubscriber(d.ID));
                    
                    if(d.FILE_FileInfo_ID != null && d.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted && d.SuccessorSubscriptionID == null)
                    {
                        item.CancelAction = (() => NewVersionSubscription(d));
                        item.CancelTextCode = "CANTEEN_CHANGE_SUBSCRIPTION";
                    }
                }

                CANTEEN_Configuration? conf = await CanteenProvider.GetConfiguration(SessionWrapper.CurrentUser.AUTH_Municipality_ID.Value);

                if ((d.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted || d.CANTEEN_Subscriber_Status_ID == CanteenStatus.Comitted) && conf != null && conf.PosMode == true)
                {
                    List<V_CANTEEN_Subscriber_Card> _cardsOfSubscriber = _cards.Where(p => p.Taxnumber.Trim().ToLower() == d.TaxNumber.Trim().ToLower()).OrderByDescending(p => p.CreationDate).ToList();

                    if (_cardsOfSubscriber != null && _cardsOfSubscriber.Any())
                    {
                        foreach (V_CANTEEN_Subscriber_Card _card in _cardsOfSubscriber)
                        {
                            if (POSCardOverview.Select(p => p.ServiceItemID).Contains(_card.ID))
                            {
                                continue;
                            }
                            ServiceDataItem? _serviceItem = new ServiceDataItem();
                            _serviceItem.CreationDate = _card.CreationDate;
                            _serviceItem.Title = TextProvider.Get("CANTEEN_POSCARD_SERVICEITEMTEXT").Replace("{0}", _card.Lastname).Replace("{1}", _card.Firstname);
                            _serviceItem.File_FileInfo_ID = null;
                            _serviceItem.TaxNumber = _card.Taxnumber;
                            _serviceItem.StatusIcon = _card.StatusIcon;
                            _serviceItem.StatusText = _card.Status;
                            _serviceItem.ServiceItemID = _card.ID;
                            if (await CheckIfSubscriberCanRequestNewCard(d))
                            {
                                _serviceItem.CanteenRequestNewCardTextCode = "CANTEEN_REQUEST_NEW_CARD_BUTTON";
                                _serviceItem.CanteenRequestNewCardAction = () => NavToRequestNewCard(d.TaxNumber);
                            }
                            if (_card.CANTEEN_Subscriber_Card_Status_ID != CanteenCardStatus.Disabled)
                            {
                                _serviceItem.CancelTextCode = "CANTEEN_DOWNLOAD_DIGITAL_POSCARD";
                                _serviceItem.CancelAction = async () => await DownloadDigitalCard(_card);
                            }
                            POSCardOverview.Add(_serviceItem);
                        }
                    }
                }

                SubscribersList.Add(item);
            }

            return true;
        }

        private void NavToRequestNewCard(string subTaxnumber)
        {
            if (MyCivisService.Enabled)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/RequestNewPOSCard/" + subTaxnumber);
            }
            else
            {
                NavManager.NavigateTo("/Canteen/RequestNewPOSCard/" + subTaxnumber);
            }
        }
        private async Task<bool> CheckIfSubscriberCanRequestNewCard(V_CANTEEN_Subscriber sub)
        {
            if (SessionWrapper.CurrentUser == null || SessionWrapper.CurrentUser.AUTH_Municipality_ID == null)
                return false;
            var conf = await CanteenProvider.GetConfiguration(SessionWrapper.CurrentUser.AUTH_Municipality_ID.Value);
            if(conf == null || conf.PosMode == false)
                return false;
            if (sub.CANTEEN_Subscriber_Status_ID != CanteenStatus.Accepted)
                return false;
            var card = await CanteenProvider.GetCurrentSubscriberCard(sub.TaxNumber);
            if (card == null || (card.CANTEEN_Subscriber_Card_Status_ID != CanteenCardStatus.Finished && card.CANTEEN_Subscriber_Card_Status_ID != CanteenCardStatus.Disabled))
                return false;
            return true;
        }
        private async Task LoadMovementData()
        {
            LatestMovements = await CanteenProvider.GetPastVSubscriberMovementsByUser(SessionWrapper.CurrentUser.ID, ShowAmount);
        }
        private void GoToSubscriber(Guid SubscriberID)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/Detail/Subscribe/" + SubscriberID);
            }
            else
            {
                NavManager.NavigateTo("/Canteen/Detail/Subscribe/" + SubscriberID);
            }
        }
        private async void RechargeBalance()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/RechargeAmount");
            }
            else
            {
                NavManager.NavigateTo("/Canteen/RechargeAmount");
            }
        }
        private async void RegisterAbsence()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/Absence");
            }
            else
            {
                NavManager.NavigateTo("/Canteen/Absence");
            }
        }
        private async void RequestRefundBalances()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/RequestRefundBalances");
            }
            else
            {
                NavManager.NavigateTo("/Canteen/RequestRefundBalances");
            }
        }
        private async void InfoPage()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivisInfo");
            }
            else
            {
                NavManager.NavigateTo("/Canteen/info");
            }
        }
        private async Task GoToRequestRefundBalanceMensa(Guid RequestID)
        {
            CANTEEN_RequestRefundBalances? _request = await CanteenProvider.GetRequestRefundBalance(RequestID.ToString());

            if (_request != null)
            {
                if (MyCivisService.Enabled == true && _request.SignedDate == null)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Canteen/MyCivis/SignRefundBalances/" + _request.ID);
                    StateHasChanged();
                }
                else if (MyCivisService.Enabled == true && _request.SignedDate != null)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Canteen/MyCivis/RequestRefundBalances/Details/" + _request.ID);
                    StateHasChanged();
                }
                else if (MyCivisService.Enabled == false && _request.SignedDate == null)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Canteen/SignRefundBalances/" + _request.ID);
                    StateHasChanged();
                }
                else
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Canteen/RequestRefundBalances/Details/" + _request.ID);
                    StateHasChanged();
                }
            }
        }
        private void CreateSubscription()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/Subscribe");
            }
            else
            {
                NavManager.NavigateTo("/Canteen/Subscribe");
            }
        } 
        private async void SignSubscription(V_CANTEEN_Subscriber? sub)
        {
            if (sub != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();

                if (MyCivisService.Enabled == true)
                {
                    NavManager.NavigateTo("/Canteen/MyCivis/Sign" + sub.SubscriptionFamilyID);
                }
                else
                {
                    NavManager.NavigateTo("/Canteen/Sign/" + sub.SubscriptionFamilyID);
                }
            }
        }
        private async void NewVersionSubscription(V_CANTEEN_Subscriber? sub)
        {

            if (!await Dialogs.ConfirmAsync(TextProvider.Get("CANTEEN_CREATE_NEW_VERSION_OF_SUBSCRIPTION_ARE_YOU_SURE"),
                    TextProvider.Get("INFORMATION")))
            {
                Console.WriteLine("Action cancelled");
            }
            else
            {
                if (sub != null && sub.ID != null)
                {
                    var newSub = await CanteenProvider.CloneAndArchiveSubscriber(sub.ID);

                    if (newSub != null)
                    {
                        BusyIndicatorService.IsBusy = true;

                        if (MyCivisService.Enabled == true)
                        {
                            NavManager.NavigateTo("/Canteen/MyCivis/Subscribe/" + newSub.SubscriptionFamilyID + "/1");
                        }
                        else
                        {
                            NavManager.NavigateTo("/Canteen/Subscribe/" + newSub.SubscriptionFamilyID + "/1");
                        }

                        StateHasChanged();
                    }
                }
            }

            StateHasChanged();
        }
        private async void CancelChangeRequest(V_CANTEEN_Subscriber? sub)
        {
            if (!await Dialogs.ConfirmAsync(TextProvider.Get("CANTEEN_CANCEL_CHANGE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
            {
                return;
            }

            if (sub == null || sub.ID == null)
            {
                return;
            }

            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            var locSubscriber = await CanteenProvider.GetSubscriberWithoutInclude(sub.ID);

            if (locSubscriber != null)
            {
                if (locSubscriber.PreviousSubscriptionID != null)
                {
                    var previousSubscriber = await CanteenProvider.GetSubscriberWithoutInclude(locSubscriber.PreviousSubscriptionID.Value);

                    if (previousSubscriber != null)
                    {
                        previousSubscriber.SuccessorSubscriptionID = null;

                        await CanteenProvider.SetSubscriber(previousSubscriber);
                    }
                }

                await CanteenProvider.RemoveSubscriber(locSubscriber.ID, true);
            }

            await LoadData();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
        }
        private void EditSubscription(V_CANTEEN_Subscriber? sub)
        {
            if (sub != null)
            {
                BusyIndicatorService.IsBusy = true;

                if (sub.PreviousSubscriptionID != null)
                {
                    if (MyCivisService.Enabled == true)
                    {
                        NavManager.NavigateTo("/Canteen/MyCivis/Subscribe/" + sub.SubscriptionFamilyID + "/1");
                    }
                    else
                    {
                        NavManager.NavigateTo("/Canteen/Subscribe/" + sub.SubscriptionFamilyID + "/1");
                    }
                }
                else
                {
                    if (MyCivisService.Enabled == true)
                    {
                        NavManager.NavigateTo("/Canteen/MyCivis/Subscribe/" + sub.SubscriptionFamilyID);
                    }
                    else
                    {
                        NavManager.NavigateTo("/Canteen/Subscribe/" + sub.SubscriptionFamilyID);
                    }
                }

                StateHasChanged();
            }
        }
        private void NavigateToTaxReportPage()
        {
            BusyIndicatorService.IsBusy = true;

            if (MyCivisService.Enabled)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/TaxReports");
            }
            else
            {
                NavManager.NavigateTo("/Canteen/TaxReports");
            }

            StateHasChanged();
        }
        private async void IncreaseShowAmount()
        {
            ShowAmount += 5;
            await LoadMovementData();
            StateHasChanged();
        }
        private async Task<bool> DownloadDigitalCard(V_CANTEEN_Subscriber_Card card)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            var reportPackager = new ReportPackager();
            var reportSource = new InstanceReportSource();

            string reportFileName = "PosCardPdf_DE.trdp";
            if (LangProvider.GetCurrentLanguageID() == LanguageSettings.Italian)
                reportFileName = "PosCardPdf_IT.trdp";

            var basePath = @"D:\Comunix\Reports\" + reportFileName;

            using (var sourceStream = System.IO.File.OpenRead(basePath))
            {
                var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
                report.ReportParameters.Clear();
                report.ReportParameters.Add(new ReportParameter("CardCode", ReportParameterType.String,
                    card.CardID));
                report.ReportParameters.Add(new ReportParameter("MunicipalityName", ReportParameterType.String,
                    card.MunicipalityText));
                report.ReportParameters.Add(new ReportParameter("Firstname", ReportParameterType.String,
                    card.Firstname));
                report.ReportParameters.Add(new ReportParameter("Lastname", ReportParameterType.String,
                    card.Lastname));
                report.ReportParameters.Add(new ReportParameter("Birthday", ReportParameterType.String,
                    card.DateOfBirth!.Value.ToString("dd.MM.yyyy")));
                report.ReportParameters.Add(new ReportParameter("MunicipalityUrl", ReportParameterType.String,
                    card.MunicipalityPath));
                report.ReportParameters.Add(new ReportParameter("TitleText", ReportParameterType.String,
                    "Überschrift"));
                report.ReportParameters.Add(new ReportParameter("DescriptionText", ReportParameterType.String,
                    "Hier Beschreibung"));
                

                reportSource.ReportDocument = report;

                var reportProcessor = new ReportProcessor();
                var deviceInfo = new System.Collections.Hashtable();

                deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

                var result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);
                var ms = new MemoryStream(result.DocumentBytes);
                ms.Position = 0;
                var file = ms.ToArray();
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
                await EnviromentService.DownloadFile(file, TextProvider.Get("CANTEEN_CARD") + ".pdf", true);
            }
            return true;
        }
    }
}
