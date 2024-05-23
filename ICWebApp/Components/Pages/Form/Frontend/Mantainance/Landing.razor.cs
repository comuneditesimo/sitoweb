using Blazored.SessionStorage;
using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Form.Frontend.Mantainance;

public partial class Landing
{
    [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
    [Inject] ISessionWrapper SessionWrapper { get; set; }
    [Inject] IHOMEProvider HomeProvider { get; set; }
    [Inject] ITEXTProvider TextProvider { get; set; }
    [Inject] NavigationManager NavManager { get; set; }
    [Inject] IBreadCrumbService CrumbService { get; set; }
    [Inject] ISessionStorageService SessionStorage { get; set; }
    [Inject] IFILEProvider FileProvider { get; set; }
    [Inject] IEnviromentService EnviromentService { get; set; }
    [Inject] IMyCivisService MyCivisService { get; set; }
    [Inject] ILANGProvider LangProvider { get; set; }
    [Inject] IActionBarService ActionBarService { get; set; }
    [Inject] IAPPProvider AppProvider { get; set; }
    [Inject] IAUTHProvider AuthProvider { get; set; }
    [Inject] IHPServiceHelper HpServiceHelper { get; set; }

    private List<V_HOME_Theme>? DataThemes;
    private V_MAIN_Detail? Data;
    private bool IsHomepage = false;
    private AUTH_Authority? Authority;
    private bool MetaInitialized = false;

    protected override async Task OnInitializedAsync()
    {
        BusyIndicatorService.IsBusy = true;
        SessionWrapper.PageTitle = TextProvider.Get("FRONTEND_MANTAINANCE_TITLE");

        if (SessionWrapper.AUTH_Municipality_ID != null)
        {
            var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Maintenance);

            if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
            {
                SessionWrapper.PageSubTitle = pagesubTitle.Description;
            }
            else
            {
                SessionWrapper.PageSubTitle = TextProvider.Get("HOMEPAGE_FRONTEND_MAINTENANCE_DESCRIPTION");
            }
        }

        SessionWrapper.PageDescription = null;
        SessionWrapper.DataElement = "service-title";

        CrumbService.ClearBreadCrumb();
        CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null);
        CrumbService.AddBreadCrumb("/Mantainance/Landing", "FRONTEND_MANTAINANCE_TITLE", null, null, true);

        if (SessionWrapper != null)
        {
            SessionWrapper.PageButtonAction = ContinueToService;
            SessionWrapper.PageButtonActionTitle = TextProvider.Get("CANTEEN_DASHBOARD_ONLINE_SERVICE");
            SessionWrapper.PageButtonDataElement = "service-online-access";

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data = await HomeProvider.GetVMaintenanceDetail(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                if (Data != null && Data.Managed_AUTH_Authority_ID != null)
                {
                    Authority = await AuthProvider.GetAuthority(Data.Managed_AUTH_Authority_ID.Value);
                }

                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

                if (IsHomepage)
                {
                    SessionWrapper.PageSubTitle = null;
                    SessionWrapper.ShowTitleSepparation = true;
                    
                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Maintenance);

                        if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                        {
                            SessionWrapper.PageDescription = pagesubTitle.Description;
                        }
                        else
                        {
                            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_MAINTENANCE_DESCRIPTION");
                        }
                    }

                    SessionWrapper.PageChipValue = TextProvider.Get("HOMEPAGE_SERVICE_ACTIVE");

                    ActionBarService.ClearActionBar();
                    ActionBarService.ShowShareButton = true;
                    ActionBarService.ShowDefaultButtons = true;

                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        DataThemes = await HomeProvider.GetThemesByMaintenance(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                        ActionBarService.ThemeList = DataThemes;
                    }

                }
            }
        }

        MetaInitialized = false;
        BusyIndicatorService.IsBusy = false;
        StateHasChanged();
        await base.OnInitializedAsync();
    }
    protected override async void OnAfterRender(bool firstRender)
    {
        if (!MetaInitialized)
        {
            if (Authority != null && SessionWrapper.AUTH_Municipality_ID != null)
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
                    Name = TextProvider.Get("FRONTEND_MANTAINANCE_TITLE"),
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

        base.OnAfterRender(firstRender);
    }
    private void ContinueToService()
    {
        BusyIndicatorService.IsBusy = true;
        NavManager.NavigateTo("/Mantainance");
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
    private void OfflineForm()
    {
        BusyIndicatorService.IsBusy = true;
        NavManager.NavigateTo("/Hp/Request");
        StateHasChanged();
    }
}