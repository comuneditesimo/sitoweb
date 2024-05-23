using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using Telerik.Blazor.Components;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Text.Json;

namespace ICWebApp.Components.Pages.Form.Backend
{
    public partial class Administration : IDisposable
    {
        [Inject] IFormAdministrationHelper FormAdministrationHelper { get; set; }
        [Inject] IChatNotificationService? ChatNotificationService { get; set; }
        [Inject] IFORMApplicationProvider FormApplicationProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ISETTProvider SettProvider { get; set; }

        private bool IsDataBusy = true;
        private bool MunicipalityHasMantainances = true;
        private List<Guid> AllowedAuthorities = new List<Guid>();
        private List<V_FORM_Application> Data = new List<V_FORM_Application>();
        private Administration_Filter_Item Filter = new Administration_Filter_Item();
        private bool ColumnSettingsWindowVisible { get; set; } = false;
        private bool HasCommitteeRights { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_FORM_ADMINISTRATION");

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Form/Administration", "MAINMENU_BACKEND_FORM_ADMINISTRATION", null, null, true);

            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnMunicipalChatStatusChange += OnChatStatusChange;
            }

            if (FormAdministrationHelper.Filter != null)
            {
                Filter = FormAdministrationHelper.Filter;
            }
            else
            {
                Filter.FORM_Application_Status_ID = new List<Guid>();

                var municipalStatusList = FormApplicationProvider.GetStatusListByMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                foreach (var item in municipalStatusList.Where(p => p.Selectable == true))
                {
                    Filter.FORM_Application_Status_ID.Add(item.ID);
                }

                Filter.FORM_Application_Status_ID.Add(FORMStatus.Comitted);

                Filter.FORM_Application_Priority_ID = new List<Guid>
                {
                    Guid.Parse("f1af3d5a-7a02-4faa-8845-34d2a5a66785"),
                    Guid.Parse("4318613b-17f7-4e15-a613-8801d4d5ae65"),
                    Guid.Parse("80dadb40-fa35-43eb-b2d5-871b8dc40913"),
                    Guid.Parse("f47f39b8-bfaf-4b71-9fd0-f3c978a4d1a4")
                };
            }

            HasCommitteeRights = AuthProvider.HasUserRole(AuthRoles.Committee);
            AllowedAuthorities = await GetAuthorities();

            //ListSettings = await GetOrCreateListSettings();
            Data = await GetData(Filter);

            var MunicipalApps = await AuthProvider.GetMunicipalityApps();

            if(MunicipalApps != null)
            {
                MunicipalityHasMantainances = MunicipalApps.Any(e => e.APP_Application_ID == Applications.Mantainences);
            }

            UpdateUnreadChatStatus();


            BusyIndicatorService.IsBusy = false;
            IsDataBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task<List<Guid>> GetAuthorities()
        {
            if (SessionWrapper.CurrentMunicipalUser != null)
            {
                var userAuthorities = await AuthProvider.GetMunicipalUserAuthorities(SessionWrapper.CurrentMunicipalUser.ID);

                return userAuthorities.Where(p => p.AUTH_Authority_ID != null).Select(p => p.AUTH_Authority_ID.Value).ToList();
            }

            return new List<Guid>();
        }
        private async Task<List<V_FORM_Application>> GetData(Administration_Filter_Item? Filter)
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                var applications = await FormApplicationProvider.GetApplications(SessionWrapper.AUTH_Municipality_ID.Value, SessionWrapper.CurrentMunicipalUser.ID, AllowedAuthorities, Filter);

                FormAdministrationHelper.Filter = Filter;
                        
                return applications;
            }

            return new List<V_FORM_Application>();
        }
        private async void OnChatStatusChange()
        {

            UpdateUnreadChatStatus();

        }

        private async Task UpdateUnreadChatStatus()
        {
            List<V_FORM_Application> applications = Data;
            List<V_CHAT_Unread_Messages_Responsible> unreadMessages = new List<V_CHAT_Unread_Messages_Responsible>();

            if (ChatNotificationService != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                unreadMessages.AddRange(ChatNotificationService.UnreadMessages.Where(p => p.AUTH_Municipality_ID == SessionWrapper.AUTH_Municipality_ID && p.ResponsibleMunicipalUserID == SessionWrapper.CurrentMunicipalUser.ID && p.AUTH_User_Type == "CitizenUser" && p.ContextType == "FormApplication"));
            }
            foreach (V_FORM_Application application in applications)
            {
                if (unreadMessages != null && unreadMessages.Any() && unreadMessages.Where(p => !string.IsNullOrEmpty(p.ContextElementId))
                                                                                    .Select(p => p.ContextElementId.ToLower().Trim()).Distinct().ToList().Contains(application.ID.ToString().ToLower().Trim()))
                {
                    application.HasUnreadChatMessages = true;
                }
                else
                {
                    application.HasUnreadChatMessages = false;
                }
            }

            Data = applications;

            if (Filter.OnlyNewChatMessages == true)
            {
                Data = Data.Where(p => p.HasUnreadChatMessages == true).ToList();
            }

            await InvokeAsync(StateHasChanged);
        }
        private void EditManualInput(Guid FORM_Application_ID)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Backend/Form/Application/" + FORM_Application_ID);
        }
        private void AddApplication()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Backend/Form/Application/New");
        }
        private void AddManteinance()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Backend/Form/Manteinance/New");
        }
        private async void FilterSearch(Administration_Filter_Item Filter)
        {
            IsDataBusy = true;
            StateHasChanged();

            this.Filter = Filter;

            Data = await GetData(this.Filter);
            

            UpdateUnreadChatStatus();


            IsDataBusy = false;
            StateHasChanged();
        }
        private void ShowDetailPage(Guid FORM_Application_ID)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Backend/Form/Detail/" + FORM_Application_ID);

        }
        private async Task<bool> OnRowClick(GridRowClickEventArgs Args)
        {
            var item = (V_FORM_Application)Args.Item; ;

            if (item != null)
            {
                if (item.IsManualInput == true && item.FORM_Application_Status_ID == FORMStatus.ToSign && item.ID != null) //TO SIGN
                {
                    EditManualInput(item.ID);
                }
                else if (item.ID != null) //COMITTED
                {
                    ShowDetailPage(item.ID);
                }
            }

            return true;
        } 
        private async Task OnStateInitHandler(GridStateEventArgs<V_FORM_Application> args)
        {
            try
            {
                var state = await SettProvider.GetUserState(SessionWrapper.CurrentMunicipalUser.ID);

                if (state != null && !string.IsNullOrEmpty(state.FORM_Administration_State))
                {
                    args.GridState = JsonSerializer.Deserialize<GridState<V_FORM_Application>>(state.FORM_Administration_State);
                }

            }
            catch { }
        }
        private async void OnStateChangedHandler(GridStateEventArgs<V_FORM_Application> args)
        {
            var state = await SettProvider.GetUserState(SessionWrapper.CurrentMunicipalUser.ID);
            var data = JsonSerializer.Serialize(args.GridState);

            if (state != null)
            {
                state.FORM_Administration_State = data;                

                await SettProvider.SetUserState(state);
            }
            else
            {
                state = new SETT_User_States();
                state.ID = Guid.NewGuid();
                state.AUTH_Users_ID = SessionWrapper.CurrentMunicipalUser.ID;
                state.FORM_Administration_State = data;

                await SettProvider.SetUserState(state);
            }
        }
        public void Dispose()
        {
            if (ChatNotificationService != null)
            {
                ChatNotificationService.OnMunicipalChatStatusChange -= OnChatStatusChange;
            }
        }
    }
}
