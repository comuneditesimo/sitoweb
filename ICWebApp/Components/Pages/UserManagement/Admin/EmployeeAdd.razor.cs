using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;

namespace ICWebApp.Components.Pages.UserManagement.Admin
{
    public partial class EmployeeAdd
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IAccountService AccountService { get; set; }
        [Inject] IMSGProvider MSGProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string ID { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        private AUTH_Municipal_Users? Data { get; set; }
        private EditContext editContext { get; set; }
        private bool validEmail = true;
        private bool ShowWindow { get; set; } = false;
        private List<AUTH_Municipality> MunicipalityList = new List<AUTH_Municipality>();
        private List<AUTH_Authority> AuthorityList = new List<AUTH_Authority>();
        private AUTH_Municipal_Users_Authority? CurrentUserAuthority = null;
        private List<AUTH_Municipal_Users_Authority> UserAuthorities = new List<AUTH_Municipal_Users_Authority>();
        private List<FILE_FileInfo> ProfilePicture = new List<FILE_FileInfo>();
        private string validEmailCSS
        {
            get
            {
                if (!validEmail || (editContext != null && editContext.GetValidationMessages(new FieldIdentifier(Data, "DA_Email")).Count() > 0))
                {
                    return "outline: 1px solid red !important;";
                }

                return "";
            }
        }
        private string valEmail
        {
            get
            {
                return Data.DA_Email;
            }
            set
            {
                Data.DA_Email = value;
                editContext.Validate();
                ValidateEmail();
            }
        }
        private bool IsValidPassword = false;
        private PasswordHelper pwhelper = new PasswordHelper();
        private string PasswordQuality { get; set; } = "";
        private string Password
        {
            get
            {
                return Data.Password;
            }
            set
            {

                Data.Password = value;
                PasswordQuality = pwhelper.GetPasswordStrength(value).ToString();
                IsValidPassword = pwhelper.IsValidPassword(value);
                StateHasChanged();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            if (ID == "New")
            {
                Data = new AUTH_Municipal_Users();
                editContext = new EditContext(Data);
                Data.ID = Guid.NewGuid();
                Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
            }
            else
            {
                Data = await AuthProvider.GetMunicipalUser(Guid.Parse(ID));

                if (Data == null)
                {
                    BackToPreviousPage();
                }

                editContext = new EditContext(Data);

                UserAuthorities = await AuthProvider.GetMunicipalUserAuthorities(Data.ID);

                if (Data.Logo_FILE_FileInfo_ID != null)
                {
                    var FileInfo = await FileProvider.GetFileInfoAsync(Data.Logo_FILE_FileInfo_ID.Value);

                    if (FileInfo != null)
                    {
                        ProfilePicture = new List<FILE_FileInfo>() { FileInfo };
                    }
                }
            }

            AuthorityList = await AuthProvider.GetAuthorityList(SessionWrapper.AUTH_Municipality_ID.Value, null, null);

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async void HandleValidSubmit()
        {
            if (Data != null && validEmail && (IsValidPassword || (string.IsNullOrEmpty(Data.Password) && (string.IsNullOrEmpty(Data.ConfirmPassword)))))
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();

                if (ID == "New")
                {
                    if(Data.DA_Email != null)
                    {
                        Data.Email = Data.DA_Email;
                    }

                    Data.Username = Data.Email;

                    if (Data.Password != null && Data.ConfirmPassword != null)
                    {
                        PasswordHelper pwd = new PasswordHelper();

                        Data.PasswordHash = pwd.CreateMD5Hash(Data.Password);
                    }

                    await AuthProvider.UpdateMunicipalUser(Data);
                    await AuthProvider.SetMunicipaRole(Data.ID, AuthRoles.Employee);    //EMPLOYEE
                }
                else
                {
                    Data.Username = Data.Email;

                    if(Data.Password != null && Data.ConfirmPassword != null)
                    {
                        PasswordHelper pwd = new PasswordHelper();

                        Data.PasswordHash = pwd.CreateMD5Hash(Data.Password);
                        Data.LastLoginToken = Guid.NewGuid();
                    }

                    await AuthProvider.UpdateMunicipalUser(Data);
                }

                foreach(var r in UserAuthorities)
                {
                    await AuthProvider.SetMunicipalUserAuthority(r);                    
                }

                if (ProfilePicture != null && ProfilePicture.Count() > 0)
                {
                    var file = await FileProvider.SetFileInfo(ProfilePicture.FirstOrDefault());

                    if (file != null)
                    {
                        Data.Logo_FILE_FileInfo_ID = file.ID;
                        await AuthProvider.UpdateMunicipalUser(Data);
                    }
                }

                NavManager.NavigateTo("/User/Management");
            }
        }
        private void BackToPreviousPage()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/User/Management");
        }
        private void ValidateEmail()
        {
            validEmail = AccountService.IsEmailUnique(Data.DA_Email, Data.ID);
            StateHasChanged();
        }
        private void AddAuthority(AUTH_Authority Authority)
        {
            if (Authority != null)
            {
                CurrentUserAuthority = new AUTH_Municipal_Users_Authority();
                CurrentUserAuthority.ID = Guid.NewGuid();
                CurrentUserAuthority.AUTH_Municipal_Users_ID = Data.ID;
                CurrentUserAuthority.AUTH_Authority_ID = Authority.ID;
                CurrentUserAuthority.EnableNotifications = true;

                if (CurrentUserAuthority != null)
                {
                    var role = UserAuthorities.FirstOrDefault(p => p.ID == CurrentUserAuthority.ID);

                    if (role == null)
                    {
                        var selectedRole = AuthorityList.FirstOrDefault(p => p.ID == CurrentUserAuthority.AUTH_Authority_ID);

                        if (selectedRole != null)
                        {
                            CurrentUserAuthority.AuthorityName = selectedRole.Name;
                        }

                        UserAuthorities.Add(CurrentUserAuthority);
                    }
                    else if (role.Removed)
                    {
                        role.Removed = false;
                    }
                }

                StateHasChanged();
            }
        }
        private async void DeleteAuthority(AUTH_Municipal_Users_Authority target)
        {
            if (target != null)
            {
                target.Removed = true;
                await AuthProvider.RemoveMunicipalUserAuthority(Data.ID, target.AUTH_Authority_ID.Value);
                UserAuthorities.Remove(target);
                StateHasChanged();
            }
        }
        private async void RemoveImage(Guid File_Info_ID)
        {
            if (Data != null)
            {
                Data.Logo_FILE_FileInfo_ID = null;
                await FileProvider.RemoveFileInfo(File_Info_ID);

                StateHasChanged();
            }
        }
    }
}
