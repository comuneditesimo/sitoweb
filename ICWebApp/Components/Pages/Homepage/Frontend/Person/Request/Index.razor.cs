using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Request;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Person.Request
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
        [Parameter] public string? ID { get; set; }

        private V_HOME_Person? Item;
        private HOME_Person_Request? Data = new HOME_Person_Request();
        private List<HOME_Person_Dates_Timeslots> Timeslots = new List<HOME_Person_Dates_Timeslots>();
        private List<HOME_Person_Dates_Closed> ClosedDates = new List<HOME_Person_Dates_Closed>();
        private int Step = 0;
        private bool locationSelection = false;
        private List<Timeslot> OpenSlots = new List<Timeslot>();
        private List<MonthItem> MonthItems = new List<MonthItem>();
        private int Month = 0;
        private int? Day;
        private V_HOME_Person? SelectedPerson;
        private List<V_HOME_Person> People = new List<V_HOME_Person>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_REQUEST_PERSON_APPOINTMENT_TITLE");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_REQUEST_PERSON_APPOINTMENT_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);

            if (ID != null)
            {
                Item = await HomeProvider.GetVPerson(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                if (Item != null && Item.HOME_Person_Type_ID == PersonType.Politician)
                {
                    CrumbService.AddBreadCrumb("/Hp/Type/Person/" + PersonType.Politician, Item.Type, null, null, false);
                }
                else
                {
                    CrumbService.AddBreadCrumb("/Hp/Type/Person/" + PersonType.Employee, Item.Type, null, null, false);
                }
            }

            CrumbService.AddBreadCrumb("/Hp/Request", "HOMEPAGE_REQUEST_PERSON_APPOINTMENT_TITLE", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                People = await HomeProvider.GetPeopleWithTimeslots(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                if (Data != null)
                {
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                    Data.CreationDate = DateTime.Now;

                    if (ID != null)
                    {
                        Data.HOME_Person_ID = Guid.Parse(ID);
                    }

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

                    PersonChanged();
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void PersonChanged()
        {
            if (Data != null && Data.HOME_Person_ID != null)
            {
                SelectedPerson = await HomeProvider.GetVPerson(Data.HOME_Person_ID.Value, LangProvider.GetCurrentLanguageID());

                if (SelectedPerson != null)
                {
                    Timeslots = await HomeProvider.GetPersonTimes(SelectedPerson.ID);
                    ClosedDates = await HomeProvider.GetPersonClosedDates(SelectedPerson.ID);

                    AvailableTimeslots();

                    if (!string.IsNullOrEmpty(SelectedPerson.Address))
                    {
                        locationSelection = true;
                    }
                }
            }
            else
            {
                SelectedPerson = null;
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

            if(Data != null && SelectedPerson != null)
            {
                Data.Address = SelectedPerson.Address;
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
            if (SessionWrapper.AUTH_Municipality_ID != null && SelectedPerson != null)
            {
                var allDates = new List<DateTime>();
                var aktiveDates = new List<DateTime>();
                OpenSlots = new List<Timeslot>();
                DateTime StartDate = DateTime.Today;
                DateTime EndDate = DateTime.Today.AddDays(90);

                if (SelectedPerson != null)
                {
                    StartDate = DateTime.Today.AddDays(SelectedPerson.MinDaysAheadOfBooking);
                    EndDate = DateTime.Today.AddDays(SelectedPerson.MaxDaysBookable);
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

                            var existing = await HomeProvider.GetRequest(SessionWrapper.AUTH_Municipality_ID.Value, SelectedPerson.ID, DateFrom);

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

                await HomeProvider.SetPersonRequest(Data);

                if (!string.IsNullOrEmpty(Data.EMail))
                {
                    SendMail(Data.EMail);
                }
                if (!string.IsNullOrEmpty(Data.Phone))
                {
                    SendSms(Data.Phone);
                }

                if (Data.HOME_Person_ID != null)
                {
                    await SendPersonMessage();
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Person/Request/Success/" + Data.ID);
                StateHasChanged();
            }
        }
        private async Task<bool> SendPersonMessage()
        {
            MSG_Message msg = new MSG_Message();

            msg.Subject = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_TITLE");
            msg.Messagetext = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CONTENT");

            if (Data != null && SelectedPerson != null && Data.DateFrom != null && Data.DateTo != null)
            {
                msg.Messagetext = msg.Messagetext.Replace("{Firstname}", Data.Firstname);
                msg.Messagetext = msg.Messagetext.Replace("{Lastname}", Data.Lastname);
                msg.Messagetext = msg.Messagetext.Replace("{Fiscalcode}", Data.Fiscalnumber);

                msg.Messagetext = msg.Messagetext.Replace("{EMail}", Data.EMail);
                msg.Messagetext = msg.Messagetext.Replace("{Phone}", Data.Phone);

                msg.Messagetext = msg.Messagetext.Replace("{Person}", SelectedPerson.Fullname);
                msg.Messagetext = msg.Messagetext.Replace("{Address}", Data.Address);
                msg.Messagetext = msg.Messagetext.Replace("{Date}", Data.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + Data.DateTo.Value.ToString("HH:mm"));
                
                var path = CalendarHelper.GetFilePath(Data.ID, Data.DateFrom.Value, Data.DateTo.Value, SelectedPerson.Fullname, Location: Data.Address);

                string newpath = Path.Combine(NavManager.BaseUri, path);

                msg.Messagetext = msg.Messagetext.Replace("{Link}", newpath);
                

                msg.Messagetext = msg.Messagetext.Replace("{Details}", Data.Details);                
            }

            await MessageService.SendMessage(msg);            
            
            return true;
        }
        private async void SendMail(string EMail)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_TITLE");
            mail.Body = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CONTENT");

            if (Data != null && SelectedPerson != null && Data.DateFrom != null && Data.DateTo != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", Data.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", Data.Lastname);
                mail.Body = mail.Body.Replace("{Fiscalcode}", Data.Fiscalnumber);
                               
                mail.Body = mail.Body.Replace("{EMail}", Data.EMail);
                mail.Body = mail.Body.Replace("{Phone}", Data.Phone);

                mail.Body = mail.Body.Replace("{Person}", SelectedPerson.Fullname);
                mail.Body = mail.Body.Replace("{Address}", Data.Address);
                mail.Body = mail.Body.Replace("{Date}", Data.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + Data.DateTo.Value.ToString("HH:mm"));

                var path = CalendarHelper.GetFilePath(Data.ID, Data.DateFrom.Value, Data.DateTo.Value, SelectedPerson.Fullname, Location: Data.Address);

                string newpath = Path.Combine(NavManager.BaseUri, path);

                mail.Body = mail.Body.Replace("{Link}", newpath);                

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

            sms.Message = TextProvider.Get("HOME_REQUEST_PERSON_SMS_CONTENT");

            if (Data != null && SelectedPerson != null && Data.DateFrom != null && Data.DateTo != null)
            {
                sms.Message = sms.Message.Replace("{Firstname}", Data.Firstname);
                sms.Message = sms.Message.Replace("{Lastname}", Data.Lastname);
                sms.Message = sms.Message.Replace("{Fiscalcode}", Data.Fiscalnumber);

                sms.Message = sms.Message.Replace("{EMail}", Data.EMail);
                sms.Message = sms.Message.Replace("{Phone}", Data.Phone);

                sms.Message = sms.Message.Replace("{Person}", SelectedPerson.Fullname);
                sms.Message = sms.Message.Replace("{Address}", Data.Address);
                sms.Message = sms.Message.Replace("{Date}", Data.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + Data.DateTo.Value.ToString("HH:mm"));

                var path = CalendarHelper.GetFilePath(Data.ID, Data.DateFrom.Value, Data.DateTo.Value, SelectedPerson.Fullname , Location: Data.Address);

                string newpath = Path.Combine(NavManager.BaseUri, path);

                sms.Message = sms.Message.Replace("{Link}", newpath);                

                sms.Message = sms.Message.Replace("{Details}", Data.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await SMSService.SendSMS(sms, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
    }
}
