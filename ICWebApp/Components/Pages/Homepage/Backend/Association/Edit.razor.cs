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

namespace ICWebApp.Components.Pages.Homepage.Backend.Association
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
        private HOME_Association? Data;
        private List<HOME_Association_Person> SelectedPeople { get; set; }
        private List<Guid> SelectedThemes { get; set; }
        private List<Guid> SelectedDocuments { get; set; }
        private FILE_FileInfo? ExistingFile { get; set; }
        private List<V_HOME_Association_Type>? Types;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Associations", "MAINMENU_BACKEND_HOMEPAGE_ASSOCIATIONS", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ASSOCIATIONS_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Associations/Edit", "MAINMENU_BACKEND_HOMEPAGE_ASSOCIATIONS_EDIT", null, null, true);

                    Data = await HomeProvider.GetAssociation(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Association_Extended = await HomeProvider.GetAssociation_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Association_Extended != null && Data.HOME_Association_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Association_Extended == null)
                                    {
                                        Data.HOME_Association_Extended = new List<HOME_Association_Extended>();
                                    }

                                    if (Data.HOME_Association_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Association_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Association_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetAssociationExtended(dataE);
                                        Data.HOME_Association_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ASSOCIATIONS_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Associations/Edit", "MAINMENU_BACKEND_HOMEPAGE_ASSOCIATIONS_NEW", null, null, true);

                    Data = new HOME_Association();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Association_Extended == null)
                            {
                                Data.HOME_Association_Extended = new List<HOME_Association_Extended>();
                            }

                            if (Data.HOME_Association_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Association_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Association_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Association_Extended.Add(dataE);
                            }
                        }
                    }
                }
            }

            Types = await HomeProvider.GetAssociation_Type(LangProvider.GetCurrentLanguageID());

            if (Data != null)
            {
                SelectedPeople = await HomeProvider.GetAssociation_People(Data.ID);

                var themes = await HomeProvider.GetAssociation_Themes(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();

                var documents = await HomeProvider.GetAssociation_Documents(Data.ID);

                SelectedDocuments = documents.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID.Value).ToList();

                if (Data.FILE_FileInfo_ID != null)
                {
                    ExistingFile = await FileProvider.GetFileInfoAsync(Data.FILE_FileInfo_ID.Value);
                }
            }
            else
            {
                SelectedPeople = new List<HOME_Association_Person>();
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
                    var allowed = await HomeProvider.CheckAssociationHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await HomeProvider.SetAssociation(Data);

                if (Data.HOME_Association_Extended != null)
                {
                    foreach (var ext in Data.HOME_Association_Extended)
                    {
                        await HomeProvider.SetAssociationExtended(ext);
                    }
                }

                var existingPeople = await HomeProvider.GetAssociation_People(Data.ID);

                if (existingPeople != null && SelectedPeople != null)
                {
                    var peopleToDelete = existingPeople.Where(p => p.HOME_Person_ID != null && !SelectedPeople.Select(p => p.HOME_Person_ID).Contains(p.HOME_Person_ID.Value)).ToList();
                    var peopleToAdd = SelectedPeople.Where(p => !existingPeople.Where(p => p.HOME_Person_ID != null).Select(p => p.HOME_Person_ID).Contains(p.HOME_Person_ID)).ToList();


                    foreach (var p in peopleToDelete)
                    {
                        await HomeProvider.RemoveAssociationPerson(p);
                    }

                    foreach (var p in peopleToAdd)
                    {
                        await HomeProvider.SetAssociationPerson(p);

                        if (p.HOME_Association_Person_Extended != null)
                        {
                            foreach (var ext in p.HOME_Association_Person_Extended)
                            {
                                await HomeProvider.SetAssociationPersonExtended(ext);
                            }
                        }
                    }

                    foreach (var p in SelectedPeople)
                    {
                        await HomeProvider.SetAssociationPerson(p);

                        if (p.HOME_Association_Person_Extended != null)
                        {
                            foreach (var ext in p.HOME_Association_Person_Extended)
                            {
                                await HomeProvider.SetAssociationPersonExtended(ext);
                            }
                        }
                    }
                }

                var existingThemes = await HomeProvider.GetAssociation_Themes(Data.ID);

                if (existingThemes != null && SelectedPeople != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();

                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveAssociationTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new HOME_Association_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.HOME_Association_ID = Data.ID;

                        await HomeProvider.SetAssociationTheme(newPerson);
                    }
                }

                var existingDocuments = await HomeProvider.GetAssociation_Documents(Data.ID);

                if (existingDocuments != null && SelectedDocuments != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.HOME_Document_ID != null && !SelectedDocuments.Contains(p.HOME_Document_ID.Value)).ToList();
                    var docsToAdd = SelectedDocuments.Where(p => !existingDocuments.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID).Contains(p)).ToList();

                    foreach (var t in docsToDelete)
                    {
                        await HomeProvider.RemoveAssociationDocument(t);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Association_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Document_ID = t;
                        newItem.HOME_Association_ID = Data.ID;

                        await HomeProvider.SetAssociationDocument(newItem);
                    }
                }
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Associations");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Associations");
            StateHasChanged();
        }
    }
}
