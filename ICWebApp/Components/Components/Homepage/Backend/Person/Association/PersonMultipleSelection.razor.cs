using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Person.Association
{
    public partial class PersonMultipleSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Parameter] public HOME_Association? Association {  get; set; }
        [Parameter] public List<HOME_Association_Person>? SelectedPeople { get; set; }
        [Parameter] public EventCallback<List<HOME_Association_Person>?> SelectedPeopleChanged { get; set; }

        private Guid? CurrentLanguage;
        private List<LANG_Languages>? Languages;
        private List<V_HOME_Association_Person_Type>? Types;
        private List<V_HOME_Person>? PersonList = null;
        private string? SearchText = null;
        private bool AddPersonVisible = false;
        private bool AddPersonTypeVisible = false;
        private bool IsTypeEdit = false;
        private HOME_Association_Person? CurrentPerson;
        private string? PersonEditTitle = null;
        private bool ShowPersonEdit = false;
        private string? PersonEditID = null;

        protected override async void OnInitialized()
        {
            Languages = await LangProvider.GetAll();

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                Types = await HOMEProvider.GetAssosiactionPeopleTypes(LangProvider.GetCurrentLanguageID());
            }

            if (CurrentLanguage == null)
                CurrentLanguage = LanguageSettings.German;

            StateHasChanged();
            base.OnInitialized();
        }
        protected override async void OnParametersSet()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && PersonList == null)
            {
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnParametersSet();
        }     
        private async void RemovePerson(HOME_Association_Person person)
        {
            if (SelectedPeople != null && person != null) 
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
        private async void EditPerson(HOME_Association_Person person)
        {
            CurrentPerson = person;

            person.HOME_Association_Person_Extended = await HOMEProvider.GetAssociationsPersonExtended(person.ID);

            if (person.HOME_Association_Person_Extended == null)
            {
                person.HOME_Association_Person_Extended = new List<HOME_Association_Person_Extended>();
            }

            if (Languages != null)
            {
                if (person.HOME_Association_Person_Extended != null && person.HOME_Association_Person_Extended.Count < Languages.Count)
                {
                    foreach (var l in Languages)
                    {
                        if (person.HOME_Association_Person_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                        {
                            var dataE = new HOME_Association_Person_Extended()
                            {
                                ID = Guid.NewGuid(),
                                HOME_Association_Person_ID = person.ID,
                                LANG_Language_ID = l.ID
                            };

                            person.HOME_Association_Person_Extended.Add(dataE);
                        }
                    }
                }
            }        

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
            if (SelectedPeople != null && Association != null)
            {
                CurrentPerson = new HOME_Association_Person()
                {
                    HOME_Association_ID = Association.ID,
                    HOME_Person_ID = person.ID,
                    SortOrder = SelectedPeople.Count() + 1
                };

                if (Languages != null)
                {
                    if (CurrentPerson.HOME_Association_Person_Extended != null && CurrentPerson.HOME_Association_Person_Extended.Count < Languages.Count)
                    {
                        foreach (var l in Languages)
                        {
                            if (CurrentPerson.HOME_Association_Person_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Association_Person_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Association_Person_ID = CurrentPerson.ID,
                                    LANG_Language_ID = l.ID
                                };

                                CurrentPerson.HOME_Association_Person_Extended.Add(dataE);
                            }
                        }
                    }
                }

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
        private void MoveUp(HOME_Association_Person item)
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
        private void MoveDown(HOME_Association_Person item)
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
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
        }
    }
}
