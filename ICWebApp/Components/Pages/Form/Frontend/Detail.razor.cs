using ICWebApp.Domain.Models.Homepage.Services;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Components.Pages.Form.Frontend
{
    public partial class Detail
    {
        [Inject] IFORMApplicationProvider FormApplicationProvider { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IActionBarService ActionBarService { get; set; }
        [Inject] IHPServiceHelper HpServiceHelper { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Parameter] public string ID { get; set; }

        private AUTH_Authority? Authority { get; set; }
        private V_FORM_Definition? Data { get; set; }
        private List<FORM_Definition_Ressources>? DataRessources;
        private List<FORM_Definition_Ressources_Extended> DataResourceExtendeds = new List<FORM_Definition_Ressources_Extended>();
        private List<FORM_Definition_Property>? DataProperties;
        private List<FORM_Definition_Event>? DataEvents;
        private List<V_HOME_Theme>? DataThemes;
        private bool IsHomepage = false;
        private bool MetaInitialized = false;
        private bool ApplicationAlreadyRunning { get; set; } = false;
        private V_FORM_Definition_Type? DataType;
        private List<DeadlineItem> Events = new List<DeadlineItem>();
        private List<V_HOME_Person>? People;

        protected override async Task OnParametersSetAsync()
        {
            if (ID == null)
            {
                NavManager.NavigateTo("/");
                return;
            }

            AnchorService.ForceShow = true;
            AnchorService.SkipForceReset = true;
            StateHasChanged();

            await GetData();

            if (Data == null)
            {
                NavManager.NavigateTo("/");
                return;
            }

            SessionWrapper.PageTitle = Data.Name;
            SessionWrapper.PageSubTitle = Data.ShortText;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowBreadcrumb = true;
            SessionWrapper.DataElement = "service-title";

            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);
            }

            if (IsHomepage)
            {
                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null);

                if (Data.FORM_Defintion_Type_ID != null)
                {
                    DataType = await FormDefinitionProvider.GetFormType(Data.FORM_Defintion_Type_ID.Value, LangProvider.GetCurrentLanguageID());
                }

                if (DataType != null)
                {
                    CrumbService.AddBreadCrumb("/Hp/Type/Services/" + DataType.ID, null, null, DataType.Type);
                }

                if (Data.IsActive == true)
                {
                    SessionWrapper.PageChipValue = TextProvider.Get("HOMEPAGE_SERVICE_ACTIVE");

                    if (Data.IsOfflineForm == true)
                    {
                        if (ApplicationAlreadyRunning)
                        {
                            SessionWrapper.PageButtonAction = null;
                            SessionWrapper.PageButtonActionTitle = null;
                            SessionWrapper.PageButtonSecondaryAction = null;
                            SessionWrapper.PageButtonSecondaryActionTitle = null;
                        }
                        else if (Data.ApplicationDeadline == null || Data.ApplicationDeadline >= DateTime.Now)
                        {
                            if (Data.AllowPrenotation)
                            {
                                SessionWrapper.PageButtonSecondaryAction = OfflineForm;
                                SessionWrapper.PageButtonSecondaryActionTitle = TextProvider.Get("FORM_DETAIL_OFFLINE_FORM_BUTTON");
                                SessionWrapper.PageButtonSecondaryDataElement = "service-booking-access";
                            }
                        }
                        else
                        {
                            SessionWrapper.PageButtonAction = null;
                            SessionWrapper.PageButtonActionTitle = null;
                            SessionWrapper.PageButtonSecondaryAction = null;
                            SessionWrapper.PageButtonSecondaryActionTitle = null;
                        }
                    }
                    else
                    {
                        if (ApplicationAlreadyRunning)
                        {
                            SessionWrapper.PageButtonAction = null;
                            SessionWrapper.PageButtonActionTitle = null;
                            SessionWrapper.PageButtonSecondaryAction = null;
                            SessionWrapper.PageButtonSecondaryActionTitle = null;
                        }
                        else if (Data.ApplicationDeadline == null || Data.ApplicationDeadline >= DateTime.Now)
                        {
                            SessionWrapper.PageButtonAction = SubmitForm;
                            SessionWrapper.PageButtonActionTitle = TextProvider.Get("FORM_DETAIL_SUBMIT_BUTTON");
                            SessionWrapper.PageButtonDataElement = "service-online-access";

                            if (Data.AllowPrenotation)
                            {
                                SessionWrapper.PageButtonSecondaryAction = OfflineForm;
                                SessionWrapper.PageButtonSecondaryActionTitle = TextProvider.Get("FORM_DETAIL_OFFLINE_FORM_BUTTON");
                                SessionWrapper.PageButtonSecondaryDataElement = "service-booking-access";
                            }
                        }
                        else
                        {
                            SessionWrapper.PageButtonAction = null;
                            SessionWrapper.PageButtonActionTitle = null;
                            SessionWrapper.PageButtonSecondaryAction = null;
                            SessionWrapper.PageButtonSecondaryActionTitle = null;
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageChipValue = TextProvider.Get("HOMEPAGE_SERVICE_IN_PREPARATION");
                }
            }
            else
            {
                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Form", "FRONTEND_AUTHORITY_FORM_LIST", null);

                if (Authority != null)
                {
                    CrumbService.AddBreadCrumb("/Form/List/" + Authority.ID, null, null, Authority.Description);
                }

                if (ApplicationAlreadyRunning)
                {
                    SessionWrapper.PageButtonAction = null;
                    SessionWrapper.PageButtonActionTitle = null;
                }
                else if (Data.ApplicationDeadline == null || Data.ApplicationDeadline >= DateTime.Now)
                {
                    SessionWrapper.PageButtonAction = SubmitForm;
                    SessionWrapper.PageButtonActionTitle = TextProvider.Get("FORM_DETAIL_SUBMIT_BUTTON");
                }
                else
                {
                    SessionWrapper.PageButtonAction = null;
                    SessionWrapper.PageButtonActionTitle = null;
                }
            }

            CrumbService.AddBreadCrumb(NavManager.ToBaseRelativePath(NavManager.Uri), null, null, Data.Name);

            ApplicationAlreadyRunning = false;

            if (Data.MultipleParallelApplications == false && SessionWrapper.CurrentUser != null)
            {
                Guid UserID = SessionWrapper.CurrentUser.ID;

                if (SessionWrapper.CurrentSubstituteUser != null)
                {
                    UserID = SessionWrapper.CurrentSubstituteUser.ID;
                }

                var exisitingApplications = await FormApplicationProvider.GetApplicationListByUser(UserID, Data.ID);

                foreach (var app in exisitingApplications)
                {
                    var status = FormApplicationProvider.GetStatus(app.FORM_Application_Status_ID.Value);

                    if (status != null && status.FinishedStatus != true)
                    {
                        ApplicationAlreadyRunning = true;
                    }
                }
            }

            if (Data.ApplicationDeadline != null || (Data.EstimateProcessingTime != null && Data.EstimateProcessingTime > 0) || (Data.LegalDeadline != null) || (DataEvents != null && DataEvents.Count() > 0))
            {
                if(Data.ApplicationDeadline != null)
                {
                    Events.Add(new DeadlineItem()
                    {
                        Date = Data.ApplicationDeadline.Value,
                        Title = TextProvider.Get("FORM_LIST_AUTHORITY_APPLICATION_DEADLINE"),
                        Description = TextProvider.Get("APPLICATION_DEADLINE_DESCRIPTION").Replace("{Date}", Data.ApplicationDeadline.Value.ToShortDateString()).Replace("{Time}", Data.ApplicationDeadline.Value.ToShortTimeString())
                    });
                }

                if((Data.EstimateProcessingTime != null && Data.EstimateProcessingTime > 0))
                {
                    Events.Add(new DeadlineItem()
                    {
                        Date = DateTime.Now.AddDays(Data.EstimateProcessingTime.Value),
                        Title = TextProvider.Get("FORM_LIST_AUTHORITY_ESTIMATE_TIME")
                    });
                }
                if (Data.LegalDeadline != null)
                {
                    Events.Add(new DeadlineItem()
                    {
                        Date = DateTime.Now.AddDays(Data.LegalDeadline.Value),
                        Title = TextProvider.Get("FORM_LIST_AUTHORITY_LEGAL_DEADLINE")
                    });
                }
                if (DataEvents != null)
                {
                    foreach (var e in DataEvents)
                    {
                        if (e.DisplayStartDate != null && e.DisplayStartDate.Value && e.FromDate != null)
                        {
                            Events.Add(new DeadlineItem()
                            {
                                Date = e.FromDate.Value,
                                Title = e.Title,
                                Description = e.Description
                            });
                        }
                        else if(e.ToDate != null)
                        {
                            Events.Add(new DeadlineItem()
                            {
                                Date = e.ToDate.Value,
                                Title = e.Title,
                                Description = e.Description
                            });
                        }
                    }
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            AnchorService.SkipForceReset = false;

            ActionBarService.ClearActionBar();
            ActionBarService.ShowShareButton = true;
            ActionBarService.ShowDefaultButtons = true;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                if (IsHomepage)
                {
                    SessionWrapper.PageSubTitle = null;
                    SessionWrapper.ShowTitleSepparation = true;
                    SessionWrapper.PageDescription = Data.ShortText;

                    ActionBarService.ClearActionBar();
                    ActionBarService.ShowShareButton = true;
                    ActionBarService.ShowDefaultButtons = true;

                    DataThemes = await FormDefinitionProvider.GetVThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Data.ID);

                    People = await HomeProvider.GetPeopleByFormDefinition(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Data.ID);
                    ActionBarService.ThemeList = DataThemes;

                    MetaInitialized = false;
                }
            }

            await base.OnParametersSetAsync();
        }
        protected override async void OnAfterRender(bool firstRender)
        {
            if (!MetaInitialized)
            {
                if (Data != null && Authority != null && SessionWrapper.AUTH_Municipality_ID != null)
                {
                    string AddressSmall = "";
                    string cap = "";

                    if (!string.IsNullOrEmpty(Authority.Address))
                    {
                        var data = Authority.Address.Replace(", Autonome Provinz Bozen - Südtirol, Italien", "").Split(',').ToList();

                        if (data.Count() > 2)
                        {
                            AddressSmall = data[0] + " " + data[1];

                            cap = data[2];
                        }
                    }

                    var metaItem = new MetaItem()
                    {
                        Name = Data.Name,
                        Authority = Authority.Description,
                        AudienceType = "citizen",
                        Type = TextProvider.Get("HOMEPAGE_SERVICE_ONLINE_SERVICE"),
                        Address = AddressSmall,
                        Cap = cap,
                        Url = NavManager.Uri
                    };

                    await HpServiceHelper.CreateServiceScript(SessionWrapper.AUTH_Municipality_ID.Value, metaItem);
                    await HpServiceHelper.InjectServiceScript();

                    MetaInitialized = true;
                }
            }

            if (firstRender)
            {
                await EnviromentService.ScrollToTop();
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data = await FormDefinitionProvider.GetVDefinition(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                if (Data != null && Data.AUTH_Authority_ID != null)
                {
                    Authority = await AuthProvider.GetAuthority(Data.AUTH_Authority_ID.Value);
                }

                if (Data != null) 
                {
                    DataRessources = await FormDefinitionProvider.GetDefinitionRessourceList(Data.ID);
                    foreach (var res in DataRessources)
                    {
                        var extended = (await FormDefinitionProvider.GetDefinitionRessourceExtendedList(res.ID, LangProvider.GetCurrentLanguageID())).FirstOrDefault();
                        if (extended != null)
                            DataResourceExtendeds.Add(extended);
                    }
                }

                if (Data != null)
                {
                    DataProperties = await FormDefinitionProvider.GetDefinitionPropertyList(Data.ID);
                }

                if (Data != null)
                {
                    DataEvents = await FormDefinitionProvider.GetDefinitionEventsList(Data.ID);
                }
            }

            return true;
        }
        private void SubmitForm()
        {
            if (Data != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Form/Application/" + Data.ID + "/New");
                StateHasChanged();
            }
        }
        private void OfflineForm()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Request");
            StateHasChanged();
        }
        private void GoToReservations()
        {
            if (!NavManager.Uri.Contains("/Hp/Request"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Request");
                StateHasChanged();
            }
        }
    }
}
