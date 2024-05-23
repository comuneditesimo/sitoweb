﻿using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Person
{
    public partial class PersonSingleSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Parameter] public Guid? SelectedPersonID { get; set; }
        [Parameter] public EventCallback<Guid?> SelectedPersonIDChanged { get; set; }

        private List<V_HOME_Person>? PersonList = null;
        private V_HOME_Person? SelectedPerson = null;
        private Guid ContainerGuid = Guid.NewGuid();
        private string? SearchText = null;
        private string? PersonEditTitle = null;
        private bool ShowPersonEdit = false;
        private string? PersonEditID = null;
        private bool AddPersonVisible = false;
        protected override async void OnInitialized()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                PersonList = await HOMEProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            if(SelectedPerson != null && PersonList != null)
            {
                SelectedPerson = PersonList.FirstOrDefault(p => p.ID == SelectedPersonID);
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

            if (PersonList != null) 
            {
                SelectedPerson = PersonList.FirstOrDefault(p => p.ID == SelectedPersonID);
            }
            StateHasChanged();
            base.OnParametersSet();
        }
        private async void PersonSelected(Guid PersonID)
        {
            await JSRuntime.InvokeVoidAsync("utility_HideContainer", "container-" + ContainerGuid.ToString());
            await JSRuntime.InvokeVoidAsync("utility_HideContainer", "overlay-" + ContainerGuid.ToString());
        }
        private async void ClearSelectedPerson()
        {
            SelectedPerson = null;
            SelectedPersonID = null;
            await SelectedPersonIDChanged.InvokeAsync(null);
            StateHasChanged();
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

            if (PersonList != null)
            {
                SelectedPerson = PersonList.FirstOrDefault(p => p.ID == SelectedPersonID);
            }

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
            StateHasChanged();
        }
        private async void SelectPerson(V_HOME_Person person)
        {
            SelectedPerson = person;
            SelectedPersonID = person.ID;

            await SelectedPersonIDChanged.InvokeAsync(SelectedPersonID);
            AddPersonVisible = false;
            StateHasChanged();
        }
    }
}
