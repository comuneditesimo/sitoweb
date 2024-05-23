using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Syncfusion.Blazor.Popups;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Pages.Homepage.Backend.Location
{
    public partial class Edit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_Location? Data;
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
        private List<V_HOME_Location_Type> Types = new List<V_HOME_Location_Type>();
        private List<Guid> SelectedPeople { get; set; }
        private List<Guid> SelectedThemes { get; set; }
        private List<Guid> SelectedDocuments { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Locations", "MAINMENU_BACKEND_HOMEPAGE_LOCATIONS", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_LOCATIONS_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Locations/Edit", "MAINMENU_BACKEND_HOMEPAGE_LOCATIONS_EDIT", null, null, true);

                    Data = await HomeProvider.GetLocation(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Location_Extended = await HomeProvider.GetLocation_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Location_Extended != null && Data.HOME_Location_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Location_Extended == null)
                                    {
                                        Data.HOME_Location_Extended = new List<HOME_Location_Extended>();
                                    }

                                    if (Data.HOME_Location_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Location_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Location_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetLocationExtended(dataE);
                                        Data.HOME_Location_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_LOCATIONS_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Locations/Edit", "MAINMENU_BACKEND_HOMEPAGE_LOCATIONS_NEW", null, null, true);

                    Data = new HOME_Location();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Location_Extended == null)
                            {
                                Data.HOME_Location_Extended = new List<HOME_Location_Extended>();
                            }

                            if (Data.HOME_Location_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Location_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Location_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Location_Extended.Add(dataE);
                            }
                        }
                    }
                }

                Types = await HomeProvider.GetLocation_Type(LangProvider.GetCurrentLanguageID());
            }

            if(Data != null)
            {
                var people = await HomeProvider.GetLocation_People(Data.ID);

                SelectedPeople = people.Where(p => p.HOME_Person_ID != null).Select(p => p.HOME_Person_ID.Value).ToList();

                var themes = await HomeProvider.GetLocation_Themes(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();

                if (Data.FILE_FileImage_ID != null)
                {
                    Data.Image = await FileProvider.GetFileInfoAsync(Data.FILE_FileImage_ID.Value);
                }

                var documents = await HomeProvider.GetLocation_Documents(Data.ID);

                SelectedDocuments = documents.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID.Value).ToList();
            }
            else
            {
                SelectedPeople = new List<Guid>();
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

                if (Data.Image != null)
                {
                    await FileProvider.SetFileInfo(Data.Image);

                    Data.FILE_FileImage_ID = Data.Image.ID;
                }
                else
                {
                    if (Data.FILE_FileImage_ID != null)
                    {
                        await FileProvider.RemoveFileInfo(Data.FILE_FileImage_ID.Value);

                        Data.Image = null;
                    }
                }

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckLocatinHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await HomeProvider.SetLocation(Data);

                if (Data.HOME_Location_Extended != null)
                {
                    foreach (var ext in Data.HOME_Location_Extended)
                    {
                        await HomeProvider.SetLocationExtended(ext);
                    }
                }

                var existingPeople = await HomeProvider.GetLocation_People(Data.ID);

                if (existingPeople != null && SelectedPeople != null)
                {
                    var peopleToDelete = existingPeople.Where(p => p.HOME_Person_ID != null && !SelectedPeople.Contains(p.HOME_Person_ID.Value)).ToList();
                    var peopleToAdd = SelectedPeople.Where(p => !existingPeople.Where(p => p.HOME_Person_ID != null).Select(p => p.HOME_Person_ID).Contains(p)).ToList();


                    foreach(var p in peopleToDelete)
                    {
                        await HomeProvider.RemoveLocationPerson(p);
                    }

                    foreach(var p in peopleToAdd)
                    {
                        var newPerson = new HOME_Location_Person();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Person_ID = p;
                        newPerson.HOME_Location_ID = Data.ID;

                        await HomeProvider.SetLocationPerson(newPerson);
                    }
                }

                var existingThemes = await HomeProvider.GetLocation_Themes(Data.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();

                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveLocationTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new HOME_Location_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.HOME_Location_ID = Data.ID;

                        await HomeProvider.SetLocationTheme(newPerson);
                    }
                }

                var existingDocuments = await HomeProvider.GetLocation_Documents(Data.ID);

                if(existingDocuments != null && SelectedDocuments != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.HOME_Document_ID != null && !SelectedDocuments.Contains(p.HOME_Document_ID.Value)).ToList();
                    var docsToAdd = SelectedDocuments.Where(p => !existingDocuments.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID).Contains(p)).ToList();

                    foreach (var t in docsToDelete)
                    {
                        await HomeProvider.RemoveLocationDocument(t);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Location_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Document_ID = t;
                        newItem.HOME_Location_ID = Data.ID;

                        await HomeProvider.SetLocationDocument(newItem);
                    }
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Locations");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Locations");
            StateHasChanged();
        }
    }
}
