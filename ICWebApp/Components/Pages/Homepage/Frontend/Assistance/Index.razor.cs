using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Amministration;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Assistance
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
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        private HOME_Assistance? Data = new HOME_Assistance();
        private PRIV_Privacy? Privacy;
        private bool PrivacyError = false;
        private bool? IsValid;
        private List<ServiceItem> Services = new List<ServiceItem>();
        private List<ServiceKategorieItems> Categories = new List<ServiceKategorieItems>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOME_ASSISTANCE_PAGE_TITLE");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOME_ASSISTANCE_PAGE_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Assistance", "HOME_ASSISTANCE_PAGE_TITLE", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Privacy = await PrivProvider.GetPrivacy(SessionWrapper.AUTH_Municipality_ID.Value);
                Services = HPServiceHelper.GetServices(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                Categories = HPServiceHelper.GetKategories(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void Commit()
        {
            if (Data != null) 
            {
                PrivacyError = false;

                if (!Data.Privacy)
                {
                    PrivacyError = true;
                }

                if (PrivacyError || string.IsNullOrEmpty(Data.Firstname) || string.IsNullOrEmpty(Data.Lastname) || string.IsNullOrEmpty(Data.EMail) || 
                    string.IsNullOrEmpty(Data.Description) || Data.CategoryID == null || Data.ServiceID == null)
                {
                    IsValid = false;

                    return;
                }

                Data.CreationDate = DateTime.Now;
                Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID;

                await HomeProvider.SetAssistance(Data);

                if (!string.IsNullOrEmpty(Data.EMail))
                {
                    SendMail(Data.EMail);
                }

                if (Data.ServiceID != null)
                {
                    await SendAuthorityMessages(Data.ServiceID.Value);
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Assistance/" + Data.ID);
                StateHasChanged();
            }
        }
        private async Task<bool> SendAuthorityMessages(Guid ServiceID)
        {
            MSG_Message msg = new MSG_Message();

            msg.Subject = TextProvider.Get("HOME_ASSISTANCE_MAIL_TITLE");
            msg.Messagetext = TextProvider.Get("HOME_ASSISTANCE_MAIL_CONTENT");

            if (Data != null)
            {
                msg.Messagetext = msg.Messagetext.Replace("{Firstname}", Data.Firstname);
                msg.Messagetext = msg.Messagetext.Replace("{Lastname}", Data.Lastname);
                msg.Messagetext = msg.Messagetext.Replace("{EMail}", Data.EMail);
                msg.Messagetext = msg.Messagetext.Replace("{Details}", Data.Description);

                var category = Categories.FirstOrDefault(p => p.ID == Data.CategoryID);

                if (category != null)
                {
                    msg.Messagetext = msg.Messagetext.Replace("{Categorie}", category.Title);
                }

                var service = Services.FirstOrDefault(p => p.ID == Data.ServiceID);

                if (service != null)
                {
                    msg.Messagetext = msg.Messagetext.Replace("{Service}", service.Title);
                }
            }

            Guid? authID = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                if (ServiceID == HomepageServices.Mensa)
                {
                    var auth = await AuthProvider.GetAuthorityMensa(SessionWrapper.AUTH_Municipality_ID.Value);

                    if (auth != null)
                    {
                        authID = auth.ID;
                    }
                }
                else if (ServiceID == HomepageServices.Rooms)
                {

                    var auth = await AuthProvider.GetAuthorityRooms(SessionWrapper.AUTH_Municipality_ID.Value);

                    if (auth != null)
                    {
                        authID = auth.ID;
                    }
                }
                else if (ServiceID == HomepageServices.Maintenance)
                {

                    var auth = await AuthProvider.GetAuthorityMaintenance(SessionWrapper.AUTH_Municipality_ID.Value);

                    if (auth != null)
                    {
                        authID = auth.ID;
                    }
                }
                else
                {
                    var formDef = await FormDefinitionProvider.GetDefinition(ServiceID);

                    if(formDef != null && formDef.AUTH_Authority_ID != null)
                    {
                        authID = formDef.AUTH_Authority_ID.Value;
                    }
                }

                if (authID != null)
                {
                    await MessageService.SendMessageToAuthority(authID.Value, msg);
                }
            }

            return true;
        }
        private async void SendMail(string EMail)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_ASSISTANCE_MAIL_TITLE");

            mail.Body = TextProvider.Get("HOME_ASSISTANCE_MAIL_CONTENT");

            if (Data != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", Data.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", Data.Lastname);
                mail.Body = mail.Body.Replace("{EMail}", Data.EMail);
                mail.Body = mail.Body.Replace("{Details}", Data.Description);

                var category = Categories.FirstOrDefault(p => p.ID == Data.CategoryID);

                if (category != null)
                {
                    mail.Body = mail.Body.Replace("{Categorie}", category.Title);
                }

                var service = Services.FirstOrDefault(p => p.ID == Data.ServiceID);

                if (service != null)
                {
                    mail.Body = mail.Body.Replace("{Service}", service.Title);
                }
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await MailerService.SendMail(mail, null, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
    }
}
