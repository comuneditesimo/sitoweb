using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Request;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Request
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IPRIVProvider PrivProvider { get; set; }
        [Inject] IHPServiceHelper HPServiceHelper { get; set; }
        [Inject] IMailerService MailerService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IMessageService MessageService { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] ICalendarHelper CalendarHelper { get; set; }
        [Inject] ISMSService SMSService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private HOME_Request? Data = new HOME_Request();
        private List<AUTH_Authority_Dates_Timeslot> Timeslots = new List<AUTH_Authority_Dates_Timeslot>();
        private List<AUTH_Authority_Dates_Closed> ClosedDates = new List<AUTH_Authority_Dates_Closed>();
        private List<V_AUTH_Authority_Reason> Reasons = new List<V_AUTH_Authority_Reason>();
        private List<V_HOME_Authority> Authorities = new List<V_HOME_Authority>();
        private V_HOME_Authority? SelectedAuthority;
        private int Step = 0;
        private bool locationSelection = false;
        private List<Timeslot> OpenSlots = new List<Timeslot>();
        private List<MonthItem> MonthItems = new List<MonthItem>();
        private int Month = 0;
        private int? Day;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_REQUEST_APPOINTMENT_TITLE");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_REQUEST_APPOINTMENT_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Request", "HOMEPAGE_REQUEST_APPOINTMENT_TITLE", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var auth = await HomeProvider.GetAuthorities(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                Authorities = auth.Where(p => p.AllowTimeslots == true).ToList();

                if (Data != null)
                {
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                    Data.CreationDate = DateTime.Now;

                    if (SessionWrapper.CurrentUser != null)
                    {
                        var anagrafic = await AuthProvider.GetAnagraficByUserID(SessionWrapper.CurrentUser.ID);

                        if (anagrafic != null)
                        {
                            Data.Firstname = SessionWrapper.CurrentUser.Firstname;
                            Data.Lastname = SessionWrapper.CurrentUser.Lastname;
                            Data.Fiscalnumber = anagrafic.FiscalNumber;

                            Data.EMail = SessionWrapper.CurrentUser.Email;
                            Data.Phone = anagrafic.MobilePhone;

                            Data.AUTH_Users_ID = SessionWrapper.CurrentUser.ID;
                        }
                    }
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void AuthorityChanged()
        {
            if (Data != null && Data.AUTH_Authority_ID != null)
            {
                SelectedAuthority = await HomeProvider.GetAuthority(Data.AUTH_Authority_ID.Value, LangProvider.GetCurrentLanguageID());

                if (SelectedAuthority != null)
                {
                    Reasons = await AuthProvider.GetAuthorityReasons(SelectedAuthority.ID, LangProvider.GetCurrentLanguageID());
                    Timeslots = await AuthProvider.GetAuthorityTimes(SelectedAuthority.ID);
                    ClosedDates = await AuthProvider.GetAuthorityClosedDates(SelectedAuthority.ID);

                    AvailableTimeslots();

                    if (!string.IsNullOrEmpty(SelectedAuthority.Address))
                    {
                        locationSelection = true;
                    }
                }
            }
            else
            {
                SelectedAuthority = null;
            }

            StateHasChanged();
        }
        private void NextStep()
        {
            AnchorService.ClearAnchors();
            Step++;

            if (OpenSlots != null && OpenSlots.Any())
            {
                var tm = OpenSlots.FirstOrDefault(p => p.Selected == true);

                if (Data != null && tm != null)
                {
                    Data.DateFrom = tm.DateFrom;
                    Data.DateTo = tm.DateTo;
                }
            }

            if(Data != null && SelectedAuthority != null)
            {
                Data.Address = SelectedAuthority.Address;
            }

            StateHasChanged();
        }
        private void PreviousStep()
        {
            AnchorService.ClearAnchors();
            Step--;
            StateHasChanged();
        }
        public async void AvailableTimeslots()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && SelectedAuthority != null)
            {
                var allDates = new List<DateTime>();
                var aktiveDates = new List<DateTime>();
                OpenSlots = new List<Timeslot>();
                DateTime StartDate = DateTime.Today;
                DateTime EndDate = DateTime.Today.AddDays(90);

                if (SelectedAuthority != null)
                {
                    StartDate = DateTime.Today.AddDays(SelectedAuthority.MinDaysAheadOfBooking);
                    EndDate = DateTime.Today.AddDays(SelectedAuthority.MaxDaysBookable);
                }

                for (var dt = StartDate; dt <= EndDate; dt = dt.AddDays(1))
                {
                    allDates.Add(dt);
                }

                foreach (var t in Timeslots.Select(p => p.Weekday).Distinct().ToList())
                {
                    if (t == 0)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Monday).ToList());
                    }
                    else if (t == 1)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Tuesday).ToList());
                    }
                    else if (t == 2)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Wednesday).ToList());
                    }
                    else if (t == 3)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Thursday).ToList());
                    }
                    else if (t == 4)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Friday).ToList());
                    }
                    else if (t == 5)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Saturday).ToList());
                    }
                }

                aktiveDates = aktiveDates.Where(p => ClosedDates.FirstOrDefault(x => x.Date == p) == null).ToList();

                foreach (var t in aktiveDates)
                {
                    var slotsInDay = Timeslots.Where(p => p.DayOfweek != null && p.DayOfweek == t.DayOfWeek).ToList();


                    foreach (var slot in slotsInDay)
                    {
                        if (slot.TimeFrom != null && slot.TimeTo != null)
                        {
                            var DateFrom = new DateTime(t.Year, t.Month, t.Day, slot.TimeFrom.Value.Hour, slot.TimeFrom.Value.Minute, 0);
                            var DateTo = new DateTime(t.Year, t.Month, t.Day, slot.TimeTo.Value.Hour, slot.TimeTo.Value.Minute, 0);

                            var existing = await HomeProvider.GetRequest(SessionWrapper.AUTH_Municipality_ID.Value, SelectedAuthority.ID, DateFrom);

                            if(existing == null)
                            {
                                OpenSlots.Add(new Timeslot()
                                {
                                    DateFrom = DateFrom,
                                    DateTo = DateTo 
                                });
                            }
                        }
                    }
                }

                MonthItems = new List<MonthItem>();

                foreach(var slot in OpenSlots)
                {
                    if(!MonthItems.Select(p => p.MonthInt).Contains(slot.MonthInt))
                    {
                        MonthItems.Add(new MonthItem()
                        {
                            Month = slot.Month,
                            MonthInt = slot.MonthInt,
                            YearMonth = slot.DateFrom.Value.ToString("yyyyMM")
                        });
                    }
                }

                StateHasChanged();
            }
        }
        private void ValueChanged()
        {
            StateHasChanged();
        }
        private void LocationChanged(bool value)
        {
            locationSelection = value;
            StateHasChanged();
        }
        private void SlotChanged(bool value, Timeslot slot)
        {
            foreach(var s in OpenSlots)
            {
                s.Selected = false;
            }

            slot.Selected = value;
            StateHasChanged();
        }
        private async void Save()
        {
            if(Data != null)
            {
                Data.CreationDate = DateTime.Now;

                if(Data.AUTH_Users_ID == null && !string.IsNullOrEmpty(Data.Fiscalnumber))
                {
                    var user = await AuthProvider.GetUser(Data.Fiscalnumber);

                    if(user != null)
                    {
                        Data.AUTH_Users_ID = user.ID;
                    }
                }

                await HomeProvider.SetRequest(Data);

                if (!string.IsNullOrEmpty(Data.EMail))
                {
                    SendMail(Data.EMail);
                }
                if (!string.IsNullOrEmpty(Data.Phone))
                {
                    SendSms(Data.Phone);
                }

                if (Data.AUTH_Authority_ID != null)
                {
                    await SendAuthorityMessages(Data.AUTH_Authority_ID.Value);
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Request/" + Data.ID);
                StateHasChanged();
            }
        }
        private async Task<bool> SendAuthorityMessages(Guid AUTH_Authority_ID)
        {
            MSG_Message msg = new MSG_Message();

            msg.Subject = TextProvider.Get("HOME_REQUEST_MAIL_TITLE");
            msg.Messagetext = TextProvider.Get("HOME_REQUEST_MAIL_CONTENT");

            if (Data != null && SelectedAuthority != null && Data.DateFrom != null && Data.DateTo != null)
            {
                msg.Messagetext = msg.Messagetext.Replace("{Firstname}", Data.Firstname);
                msg.Messagetext = msg.Messagetext.Replace("{Lastname}", Data.Lastname);
                msg.Messagetext = msg.Messagetext.Replace("{Fiscalcode}", Data.Fiscalnumber);

                msg.Messagetext = msg.Messagetext.Replace("{EMail}", Data.EMail);
                msg.Messagetext = msg.Messagetext.Replace("{Phone}", Data.Phone);

                msg.Messagetext = msg.Messagetext.Replace("{Authority}", SelectedAuthority.Title);
                msg.Messagetext = msg.Messagetext.Replace("{Address}", Data.Address);
                msg.Messagetext = msg.Messagetext.Replace("{Date}", Data.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + Data.DateTo.Value.ToString("HH:mm"));
                
                var reason = Reasons.FirstOrDefault(p => p.ID == Data.AUTH_Authority_Reason_ID);

                if (reason != null) 
                {
                    msg.Messagetext = msg.Messagetext.Replace("{Reason}", reason.Title);

                    var path = CalendarHelper.GetFilePath(Data.ID, Data.DateFrom.Value, Data.DateTo.Value, SelectedAuthority.Title, reason.Title, Data.Address);

                    string newpath = Path.Combine(NavManager.BaseUri, path);

                    msg.Messagetext = msg.Messagetext.Replace("{Link}", newpath);
                }

                msg.Messagetext = msg.Messagetext.Replace("{Details}", Data.Details);                
            }

            await MessageService.SendMessageToAuthority(AUTH_Authority_ID, msg);            
            
            return true;
        }
        private async void SendMail(string EMail)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_REQUEST_MAIL_TITLE");

            mail.Body = TextProvider.Get("HOME_REQUEST_MAIL_CONTENT");

            if (Data != null && SelectedAuthority != null && Data.DateFrom != null && Data.DateTo != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", Data.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", Data.Lastname);
                mail.Body = mail.Body.Replace("{Fiscalcode}", Data.Fiscalnumber);
                               
                mail.Body = mail.Body.Replace("{EMail}", Data.EMail);
                mail.Body = mail.Body.Replace("{Phone}", Data.Phone);
                                               
                mail.Body = mail.Body.Replace("{Authority}", SelectedAuthority.Title);
                mail.Body = mail.Body.Replace("{Address}", Data.Address);
                mail.Body = mail.Body.Replace("{Date}", Data.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + Data.DateTo.Value.ToString("HH:mm"));

                var reason = Reasons.FirstOrDefault(p => p.ID == Data.AUTH_Authority_Reason_ID);

                if (reason != null)
                {
                    mail.Body = mail.Body.Replace("{Reason}", reason.Title);

                    var path = CalendarHelper.GetFilePath(Data.ID, Data.DateFrom.Value, Data.DateTo.Value, SelectedAuthority.Title, reason.Title, Data.Address);

                    string newpath = Path.Combine(NavManager.BaseUri, path);

                    mail.Body = mail.Body.Replace("{Link}", newpath);
                }

                mail.Body = mail.Body.Replace("{Details}", Data.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await MailerService.SendMail(mail, null, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendSms(string Phone)
        {
            MSG_SMS sms = new MSG_SMS();

            sms.PhoneNumber = Phone;

            sms.Message = TextProvider.Get("HOME_REQUEST_SMS_CONTENT");

            if (Data != null && SelectedAuthority != null && Data.DateFrom != null && Data.DateTo != null)
            {
                sms.Message = sms.Message.Replace("{Firstname}", Data.Firstname);
                sms.Message = sms.Message.Replace("{Lastname}", Data.Lastname);
                sms.Message = sms.Message.Replace("{Fiscalcode}", Data.Fiscalnumber);

                sms.Message = sms.Message.Replace("{EMail}", Data.EMail);
                sms.Message = sms.Message.Replace("{Phone}", Data.Phone);

                sms.Message = sms.Message.Replace("{Authority}", SelectedAuthority.Title);
                sms.Message = sms.Message.Replace("{Address}", Data.Address);
                sms.Message = sms.Message.Replace("{Date}", Data.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + Data.DateTo.Value.ToString("HH:mm"));

                var reason = Reasons.FirstOrDefault(p => p.ID == Data.AUTH_Authority_Reason_ID);

                if (reason != null)
                {
                    sms.Message = sms.Message.Replace("{Reason}", reason.Title);

                    var path = CalendarHelper.GetFilePath(Data.ID, Data.DateFrom.Value, Data.DateTo.Value, SelectedAuthority.Title, reason.Title, Data.Address);

                    string newpath = Path.Combine(NavManager.BaseUri, path);

                    sms.Message = sms.Message.Replace("{Link}", newpath);
                }

                sms.Message = sms.Message.Replace("{Details}", Data.Details);
            }



            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await SMSService.SendSMS(sms, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
    }
}
