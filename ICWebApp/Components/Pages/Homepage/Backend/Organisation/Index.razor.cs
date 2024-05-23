using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using Syncfusion.Blazor.Popups;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;

namespace ICWebApp.Components.Pages.Homepage.Backend.Organisation
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private bool IsDataBusy { get; set; } = true;
        private List<V_HOME_Organisation> Data = new List<V_HOME_Organisation>();
        private List<V_HOME_Organisation_Type> TypeData = new List<V_HOME_Organisation_Type>();
        private bool ShowTypeWindow = false;
        private bool ShowTypeEditWindow = false;
        private string? TypeEditWindowTitle = null;
        private HOME_Organisation_Type? TypeItem;
        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private List<IEditorTool> Tools { get; set; } =
           new List<IEditorTool>()
           {
                new EditorButtonGroup(new Bold(), new Italic(), new Underline()),
                new EditorButtonGroup(new AlignLeft(), new AlignCenter(), new AlignRight()),
                new UnorderedList(),
                new EditorButtonGroup(new CreateLink(), new Unlink()),
                new InsertTable(),
                new DeleteTable(),
                new EditorButtonGroup(new AddRowBefore(), new AddRowAfter(), new DeleteRow(), new MergeCells(), new SplitCell())
           };
        public List<string> RemoveAttributes { get; set; } = new List<string>() { "data-id" };
        public List<string> StripTags { get; set; } = new List<string>() { "font" };

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ORGANISATIONS");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Organisations", "MAINMENU_BACKEND_HOMEPAGE_ORGANISATIONS", null, null, true);

            await GetData();

            Languages = await LangProvider.GetAll();

            if (CurrentLanguage == null)
                CurrentLanguage = LanguageSettings.German;

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data = await HomeProvider.GetOrganisations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void Edit(V_HOME_Organisation Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Organisations/Edit/" + Item.ID.ToString());
            StateHasChanged();
        }
        private async void Delete(V_HOME_Organisation Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_ORGANISATION"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();

                await HomeProvider.RemoveTheme(Item.ID);
                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }
        private void New()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Organisations/Edit/New");
            StateHasChanged();
        }
        private async void ManageTypes()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null) 
            {
                TypeData = await HomeProvider.GetOrganisation_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                ShowTypeWindow = true;
                StateHasChanged();
            }
        }
        private void CloseManageTypes()
        {
            ShowTypeWindow = false;
            StateHasChanged();
        }
        private void NewType()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                TypeEditWindowTitle = TextProvider.Get("BACKEND_HOMEPAGE_ORG_TYPES_NEW");

                TypeItem = new HOME_Organisation_Type();

                TypeItem.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                if (Languages != null)
                {
                    foreach (var l in Languages)
                    {
                        if (TypeItem.HOME_Organisation_Type_Extended == null)
                        {
                            TypeItem.HOME_Organisation_Type_Extended = new List<HOME_Organisation_Type_Extended>();
                        }

                        if (TypeItem.HOME_Organisation_Type_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                        {
                            var dataE = new HOME_Organisation_Type_Extended()
                            {
                                ID = Guid.NewGuid(),
                                HOME_Organisation_Type_ID = TypeItem.ID,
                                LANG_Language_ID = l.ID
                            };

                            TypeItem.HOME_Organisation_Type_Extended.Add(dataE);
                        }
                    }
                }

                ShowTypeEditWindow = true;
                StateHasChanged();
            }
        }
        private async void EditType(V_HOME_Organisation_Type Item)
        {
            TypeItem = await HomeProvider.GetOrganisationType(Item.ID);

            if (TypeItem != null)
            {
                TypeEditWindowTitle = TextProvider.Get("BACKEND_HOMEPAGE_ORG_TYPES_EDIT");
                TypeItem.HOME_Organisation_Type_Extended = await HomeProvider.GetOrganisation_Type_Extended(TypeItem.ID);
            }

            ShowTypeEditWindow = true;
            StateHasChanged();
        }
        private async void SaveType()
        {
            if(TypeItem != null)
            {
                await HomeProvider.SetOrganisationType(TypeItem);

                if (TypeItem.HOME_Organisation_Type_Extended != null)
                {
                    foreach (var ext in TypeItem.HOME_Organisation_Type_Extended)
                    {
                        await HomeProvider.SetOrganisationTypeExtended(ext);
                    }
                }
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                TypeData = await HomeProvider.GetOrganisation_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            TypeItem = null;
            ShowTypeEditWindow = false;
            StateHasChanged();
        }
        private void CancelEditType()
        {
            TypeItem = null;
            ShowTypeEditWindow = false;
            StateHasChanged();
        }
        private async void DeleteType(V_HOME_Organisation_Type Item)
        {
            if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_ORGANISATION_TYPE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                return;

            try
            {
                await HomeProvider.RemoveOrganisationType(Item.ID);
            }
            catch
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_ORGANISATION_TYPE_IN_USE"), TextProvider.Get("WARNING")))
                    return;
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                TypeData = await HomeProvider.GetOrganisation_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
        }
    }
}
