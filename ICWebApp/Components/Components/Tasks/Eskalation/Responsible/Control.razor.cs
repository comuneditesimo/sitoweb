using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Components.Components.Tasks.Eskalation.Responsible
{    public partial class Control : IDisposable
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }

        [Parameter] public Guid TaskEskalationID { get; set; }
        [Parameter] public List<TASK_Task_Eskalation_Responsible> ResponsibleList { get; set; }
        [Parameter] public EventCallback<TASK_Task_Eskalation_Responsible> ItemAdded { get; set; }
        [Parameter] public EventCallback<TASK_Task_Eskalation_Responsible> ItemRemoved { get; set; }

        private List<AUTH_Municipal_Users> AllResponsibles = new List<AUTH_Municipal_Users>();
        private List<AUTH_Municipal_Users> ResponsiblesToAdd = new List<AUTH_Municipal_Users>();
        private bool ResponsibleDropdownVisibility = false;
        private SearchInput? Search = new SearchInput();

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                AllResponsibles = await AuthProvider.GetMunicipalUserList(SessionWrapper.AUTH_Municipality_ID.Value, AuthRoles.Employee);    //EMPLOYEE
                ResponsiblesToAdd = AllResponsibles.Where(p => !ResponsibleList.Select(p => p.AUTH_Municipal_Users_ID).Contains(p.ID)).ToList();
            }
            EnviromentService.CloseAllCustomDropdowns += HideResponsibleDropdown;

            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void AddResponsible(AUTH_Municipal_Users Item)
        {
            var item = new TASK_Task_Eskalation_Responsible()
            {
                ID = Guid.NewGuid(),
                AUTH_Municipal_Users_ID = Item.ID,
                TASK_Task_Eskalation_ID = TaskEskalationID,
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
        public void Dispose()
        {
            EnviromentService.CloseAllCustomDropdowns -= HideResponsibleDropdown;
        }
    }
}
