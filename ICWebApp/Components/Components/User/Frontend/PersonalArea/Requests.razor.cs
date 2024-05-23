using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.Models.User;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using Syncfusion.Blazor.Popups;


namespace ICWebApp.Components.Components.User.Frontend.PersonalArea
{
    public partial class Requests
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IFORMApplicationProvider FormApplicationProvider { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBookingService BookingService { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IORGProvider OrgProvider { get; set; }
        [Inject] IMailerService MailerService { get; set; }
        [Inject] ISMSService SMSService { get; set; }
        [Inject] IMessageService MessageService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        [Inject] public SfDialogService Dialogs { get; set; }

        private List<ServiceDataItem> ApplicationList = new List<ServiceDataItem>();
        private List<ServiceDataItem> ManteinanceList = new List<ServiceDataItem>();
        private List<ServiceDataItem> MensaList = new List<ServiceDataItem>();
        private List<ServiceDataItem> BookingList = new List<ServiceDataItem>();
        private List<ServiceDataItem> OrganisationList = new List<ServiceDataItem>();
        private List<ServiceDataItem> PersonRequestList = new List<ServiceDataItem>();
        private List<ServiceDataItem> RequestList = new List<ServiceDataItem>();
        private bool ShowBankDataWindow = false;
        private ROOM_BookingGroup? CurrentBooking;
        private bool IsBusy = true;

        protected override async void OnParametersSet()
        {
            IsBusy = true;
            StateHasChanged();

            AnchorService.ClearAnchors();

            await GetApplicationData();
            await GetManteinanceData();
            await GetMensaData();
            await GetBookingData();
            await GetOrganisationData();
            await GetRequestData();

            IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private async Task GetApplicationData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                ApplicationList.Clear();

                var data = await FormApplicationProvider.GetApplicationsPersonalArea(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), LangProvider.GetCurrentLanguageID(), FORMCategories.Applications);

                foreach (var d in data)
                {
                    ApplicationList.Add(new ServiceDataItem()
                    {
                        CreationDate = d.SubmitAt ?? d.CreatedAt,
                        File_FileInfo_ID = d.FILE_Fileinfo_ID,
                        LastChangeDate = d.StatusChangeDate,
                        ProtocollNumber = d.FullProgressivNumber,
                        StatusIcon = d.StatusIcon,
                        StatusText = d.Status,
                        Title = d.FormName,
                        ServiceItemID = d.ID,
                        HasToPay = d.FORM_Application_Status_ID == FORMStatus.ToPay || d.OpenPayments > 0,
                        DetailAction = (() => GoToApplication(d.ID))
                    });
                }
            }

            return;
        }
        private async Task GetManteinanceData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                ManteinanceList.Clear();

