using AdobeSign.Rest.Model.LibraryDocuments;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;
using Syncfusion.Blazor.Popups;

namespace ICWebApp.Components.Pages.Homepage.Backend.Organisation
{
    public partial class Edit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] SfDialogService Dialogs { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Parameter] public string ID { get; set; }

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_Organisation? Data;
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
        private List<V_HOME_Organisation_Type> Types = new List<V_HOME_Organisation_Type>();
        private List<HOME_Organisation_Person> SelectedPeople { get; set; }
        private List<Guid> SelectedThemes { get; set; }
        private List<Guid> SelectedDocuments { get; set; }
        private FILE_FileInfo? ExistingFile { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Organisations", "MAINMENU_BACKEND_HOMEPAGE_ORGANISATIONS", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ORGANISATIONS_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Organisations/Edit", "MAINMENU_BACKEND_HOMEPAGE_ORGANISATIONS_EDIT", null, null, true);

                    Data = await HomeProvider.GetOrganisation(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Organisation_Extended = await HomeProvider.GetOrganisation_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Organisation_Extended != null && Data.HOME_Organisation_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Organisation_Extended == null)
                                    {
                                        Data.HOME_Organisation_Extended = new List<HOME_Organisation_Extended>();
                                    }

                                    if (Data.HOME_Organisation_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Organisation_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Organisation_ID = Data.ID,
                                            LANG_Languages_ID = l.ID
                                        };

                                        await HomeProvider.SetOrganisationExtended(dataE);
                                        Data.HOME_Organisation_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ORGANISATIONS_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Organisations/Edit", "MAINMENU_BACKEND_HOMEPAGE_ORGANISATIONS_EDIT", null, null, true);

                    Data = new HOME_Organisation();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Organisation_Extended == null)
                            {
                                Data.HOME_Organisation_Extended = new List<HOME_Organisation_Extended>();
                            }

                            if (Data.HOME_Organisation_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Organisation_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Organisation_ID = Data.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                Data.HOME_Organisation_Extended.Add(dataE);
                            }
                        }
                    }
                }

                Types = await HomeProvider.GetOrganisation_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            if(Data != null)
            {
                SelectedPeople = await HomeProvider.GetOrganisation_People(Data.ID);

                var themes = await HomeProvider.GetOrganisation_Themes(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();

                var documents = await HomeProvider.GetVenue_Documents(Data.ID);

                SelectedDocuments = documents.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID.Value).ToList();

                if (Data.FILE_FileInfo_ID != null)
                {
                    ExistingFile = await FileProvider.GetFileInfoAsync(Data.FILE_FileInfo_ID.Value);
                }
            }
            else
            {
                SelectedPeople = new List<HOME_Organisation_Person>();
                SelectedThemes = new List<Guid>();
                SelectedDocuments = new List<Guid>();
            }

            if (CurrentLanguage == null)
                CurrentLanguage = LanguageSettings.German;

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void Save()
        {
            if (Data != null)
            {
                Data.LastChangeDate = DateTime.Now;

                if (ExistingFile != null)
                {
                    await FileProvider.SetFileInfo(ExistingFile);

                    Data.FILE_FileInfo_ID = ExistingFile.ID;
                }
                else
                {
                    if (Data.FILE_FileInfo_ID != null)
                    {
                        await FileProvider.RemoveFileInfo(Data.FILE_FileInfo_ID.Value);

                        Data.FILE_FileInfo_ID = null;
                    }
                }

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckOrganisationHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await HomeProvider.SetOrganisation(Data);

                if (Data.HOME_Organisation_Extended != null)
                {
                    foreach (var ext in Data.HOME_Organisation_Extended)
                    {
                        await HomeProvider.SetOrganisationExtended(ext);
                    }
                }

                var existingPeople = await HomeProvider.GetOrganisation_People(Data.ID);

                if (existingPeople != null && SelectedPeople != null)
                {
                    var peopleToDelete = existingPeople.Where(p => p.HOME_Person_ID != null && !SelectedPeople.Select(p => p.HOME_Person_ID).Contains(p.HOME_Person_ID.Value)).ToList();
                    var peopleToAdd = SelectedPeople.Where(p => !existingPeople.Where(p => p.HOME_Person_ID != null).Select(p => p.HOME_Person_ID).Contains(p.HOME_Person_ID)).ToList();


                    foreach(var p in peopleToDelete)
                    {
                        await HomeProvider.RemoveOrganisationPerson(p);
                    }

                    foreach(var p in peopleToAdd)
                    {
                        await HomeProvider.SetOrganisationPerson(p);
                    }

                    foreach(var p in SelectedPeople)
                    {
                        await HomeProvider.SetOrganisationPerson(p);
                    }
                }

                var existingThemes = await HomeProvider.GetOrganisation_Themes(Data.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();


                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveOrganisationTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new HOME_Organisation_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.HOME_Organisation_ID = Data.ID;

                        await HomeProvider.SetOrganisationTheme(newPerson);
                    }
                }

                var existingDocuments = await HomeProvider.GetOrganisation_Documents(Data.ID);

                if (existingDocuments != null && SelectedDocuments != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.HOME_Document_ID != null && !SelectedDocuments.Contains(p.HOME_Document_ID.Value)).ToList();
                    var docsToAdd = SelectedDocuments.Where(p => !existingDocuments.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID).Contains(p)).ToList();

                    foreach (var t in docsToDelete)
                    {
                        await HomeProvider.RemoveOrganisationDocument(t);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Organisation_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Document_ID = t;
                        newItem.HOME_Organisation_ID = Data.ID;

                        await HomeProvider.SetOrganisationDocument(newItem);
                    }
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Organisations");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Organisations");
            StateHasChanged();
        }
    }
}
