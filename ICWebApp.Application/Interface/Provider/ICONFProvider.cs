using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ICONFProvider
    {
        public Task<CONF_Mailer?> GetMailerConfiguration(Guid? ID);
        public Task<CONF_SMS?> GetSMSConfiguration(Guid? ID);
        public Task<CONF_Push?> GetPushConfiguration(Guid? ID);
        public Task<CONF_Sign?> GetSignConfiguration(Guid? ID, Guid? AUTH_Municipality_ID = null);
        public Task<CONF_Mailer_Type?> GetMailerType(Guid? ID);
        public Task<CONF_Enviroment?> GetEnviromentConfiguration(Guid? ID);
        public Task<List<CONF_MainMenu>?> GetLoggedInMainMenuElements();
        public Task<List<CONF_MainMenu>?> GetLoggedInSubMenuElements(Guid ParentID);
        public Task<List<CONF_MainMenu>?> GetPublicMainMenuElements();
        public Task<List<CONF_MainMenu>?> GetPublicSubMenuElements(Guid ParentID);
        public Task<List<CONF_MainMenu>?> GetCitizenMainMenuElements();
        public Task<List<CONF_MainMenu>?> GetCitizenSubMenuElements(Guid ParentID);
        public Task<List<CONF_MainMenu>?> GetMunicipalLoggedInMainMenuElements();
        public Task<List<CONF_MainMenu>?> GetMunicipalLoggedInSubMenuElements(Guid ParentID);
        public Task<List<CONF_MainMenu>?> GetMunicipalCitizenMainMenuElements();
        public Task<List<CONF_MainMenu>?> GetMunicipalCitizenSubMenuElements(Guid ParentID);
        public Task<CONF_MainMenu_Setttings?> GetMainMenuSettings(Guid? ID);
        public Task<CONF_MainMenu> GetMenuByID(Guid ID);
        public Task<List<CONF_MainMenu>?> GetAuthorityList(Guid ParentID);
        public Task<List<CONF_MainMenu>?> GetMantainanceList(Guid ParentID);
        public Task<List<CONF_MainMenu>?> GetBackendAuthorityList(Guid ParentID);
        public Task<CONF_Sign?> SetSignConfiguration(CONF_Sign Data);
        public Task<List<CONF_Sign>> GetSignConfigurationList();
        public Task<CONF_PAY?> GetPayConfiguration(Guid? ID);
        public Task<CONF_PagoPA?> GetPagoPAConfiguration(Guid? ID);
        public Task<CONF_Veriff?> GetVeriffConfiguration(Guid? ID);
        public Task<CONF_Freshdesk?> GetFreshDeskConfiguration(Guid? ID);
        public Task<List<V_CONF_Freshdesk_Priority>?> GetVPriorityList();
        public Task<CONF_Spid?> GetSpidConfiguration(Guid AUTH_Municipality_ID);
        public Task<CONF_Spid_Maintenance?> GetSpidMaintenance();
        public Task<List<CONF_MyCivis>?> GetMyCivisConfiguration(Guid AUTH_Municipality_ID);
    }
}