                var data = await FormApplicationProvider.GetMantainancesPersonalArea(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), LangProvider.GetCurrentLanguageID(), FORMCategories.Maintenance);

                foreach (var d in data)
                {
                    ManteinanceList.Add(new ServiceDataItem()
                    {
                        CreationDate = d.SubmitAt ?? d.CreatedAt,
                        File_FileInfo_ID = d.FILE_Fileinfo_ID,
                        LastChangeDate = d.StatusChangeDate,
                        ProtocollNumber = d.FullProgressivNumber,
                        StatusIcon = d.StatusIcon,
                        StatusText = d.Status,
                        Title = d.Mantainance_Title,
                        Description = d.FormName,
                        ServiceItemID = d.ID,
                        HasToPay = d.FORM_Application_Status_ID == FORMStatus.ToPay || d.OpenPayments > 0,
                        DetailAction = (() => GoToApplication(d.ID))
                    });
                }
            }

            return;
        }
        private async Task GetMensaData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                MensaList.Clear();

                Guid _language = LanguageSettings.German;

                if (LangProvider != null)
                {
                    LANG_Languages? _actualLanguage = LangProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

                    if (_actualLanguage != null)
                    {
                        _language = _actualLanguage.ID;
                    }
                }

                List<V_CANTEEN_RequestRefundBalances> _requests = await CanteenProvider.GetRequestRefundBalances(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), _language);

                foreach (V_CANTEEN_RequestRefundBalances _request in _requests)
                {
                    MensaList.Add(new ServiceDataItem()
                    {
                        CreationDate = _request.Date,
                        Title = TextProvider.Get("MYSERVICE_CANTEEN_REQUESTREFUNDBALANCE_TITLE").Replace("{0}", string.Format("{0:C}", _request.Fee)),
                        File_FileInfo_ID = _request.FILE_FileInfoID,
                        ProtocollNumber = _request.ProgressivNumberCombined,
                        StatusIcon = _request.Status_Icon,
                        StatusText = _request.Status_Text,
                        ServiceItemID = _request.ID,
                        DetailAction = (async () => await GoToRequestRefundBalanceMensa(_request.ID))
                    });
                }

                if (SessionWrapper.CurrentUser != null)
                {
                    var subscribers = await CanteenProvider.GetVSubscribersByUserID(SessionWrapper.CurrentUser.ID, LangProvider.GetCurrentLanguageID());

                    foreach (var sub in subscribers)
                    {
                        MensaList.Add(new ServiceDataItem()
                        {
                            CreationDate = sub.CreationDate,
                            Title = TextProvider.Get("CANTEEN_POSCARD_SERVICEITEMTEXT").Replace("{0}", sub.LastName).Replace("{1}", sub.FirstName),
                            ProtocollNumber = sub.ProgressivNumber,
                            StatusIcon = sub.StatusIcon,
                            StatusText = sub.StatusText,
                            ServiceItemID = sub.ID,
                            DetailAction = GoToCanteenDashboard
                        });
                    }
                }
            }

            return;
        }
        private async Task GetBookingData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                BookingList.Clear();

                await RoomProvider.VerifyBookingsState(SessionWrapper.CurrentUser.ID, NavManager.BaseUri);

                var Bookings = await RoomProvider.GetBookingGroupByUser(SessionWrapper.CurrentUser.ID);
                var BackendBookings = await RoomProvider.GetBookingGroupBackendByUser(SessionWrapper.CurrentUser.ID);

                if (SessionWrapper.CurrentSubstituteUser != null)
                {
                    Bookings = await RoomProvider.GetBookingGroupByUser(SessionWrapper.CurrentSubstituteUser.ID);
                    BackendBookings = await RoomProvider.GetBookingGroupBackendByUser(SessionWrapper.CurrentSubstituteUser.ID);
                }

                foreach (var booking in Bookings)
                {
                    var newItem = new ServiceDataItem()
                    {
                        CreationDate = booking.SubmitAt ?? booking.CreationDate,
                        Title = booking.Title,
                        File_FileInfo_ID = booking.FILE_FileInfo_ID,
                        ProtocollNumber = booking.ProgressivNumberCombined,
                        StatusIcon = booking.IconCSS,
                        StatusText = booking.Status,
                        Days = booking.Days,
                        Rooms = booking.Rooms,
                        HasToPay = booking.ROOM_BookingStatus_ID == ROOMStatus.ToPay
                    };

                    if (booking.ROOM_BookingStatus_ID == ROOMStatus.ToPay)
                    {
                        newItem.DetailAction = (() => Booking_GoToPayment(booking.ID));
                    }
                    else if (booking.ROOM_BookingStatus_ID == ROOMStatus.ToSign)
                    {
                        newItem.DetailAction = (() => Booking_GoToSigning(booking.ID));
                    }
                    else if (booking.ROOM_BookingStatus_ID == ROOMStatus.Cancelled || booking.ROOM_BookingStatus_ID == ROOMStatus.CancelledByCitizen)
                    {
                        newItem.StatusCSS = "booking-canceled";
                    }
                    else
                    {
                        newItem.DetailAction = (() => Booking_ShowDetails(booking.ID));
                    }

                    if ((booking.ROOM_BookingStatus_ID == ROOMStatus.Accepted ||
                        booking.ROOM_BookingStatus_ID == ROOMStatus.AcceptedWithChanges ||
                        booking.ROOM_BookingStatus_ID == ROOMStatus.Comitted) &&
                        booking.StartDate > DateTime.Now)
                    {
                        newItem.CancelAction = (() => CancelBooking(booking));
                    }


                    BookingList.Add(newItem);
                }

                foreach (var booking in BackendBookings)
                {
                    var newItem = new ServiceDataItem()
                    {
                        CreationDate = booking.CreatedAt,
                        Title = booking.Title,
                        StatusIcon = booking.IconCSS,
                        StatusText = booking.Status,
                        Days = booking.Days,
                        Rooms = booking.Rooms,
                        HasToPay = booking.ROOM_BookingStatus_ID == ROOMStatus.ToPay,
                        Description = TextProvider.Get("FRONTEND_BOOKING_BACKEND_TEXT")
                    };

                    BookingList.Add(newItem);
                }
            }

            return;
        }
        private async Task GetOrganisationData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                OrganisationList.Clear();

                var Items = await OrgProvider.GetRequestList(SessionWrapper.CurrentUser.ID);

                if (Items != null)
                {

                    foreach (var item in Items)
                    {
                        var newItem = new ServiceDataItem()
                        {
                            CreationDate = item.SubmitAt ?? item.CreationDate,
                            Title = item.Firstname + " " + item.Lastname,
                            Description = item.CompanyType,
                            File_FileInfo_ID = item.FILE_Fileinfo_ID,
                            ProtocollNumber = item.ProgressivNumber,
                            StatusIcon = item.StatusIcon,
                            StatusText = item.Status,
                            ServiceItemID = item.ID,
                            DetailAction = (() => Organization_GoToApplication(item.ID)),
                            DetailTextCode = await Organization_ActionButtonText(item.ID)
                        };

                        OrganisationList.Add(newItem);
                    }
                }
            }

            return;
        }
        private async Task GetRequestData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && SessionWrapper.CurrentUser != null)
            {
                PersonRequestList.Clear();
                RequestList.Clear();

                var requests = await HomeProvider.GetPersonUserRequest(SessionWrapper.CurrentUser.ID);
                var requestsPerson = await HomeProvider.GetUserRequest(SessionWrapper.CurrentUser.ID);

                if (requests != null)
                {
                    foreach (var item in requests)
                    {
                        var newItem = new ServiceDataItem()
                        {
                            CreationDate = item.CreationDate,
                            Title = item.Person_Fullname,
                            Description = item.Address,
                            ServiceItemID = item.ID,
                            CancelAction = (() => CancelPersonRequest(item)),
                            CancelTextCode = "HOME_FRONTEND_CANCEL_REQUEST"
                        };

                        PersonRequestList.Add(newItem);
                    }
                }

                if (requestsPerson != null)
                {
                    foreach (var item in requestsPerson)
                    {
                        var newItem = new ServiceDataItem()
                        {
                            CreationDate = item.CreationDate,
                            Title = item.Authority,
                            Description = item.Reason,
                            ServiceItemID = item.ID,
                            CancelAction = (() => CancelRequest(item)),
                            CancelTextCode = "HOME_FRONTEND_CANCEL_REQUEST"
                        };

                        RequestList.Add(newItem);
                    }
                }
            }

            return;
        }
        private async void CancelPersonRequest(V_HOME_Person_Request item)
        {
            if (item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("HOME_FRONTEND_CANCEL_REQUEST_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;


                if (!string.IsNullOrEmpty(item.EMail))
                {
                    SendPersonCancelMail(item.EMail, item);
                }
                if (!string.IsNullOrEmpty(item.Phone))
                {
                    SendPersonCancelSms(item.Phone, item);
                }

                await SendPersonMessage(item);

                await HomeProvider.RemovePersonRequest(item.ID);

                await GetRequestData();
                StateHasChanged();
            }
        }
        private async void CancelRequest(V_HOME_Request item)
        {
            if (item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("HOME_FRONTEND_CANCEL_REQUEST_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;


                if (!string.IsNullOrEmpty(item.EMail))
                {
                    SendCancelMail(item.EMail, item);
                }
                if (!string.IsNullOrEmpty(item.Phone))
                {
                    SendCancelSms(item.Phone, item);
                }

                await SendAuthorityMessages(item);

                await HomeProvider.RemoveRequest(item.ID);


                await GetRequestData();
            }
        }
        private async Task<bool> SendPersonMessage(V_HOME_Person_Request item)
        {
            MSG_Message msg = new MSG_Message();

            msg.Subject = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CANCEL_TITLE");
            msg.Messagetext = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CANCEL_CONTENT");

            if (item.DateFrom != null && item.DateTo != null)
            {
                msg.Messagetext = msg.Messagetext.Replace("{Firstname}", item.Firstname);
                msg.Messagetext = msg.Messagetext.Replace("{Lastname}", item.Lastname);
                msg.Messagetext = msg.Messagetext.Replace("{Fiscalcode}", item.Fiscalnumber);
                        
                msg.Messagetext = msg.Messagetext.Replace("{EMail}", item.EMail);
                msg.Messagetext = msg.Messagetext.Replace("{Phone}", item.Phone);
                        
                msg.Messagetext = msg.Messagetext.Replace("{Person}", item.Person_Fullname);
                        
                msg.Messagetext = msg.Messagetext.Replace("{Address}", item.Address);
                msg.Messagetext = msg.Messagetext.Replace("{Date}", item.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + item.DateTo.Value.ToString("HH:mm"));
                        
                msg.Messagetext = msg.Messagetext.Replace("{Details}", item.Details);
            }

            await MessageService.SendMessage(msg);

            return true;
        }
        private async Task<bool> SendAuthorityMessages(V_HOME_Request item)
        {
            if (item.DateFrom != null && item.DateTo != null && item.AUTH_Authority_ID != null)
            {
                MSG_Message msg = new MSG_Message();

                msg.Subject = TextProvider.Get("HOME_REQUEST_MAIL_CANCEL_TITLE");
                msg.Messagetext = TextProvider.Get("HOME_REQUEST_MAIL_CANCEL_CONTENT");

                msg.Messagetext = msg.Messagetext.Replace("{Firstname}", item.Firstname);
                msg.Messagetext = msg.Messagetext.Replace("{Lastname}", item.Lastname);
                msg.Messagetext = msg.Messagetext.Replace("{Fiscalcode}", item.Fiscalnumber);

                msg.Messagetext = msg.Messagetext.Replace("{EMail}", item.EMail);
                msg.Messagetext = msg.Messagetext.Replace("{Phone}", item.Phone);

                msg.Messagetext = msg.Messagetext.Replace("{Authority}", item.Authority);
                msg.Messagetext = msg.Messagetext.Replace("{Address}", item.Address);
                msg.Messagetext = msg.Messagetext.Replace("{Date}", item.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + item.DateTo.Value.ToString("HH:mm"));

                msg.Messagetext = msg.Messagetext.Replace("{Reason}", item.Reason);

                msg.Messagetext = msg.Messagetext.Replace("{Details}", item.Details);

                await MessageService.SendMessageToAuthority(item.AUTH_Authority_ID.Value, msg);
            }

            return true;
        }
        private async void SendPersonCancelMail(string EMail, V_HOME_Person_Request request)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CANCEL_TITLE");

            mail.Body = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CANCEL_CONTENT");

            if (request != null && request.DateFrom != null && request.DateTo != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", request.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", request.Lastname);
                mail.Body = mail.Body.Replace("{Fiscalcode}", request.Fiscalnumber);

                mail.Body = mail.Body.Replace("{EMail}", request.EMail);
                mail.Body = mail.Body.Replace("{Phone}", request.Phone);

                mail.Body = mail.Body.Replace("{Person}", request.Person_Fullname);                

                mail.Body = mail.Body.Replace("{Address}", request.Address);
                mail.Body = mail.Body.Replace("{Date}", request.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + request.DateTo.Value.ToString("HH:mm"));

                mail.Body = mail.Body.Replace("{Details}", request.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await MailerService.SendMail(mail, null, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendPersonCancelSms(string Phone, V_HOME_Person_Request request)
        {
            MSG_SMS sms = new MSG_SMS();

            sms.PhoneNumber = Phone;

            sms.Message = TextProvider.Get("HOME_REQUEST_PERSON_SMS_CANCEL_CONTENT");

            if (request != null && request.DateFrom != null && request.DateTo != null)
            {
                sms.Message = sms.Message.Replace("{Firstname}", request.Firstname);
                sms.Message = sms.Message.Replace("{Lastname}", request.Lastname);
                sms.Message = sms.Message.Replace("{Fiscalcode}", request.Fiscalnumber);

                sms.Message = sms.Message.Replace("{EMail}", request.EMail);
                sms.Message = sms.Message.Replace("{Phone}", request.Phone);

                sms.Message = sms.Message.Replace("{Person}", request.Person_Fullname);                

                sms.Message = sms.Message.Replace("{Address}", request.Address);
                sms.Message = sms.Message.Replace("{Date}", request.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + request.DateTo.Value.ToString("HH:mm"));

                sms.Message = sms.Message.Replace("{Details}", request.Details);
            }



            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await SMSService.SendSMS(sms, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendCancelMail(string EMail, V_HOME_Request request)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_REQUEST_MAIL_CANCEL_TITLE");

            mail.Body = TextProvider.Get("HOME_REQUEST_MAIL_CANCEL_CONTENT");

            if (request != null && request.DateFrom != null && request.DateTo != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", request.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", request.Lastname);
                mail.Body = mail.Body.Replace("{Fiscalcode}", request.Fiscalnumber);

                mail.Body = mail.Body.Replace("{EMail}", request.EMail);
                mail.Body = mail.Body.Replace("{Phone}", request.Phone);

                mail.Body = mail.Body.Replace("{Authority}", request.Authority);                

                mail.Body = mail.Body.Replace("{Address}", request.Address);
                mail.Body = mail.Body.Replace("{Date}", request.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + request.DateTo.Value.ToString("HH:mm"));

                mail.Body = mail.Body.Replace("{Reason}", request.Reason);

                mail.Body = mail.Body.Replace("{Details}", request.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await MailerService.SendMail(mail, null, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendCancelSms(string Phone, V_HOME_Request request)
        {
            MSG_SMS sms = new MSG_SMS();

            sms.PhoneNumber = Phone;

            sms.Message = TextProvider.Get("HOME_REQUEST_SMS_CANCEL_CONTENT");

            if (request != null && request.DateFrom != null && request.DateTo != null)
            {
                sms.Message = sms.Message.Replace("{Firstname}", request.Firstname);
                sms.Message = sms.Message.Replace("{Lastname}", request.Lastname);
                sms.Message = sms.Message.Replace("{Fiscalcode}", request.Fiscalnumber);

                sms.Message = sms.Message.Replace("{EMail}", request.EMail);
                sms.Message = sms.Message.Replace("{Phone}", request.Phone);

                sms.Message = sms.Message.Replace("{Authority}", request.Authority);                

                sms.Message = sms.Message.Replace("{Address}", request.Address);
                sms.Message = sms.Message.Replace("{Date}", request.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + request.DateTo.Value.ToString("HH:mm"));

                sms.Message = sms.Message.Replace("{Reason}", request.Reason);                

                sms.Message = sms.Message.Replace("{Details}", request.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await SMSService.SendSMS(sms, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private Guid GetCurrentUserID()
        {
            Guid CurrentUserID = SessionWrapper.CurrentUser.ID;

            if (SessionWrapper.CurrentSubstituteUser != null)
            {
                CurrentUserID = SessionWrapper.CurrentSubstituteUser.ID;
            }

            return CurrentUserID;
        }
        private async void CancelBooking(V_ROOM_Booking_Group Item)
        {
            if ((Item.ROOM_BookingStatus_ID == ROOMStatus.Accepted || Item.ROOM_BookingStatus_ID == ROOMStatus.AcceptedWithChanges || Item.ROOM_BookingStatus_ID == ROOMStatus.Comitted) && Item.StartDate > DateTime.Now)
            {
                var dbBooking = await RoomProvider.GetBookingGroup(Item.ID);

                if (dbBooking == null)
                {
                    return;
                }

                var paymentTransactions = await RoomProvider.GetBookingTransactionList(dbBooking.ID);

                if (paymentTransactions != null && paymentTransactions.Count() > 0)
                {
                    CurrentBooking = dbBooking;
                    ShowBankDataWindow = true;
                    StateHasChanged();

                    return;
                }


                if (!await Dialogs.ConfirmAsync(TextProvider.Get("ROOM_BOOKING_CANCEL_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                await RoomProvider.SetBookingCanceled(dbBooking.ID, NavManager.BaseUri);

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var dbBookingGroup = await RoomProvider.GetBookingGroup(dbBooking.ID);

                    if (dbBookingGroup != null && dbBookingGroup.ROOM_Booking_Type_ID != ROOMBookingType.Blocked)
                    {
                        BookingService.SendMessagesToContacts(SessionWrapper.AUTH_Municipality_ID.Value, dbBookingGroup.ID, dbBookingGroup.Title, true);
                    }
                }

                await GetBookingData();
                StateHasChanged();
            }
        }
        private async void SaveCancelBooking()
        {
            if (CurrentBooking != null)
            {
                ShowBankDataWindow = false;
                StateHasChanged();

                if (!await Dialogs.ConfirmAsync(TextProvider.Get("ROOM_BOOKING_CANCEL_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                await RoomProvider.SetBookingCanceled(CurrentBooking.ID, NavManager.BaseUri);

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var dbBookingGroup = await RoomProvider.GetBookingGroup(CurrentBooking.ID);

                    if (dbBookingGroup != null && dbBookingGroup.ROOM_Booking_Type_ID != ROOMBookingType.Blocked)
                    {
                        BookingService.SendMessagesToContacts(SessionWrapper.AUTH_Municipality_ID.Value, dbBookingGroup.ID, dbBookingGroup.Title, true);
                    }
                }

                await GetBookingData();
                StateHasChanged();
            }
        }
        private void Booking_GoToPayment(Guid Booking_ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Room/Payment/" + Booking_ID);
            StateHasChanged();
        }
        private void Booking_GoToSigning(Guid Booking_ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Room/Sign/" + Booking_ID);
            StateHasChanged();
        }
        private void Booking_ShowDetails(Guid Booking_ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Room/User/" + Booking_ID);
            StateHasChanged();
        }
        private void HideCancelBooking()
        {
            CurrentBooking = null;
            ShowBankDataWindow = false;
            StateHasChanged();
        }
        private async Task GoToRequestRefundBalanceMensa(Guid RequestID)
        {
            CANTEEN_RequestRefundBalances? _request = await CanteenProvider.GetRequestRefundBalance(RequestID.ToString());

            if (_request != null)
            {
                if (_request.SignedDate == null)
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
        private void GoToCanteenDashboard()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Canteen/Service");
            StateHasChanged();                
        }
        private async Task GoToApplication(Guid FORM_Application_ID)
        {
            var application = await FormApplicationProvider.GetApplication(FORM_Application_ID);

            if (application != null && application.FORM_Definition_ID != null)
            {
                var definition = await FormDefinitionProvider.GetDefinition(application.FORM_Definition_ID.Value);

                if (definition != null && !definition.Enabled)
                {
                    return;
                }
            }

            if (application != null)
            {
                if (application.FORM_Application_Status_ID == FORMStatus.Incomplete)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Form/Application/" + application.FORM_Definition_ID + "/" + application.ID);
                    StateHasChanged();
                }
                else if (application.SubmitAt == null && application.FORM_Application_Status_ID == FORMStatus.InSigning)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Form/Application/Preview/" + application.ID);
                    StateHasChanged();
                }
                else if (application.SubmitAt == null && (application.FORM_Application_Status_ID == FORMStatus.ToSign || application.FORM_Application_Status_ID == FORMStatus.ToPay
                         || application.FORM_Application_Status_ID == FORMStatus.PayProcessing))
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Form/Application/Preview/" + application.ID);
                    StateHasChanged();
                }
                else if (application != null)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Form/Application/UserDetails/" + application.ID);
                    StateHasChanged();
                }
            }

        }
        private async void Organization_GoToApplication(Guid ORG_Request_ID)
        {
            var application = await OrgProvider.GetRequest(ORG_Request_ID);

            if (application != null && application.ORG_Request_Status_ID == Guid.Parse("75b8b15c-9fda-4748-a7a2-1f2d409a521e"))  //TO SIGN
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Organization/Application/Sign/" + application.ID);
                StateHasChanged();
                return;
            }
            else if (application != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Organization/Detail/" + application.ID);
                StateHasChanged();
            }
        }
        private async Task<string?> Organization_ActionButtonText(Guid ORG_Request_ID)
        {
            var application = await OrgProvider.GetRequest(ORG_Request_ID);

            if (application != null && application.ORG_Request_Status_ID == Guid.Parse("75b8b15c-9fda-4748-a7a2-1f2d409a521e"))  //TO SIGN
            {
                return "BUTTON_SIGN";
            }

            return null;
        }
    }
}
