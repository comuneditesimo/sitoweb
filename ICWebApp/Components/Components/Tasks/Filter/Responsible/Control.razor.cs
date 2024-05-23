using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Tasks.Filter.Responsible
{    public partial class Control : IDisposable
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }

        [Parameter] public List<Guid> AUTH_Users_List { get; set; }
        [Parameter] public EventCallback<Guid> ItemAdded { get; set; }
        [Parameter] public EventCallback<Guid> ItemRemoved { get; set; }

        private List<AUTH_Municipal_Users> AllResponsibles = new List<AUTH_Municipal_Users>();
        private List<AUTH_Municipal_Users> ResponsiblesToAdd = new List<AUTH_Municipal_Users>();
        private bool ResponsibleDropdownVisibility = false;
        private SearchInput? Search = new SearchInput();

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                AllResponsibles = await AuthProvider.GetMunicipalUserList(SessionWrapper.AUTH_Municipality_ID.Value, AuthRoles.Employee);    //EMPLOYEE
                ResponsiblesToAdd = AllResponsibles.Where(p => !AUTH_Users_List.Contains(p.ID)).ToList();
            }
            EnviromentService.CloseAllCustomDropdowns += HideResponsibleDropdown;

            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void AddResponsible(AUTH_Municipal_Users Item)
        {
            AUTH_Users_List.Add(Item.ID);
            ResponsiblesToAdd = AllResponsibles.Where(p => !AUTH_Users_List.Contains(p.ID)).ToList();

            HideResponsibleDropdown();

            await ItemAdded.InvokeAsync(Item.ID);
            StateHasChanged();
        }
        private async void RemoveResponsible(AUTH_Municipal_Users Item)
        {
            var tagToRemove = AUTH_Users_List.FirstOrDefault(p => p == Item.ID);

            if(tagToRemove != null)
            {
                AUTH_Users_List.Remove(tagToRemove);
            }

            ResponsiblesToAdd = AllResponsibles.Where(p => !AUTH_Users_List.Contains(p.ID)).ToList();

            await ItemRemoved.InvokeAsync(tagToRemove);
            StateHasChanged();
        }
        private void ToggleResponsibleDropdown()
        {
            if (ResponsibleDropdownVisibility != true)
            {
                EnviromentService.NotifyCloseAllCustomDropdowns();
                ResponsibleDropdownVisibility = true;
            }
            else
            {
                ResponsibleDropdownVisibility = false;
            }
            StateHasChanged();
        }
        private void HideResponsibleDropdown()
        {
            ResponsibleDropdownVisibility = false;
            StateHasChanged();
        }
        public void Dispose()
        {
            EnviromentService.CloseAllCustomDropdowns -= HideResponsibleDropdown;
        }
    }
}
