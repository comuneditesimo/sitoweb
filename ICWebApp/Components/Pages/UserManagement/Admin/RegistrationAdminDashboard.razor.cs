using Freshdesk;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;

namespace ICWebApp.Components.Pages.UserManagement.Admin
{
    public partial class RegistrationAdminDashboard
    {
        [Inject] public ITEXTProvider TextProvider { get; set; }
        [Inject] public IAUTHProvider AuthProvider { get; set; }
        [Inject] public IAccountService AccountService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private List<V_AUTH_Citizens> Data = new List<V_AUTH_Citizens>();
        private bool IsDataBusy { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_USER_REGISTRATION_MANAGEMENT");
            await GetData();

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }

        private async Task<bool> GetData()
        {
            if (SessionWrapper.CurrentMunicipalUser != null && SessionWrapper.CurrentMunicipalUser.AUTH_Municipality_ID != null)
            {
                Guid? municipalityID = SessionWrapper.CurrentMunicipalUser.AUTH_Municipality_ID.Value;

                var data = await AccountService.GetRegistrationAdminList(municipalityID);   //EMPLOYEE
                Data = data.OrderByDescending(o=>o.LastLoginTimeStamp).ToList();
            }

            return true;
        }
     
        private async void ConfirmEmail(V_AUTH_Citizens Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("CONFIRM_EMAIL_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();
                var selecteduser = await AuthProvider.GetUser(Item.ID);
                selecteduser.EmailConfirmed = true;
                await AuthProvider.UpdateUser(selecteduser);

                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }

        private async void ConfirmPhone(V_AUTH_Citizens Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("CONFIRM_PHONE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;

                var selecteduser = await AuthProvider.GetUser(Item.ID);
                selecteduser.PhoneNumberConfirmed = true;
                await AuthProvider.UpdateUser(selecteduser);

                StateHasChanged();

                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }



        private async void ResendSMS(V_AUTH_Citizens Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("SEND_SMS_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();
                var selecteduser = await AuthProvider.GetUser(Item.ID);
                AccountService.SendVerificationSMS(selecteduser);
                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }


    }
}