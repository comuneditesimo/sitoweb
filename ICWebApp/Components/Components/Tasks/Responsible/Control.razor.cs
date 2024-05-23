using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Tasks.Responsible
{    public partial class Control : IDisposable
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }

        [Parameter] public Guid TaskID { get; set; }
        [Parameter] public List<TASK_Task_Responsible> ResponsibleList { get; set; }
        [Parameter] public EventCallback<TASK_Task_Responsible> ItemAdded { get; set; }
        [Parameter] public EventCallback<TASK_Task_Responsible> ItemRemoved { get; set; }
        [Parameter] public EventCallback OverlayClicked { get; set; }
        [Parameter] public bool ShowInputField { get; set; } = true;

        private List<AUTH_Municipal_Users> AllResponsibles = new List<AUTH_Municipal_Users>();
        private List<AUTH_Municipal_Users> ResponsiblesToAdd = new List<AUTH_Municipal_Users>();
        private bool ResponsibleDropdownVisibility = false;
        private SearchInput? Search = new SearchInput();

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && ResponsibleList != null)
            {
                AllResponsibles = await AuthProvider.GetMunicipalUserList(SessionWrapper.AUTH_Municipality_ID.Value, AuthRoles.Employee);    //EMPLOYEE
                ResponsiblesToAdd = AllResponsibles.Where(p => !ResponsibleList.Select(p => p.AUTH_Municipal_Users_ID).Contains(p.ID)).ToList();
            }
            EnviromentService.CloseAllCustomDropdowns += HideResponsibleDropdown;

            StateHasChanged();
            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && ResponsibleList != null && (ResponsiblesToAdd == null || ResponsiblesToAdd.Count() == 0))
            {
                AllResponsibles = await AuthProvider.GetMunicipalUserList(SessionWrapper.AUTH_Municipality_ID.Value, AuthRoles.Employee);    //EMPLOYEE
                ResponsiblesToAdd = AllResponsibles.Where(p => !ResponsibleList.Select(p => p.AUTH_Municipal_Users_ID).Contains(p.ID)).ToList();
            }

            await base.OnParametersSetAsync();
        }
        private async void AddResponsible(AUTH_Municipal_Users Item)
        {
            var item = new TASK_Task_Responsible()
            {
                ID = Guid.NewGuid(),
                AUTH_Municipal_Users_ID = Item.ID,
                TASK_Task_ID = TaskID,
                SortDesc = Item.Lastname + " " + Item.Firstname
            };

            ResponsibleList.Add(item);
            ResponsiblesToAdd = AllResponsibles.Where(p => !ResponsibleList.Select(p => p.AUTH_Municipal_Users_ID).Contains(p.ID)).ToList();

            HideResponsibleDropdown();

            await ItemAdded.InvokeAsync(item);
            StateHasChanged();
        }
        private async void RemoveResponsible(AUTH_Municipal_Users Item)
        {
            var tagToRemove = ResponsibleList.FirstOrDefault(p => p.AUTH_Municipal_Users_ID == Item.ID);

            if(tagToRemove != null)
            {
                ResponsibleList.Remove(tagToRemove);
            }

            ResponsiblesToAdd = AllResponsibles.Where(p => !ResponsibleList.Select(p => p.AUTH_Municipal_Users_ID).Contains(p.ID)).ToList();

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
        private void InvokeOverlayClicked()
        {
            OverlayClicked.InvokeAsync();
            StateHasChanged();
        }
        public void Dispose()
        {
            EnviromentService.CloseAllCustomDropdowns -= HideResponsibleDropdown;
        }
    }
}
