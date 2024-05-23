using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Person.Organisations
{
    public partial class PersonMultipleSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Parameter] public HOME_Organisation? Organisation {  get; set; }
        [Parameter] public List<HOME_Organisation_Person>? SelectedPeople { get; set; }
        [Parameter] public EventCallback<List<HOME_Organisation_Person>?> SelectedPeopleChanged { get; set; }

        private List<V_HOME_Organisation_Person_Type>? Types;
        private List<V_HOME_Person>? PersonList = null;
        private string? SearchText = null;
        private bool AddPersonVisible = false;
        private bool AddPersonTypeVisible = false;
        private bool IsTypeEdit = false;
        private HOME_Organisation_Person? CurrentPerson;
        private string? PersonEditTitle = null;
        private bool ShowPersonEdit = false;
        private string? PersonEditID = null;

        protected override async void OnInitialized()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                Types = await HOMEProvider.GetOrganisationPeopleTypes(LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnInitialized();
        }
        protected override async void OnParametersSet()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && PersonList == null)
            {
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnParametersSet();
        }     
        private async void RemovePerson(HOME_Organisation_Person person)
        {
            if (SelectedPeople != null) 
            {
                var item = SelectedPeople.FirstOrDefault(p => p.HOME_Person_ID == person.HOME_Person_ID);

                if (item != null)
                {
                    SelectedPeople.Remove(item);
                    await SelectedPeopleChanged.InvokeAsync(SelectedPeople);
                }

                StateHasChanged();
            }
        }
        private void EditPerson(HOME_Organisation_Person person)
        {
            CurrentPerson = person;
            AddPersonTypeVisible = true;
            IsTypeEdit = true;
            StateHasChanged();
        }
        private void AddPerson()
        {
            SearchText = null;
            AddPersonVisible = true;
            StateHasChanged();
        }
        private void HideAddPerson()
        {
            AddPersonVisible = false;
            AddPersonTypeVisible = false;
            CurrentPerson = null;
            StateHasChanged();
        }
        private void SelectPerson(V_HOME_Person person)
        {
            if (SelectedPeople != null && Organisation != null)
            {
                CurrentPerson = new HOME_Organisation_Person()
                {
                    HOME_Organisation_ID = Organisation.ID,
                    HOME_Person_ID = person.ID,
                    SortOrder = SelectedPeople.Count() + 1
                };

                IsTypeEdit = false;
                AddPersonVisible = false;
                AddPersonTypeVisible = true;
                StateHasChanged();
            }
        }
        private async void AddPersonType()
        {
            if (CurrentPerson != null && SelectedPeople != null)
            {
                if (!IsTypeEdit)
                {
                    SelectedPeople.Add(CurrentPerson);
                }

                await SelectedPeopleChanged.InvokeAsync(SelectedPeople);
            }

            AddPersonTypeVisible = false;
            StateHasChanged();
        }
        private void MoveUp(HOME_Organisation_Person item)
        {
            if(SelectedPeople != null) 
            {
                int? currentOrder = item.SortOrder;

                var otherItem = SelectedPeople.FirstOrDefault(p => p.SortOrder == item.SortOrder - 1);

                if (otherItem != null)
                {
                    otherItem.SortOrder = item.SortOrder;
                    item.SortOrder = item.SortOrder - 1;

                    StateHasChanged();
                }
            }
        }
        private void MoveDown(HOME_Organisation_Person item)
        {
            if (SelectedPeople != null)
            {
                int? currentOrder = item.SortOrder;

                var otherItem = SelectedPeople.FirstOrDefault(p => p.SortOrder == item.SortOrder + 1);

                if (otherItem != null)
                {
                    otherItem.SortOrder = item.SortOrder;
                    item.SortOrder = item.SortOrder + 1;

                    StateHasChanged();
                }
            }
        }
        private void New()
        {
            ShowPersonEdit = true;
            PersonEditID = "New";
            PersonEditTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PERSON_NEW");
            StateHasChanged();
        }
        private void Edit(Guid HOME_Person_ID)
        {
            ShowPersonEdit = true;
            PersonEditID = HOME_Person_ID.ToString();
            PersonEditTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PERSON_EDIT");
            StateHasChanged();
        }
        private void Cancel()
        {
            ShowPersonEdit = false;
            PersonEditID = null;
            PersonEditTitle = null;
            StateHasChanged();
        }
        private async void Save()
        {
            ShowPersonEdit = false;
            PersonEditID = null;
            PersonEditTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
        }
    }
}
