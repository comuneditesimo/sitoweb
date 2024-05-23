using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Person
{
    public partial class PersonMultipleSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Parameter] public List<Guid>? SelectedPeople { get; set; }
        [Parameter] public EventCallback<List<Guid>?> SelectedPeopleChanged { get; set; }

        private List<V_HOME_Person>? PersonList = null;
        private string? SearchText = null;
        private bool AddPersonVisible = false;
        private string? PersonEditTitle = null;
        private bool ShowPersonEdit = false;
        private string? PersonEditID = null;
        protected override async void OnInitialized()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
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
        private async void RemovePerson(V_HOME_Person person)
        {
            if (SelectedPeople != null) 
            {
                SelectedPeople.Remove(person.ID);
                await SelectedPeopleChanged.InvokeAsync(SelectedPeople);
                StateHasChanged();
            }
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
            StateHasChanged();
        }
        private async void SelectPerson(V_HOME_Person person)
        {
            if (SelectedPeople != null)
            {
                SelectedPeople.Add(person.ID);
                await SelectedPeopleChanged.InvokeAsync(SelectedPeople);
                AddPersonVisible = false;
                StateHasChanged();
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
