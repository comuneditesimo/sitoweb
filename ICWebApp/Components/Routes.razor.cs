using System.Globalization;
using Blazored.SessionStorage;
using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;

namespace ICWebApp.Components;

public partial class Routes
{
    [Inject] ISessionWrapper SessionWrapper { get; set; }
    [Inject] AuthenticationStateProvider AuthenticationHelper { get; set; }
    [Inject] ILocalizationService LocalizationService { get; set; }
    [Inject] ILANGProvider LangProvider { get; set; }
    [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
    [Inject] ISYSProvider SYSProvider { get; set; }
    [Inject] NavigationManager NavManager { get; set; }
    [Inject] ISessionStorageService SessionStorage { get; set; }
    [Inject] IAUTHProvider AuthProvider { get; set; }
    [Inject] IEnviromentService EnviromentService { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IAPPProvider APPProvider { get; set; }
    [Inject] ITEXTProvider TextProvider { get; set; }
    [Inject] ITASKService TaskService { get; set; }
    [Inject] IUrlService UrlService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        NavManager.LocationChanged += NavManager_LocationChanged;

        var baseUri = NavManager.BaseUri; 
        var matchedMun = await UrlService.GetMunicipalityByUrl(baseUri);
        
        if (matchedMun != null)
        {
            SessionWrapper.AUTH_Municipality_ID = matchedMun.AUTH_Municipality_ID;
        } 
        else if (!baseUri.ToLower().Contains("spid"))
        {
            NavManager.NavigateTo("https://innovation-consulting.it/", true);
        }

        await base.OnInitializedAsync();
    }
    private async void LogData()
    {
        var data = new SYS_Log();

        data.CreationDate = DateTime.Now;
        data.Url = NavManager.Uri;
        data.Type = "pagelog";

        if (SessionWrapper.CurrentUser != null)
        {
            data.AUTH_Users_ID = SessionWrapper.CurrentUser.ID;

            if (SessionWrapper.CurrentUser.AUTH_Municipality_ID != null)
                data.AUTH_Municipality_ID = SessionWrapper.CurrentUser.AUTH_Municipality_ID;
        }
        if (SessionWrapper.CurrentMunicipalUser != null)
        {
            data.AUTH_Users_ID = SessionWrapper.CurrentMunicipalUser.ID;

            if (SessionWrapper.CurrentMunicipalUser.AUTH_Municipality_ID != null)
                data.AUTH_Municipality_ID = SessionWrapper.CurrentMunicipalUser.AUTH_Municipality_ID;
        }

        await SYSProvider.SetLog(data);
    }
    private async void NavManager_LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        LogData();

        await VerifyAccess();

        CheckUser();

        TaskService.TASK_Context_ID = null;
        TaskService.ContextElementID = null;
        TaskService.ContextName = null;
        TaskService.ShowToolbar = true;
    }
    private void CheckUser()
    {
        if (SessionWrapper.CurrentUser != null)
        {
            AuthProvider.CheckUserVerifications(SessionWrapper.CurrentUser.ID);
        }
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SessionWrapper.PageIsRendered = true;

            if (AuthenticationHelper != null)
            {
                (AuthenticationHelper as AuthenticationHelper).Notify(); //HAS TO BE THE FIRST CALL HERE
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
			{
				var languages = await LangProvider.GetAll();
				var langId = await AuthProvider.GetLanguageIdFromDomain(NavManager.BaseUri, SessionWrapper.AUTH_Municipality_ID.Value);

				if (languages != null && langId != null && !await LangProvider.LanguageInitialized())
				{
					var lang = languages.FirstOrDefault(e => e.ID == langId);

                    if (lang != null)
					{
						await LangProvider.SetLanguage(lang.Code);
					}
				}
			}

            Task.Run(async () => {
                var mobile = await JSRuntime.InvokeAsync<bool>("isDevice");
                EnviromentService.IsMobile = mobile;
            });


            await VerifyAccess();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
    private async Task<bool> VerifyAccess()
    {
        var lockedUrlList = await APPProvider.GetAppUrl();
        var allowed = true;

        if (lockedUrlList != null)
        {
            var mainUrl = NavManager.Uri;
            var urlsToCheck = lockedUrlList.Where(p => p.Url != null && mainUrl.EndsWith(p.Url)).ToList();

            if (urlsToCheck != null && urlsToCheck.Count() > 0)
            {
                if (SessionWrapper.MunicipalityApps == null || SessionWrapper.MunicipalityApps.Count() == 0)
                    SessionWrapper.MunicipalityApps = await AuthProvider.GetMunicipalityApps(true);

                var appIDs = SessionWrapper.MunicipalityApps.Select(p => p.APP_Application_ID);

                var locCheck = false;

                foreach (var UrlCheck in urlsToCheck)
                    if (UrlCheck.APP_Applications_ID != null && appIDs.Contains(UrlCheck.APP_Applications_ID.Value))
                    {
                        locCheck = true;
                        break;
                    }

                allowed = locCheck;
            }
        }

        if (!allowed)
        {
            NavManager.NavigateTo("/");
            StateHasChanged();

            return false;
        }

        return true;
    }
}