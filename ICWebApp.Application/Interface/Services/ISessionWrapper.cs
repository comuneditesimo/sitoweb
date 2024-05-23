using ICWebApp.Application.Services;
using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface ISessionWrapper
    {
        public string Login_AuthToken { get; set; }
        public string Login_AuthTimestamp { get; set; }
        public string Login_UserID { get; set; }
        public bool PageIsRendered { get; set; }
        public bool PageIsPublic { get; set; }
        public bool ShowBreadcrumb { get; set; }
        public event Action OnShowBreadcrumbChanged;
        public string? DataElement { get; set; }
        public event Action OnDataElementChanged;
        public AUTH_Users? CurrentUser { get; set; }
        public event Action OnCurrentUserChanged;
        public AUTH_Municipal_Users? CurrentMunicipalUser { get; set; }
        public event Action OnCurrentMunicipalUserChanged;
        public AUTH_Users? CurrentSubstituteUser { get; set; }
        public event Action OnCurrentSubUserChanged;
        public Guid? AUTH_Municipality_ID { get; set; }
        public event Action OnAUTHMunicipalityIDChanged;
        public string PageTitle { get; set; }
        public event Action OnPageTitleChanged;
        public string PageSubTitle { get; set; }
        public event Action OnPageSubTitleChanged;
        public string? PageChipValue { get; set; }
        public event Action OnPageChipValueChanged;
        public string PageDescription { get; set; }
        public event Action OnPageDescriptionChanged;
        public Action? PageButtonAction { get; set; }
        public string? PageButtonActionTitle { get; set; }
        public Action? PageButtonSecondaryAction { get; set; }
        public string? PageButtonSecondaryActionTitle { get; set; }        
        public string? PageButtonDataElement { get; set; }
        public string? PageButtonSecondaryDataElement { get; set; }
        public bool ShowTitleSepparation { get; set; }
        public bool ShowTitleSmall { get; set; }
        public Task<Guid?> GetMunicipalityID();
        public bool Initializing {get;set; }
        public event Action OnInitialized;
        public bool ShowHelpSection { get; set; }
        public event Action OnShowHelpSectionChanged;
        public bool ShowQuestioneer { get; set; }
        public event Action ShowQuestioneerChanged;
        public Task<Guid?> SessionKeyID();
        public List<AUTH_MunicipalityApps> MunicipalityApps { get; set; }
        public DateTime? ReleaseDate {  get; set; }
        public int? ReadDuration { get; set; }
        public event Action OnReleaseChanged;
    }
}
