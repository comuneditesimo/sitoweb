using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;
using ICWebApp.Application.Interface.Services;
using Syncfusion.Blazor.Popups;
using ICWebApp.Application.Settings;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace ICWebApp.Components.Components.Homepage.Backend.Documents
{
    public partial class DocumentEdit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string DocumentID{ get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_Document? Data;
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
        private List<V_HOME_Document_Type> Types = new List<V_HOME_Document_Type>();
        private List<V_HOME_Document_Format> Formats = new List<V_HOME_Document_Format>();
        private List<V_HOME_Document_License> Licenses = new List<V_HOME_Document_License>();
        private List<V_HOME_Organisation> Organisations = new List<V_HOME_Organisation>();
        private List<Guid> SelectedThemes { get; set; }
        private List<Guid>? SelectedTypes { get; set; }
        private List<Guid> SelectedAuthorities { get; set; }
        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (DocumentID != "New")
                {
                    Data = await HomeProvider.GetDocument(Guid.Parse(DocumentID));

                    if (Data != null)
                    {
                        Data.HOME_Document_Extended = await HomeProvider.GetDocument_Extended(Data.ID);
                        Data.HOME_Document_Data = await HomeProvider.GetDocumentData(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Document_Extended != null && Data.HOME_Document_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Document_Extended == null)
                                    {
                                        Data.HOME_Document_Extended = new List<HOME_Document_Extended>();
                                    }

                                    if (Data.HOME_Document_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Document_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Document_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetDocumentExtended(dataE);
                                        Data.HOME_Document_Extended.Add(dataE);
                                    }

                                    if (Data.HOME_Document_Data == null)
                                    {
                                        Data.HOME_Document_Data = new List<HOME_Document_Data>();
                                    }

                                    if (Data.HOME_Document_Data.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Document_Data()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Document_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetDocumentData(dataE);
                                        Data.HOME_Document_Data.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {

                    Data = new HOME_Document();

                    Data.CreationDate = DateTime.Now;
                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Document_Extended == null)
                            {
                                Data.HOME_Document_Extended = new List<HOME_Document_Extended>();
                            }

                            if (Data.HOME_Document_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Document_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Document_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Document_Extended.Add(dataE);
                            }

                            if (Data.HOME_Document_Data == null)
                            {
                                Data.HOME_Document_Data = new List<HOME_Document_Data>();
                            }

                            if (Data.HOME_Document_Data.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Document_Data()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Document_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Document_Data.Add(dataE);
                            }
                        }
                    }
                }

                Types = await HomeProvider.GetDocument_Types(LangProvider.GetCurrentLanguageID());
                Formats = await HomeProvider.GetDocument_Formats(LangProvider.GetCurrentLanguageID());
                Licenses = await HomeProvider.GetDocument_Licenses(LangProvider.GetCurrentLanguageID());
                Organisations = await HomeProvider.GetOrganisations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            if (Data != null)
            {
                var themes = await HomeProvider.GetDocument_Themes(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();

                var types = await HomeProvider.GetDocument_Document_Types(Data.ID);

                SelectedTypes = types.Where(p => p.HOME_Document_Type_ID != null).Select(p => p.HOME_Document_Type_ID.Value).ToList();

                var authorities = await HomeProvider.GetDocument_Authorities(Data.ID);

                SelectedAuthorities = authorities.Where(p => p.AUTH_Authority_ID != null).Select(p => p.AUTH_Authority_ID.Value).ToList();

                if (Data.HOME_Document_Data != null)
                {
                    foreach (var doc in Data.HOME_Document_Data)
                    {
                        if (doc.Data != null)
                        {
                            doc.File = new FILE_FileInfo();
                            doc.File.FILE_FileStorage = new List<FILE_FileStorage>() { new FILE_FileStorage() };

                            if (doc.File.FILE_FileStorage.FirstOrDefault() != null)
                            {
                                doc.File.FILE_FileStorage.FirstOrDefault().FileImage = doc.Data;
                                doc.File.FILE_FileStorage.FirstOrDefault().FileImage = doc.Data;

                                doc.File.FileName = doc.Filename;
                                doc.File.FileExtension = doc.Fileextension;
                            }
                        }
                    }
                }
            }
            else
            {
                SelectedThemes = new List<Guid>();
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

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckDocumentsHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await HomeProvider.SetDocument(Data);

                if (Data.HOME_Document_Data != null)
                {
                    foreach (var doc in Data.HOME_Document_Data)
                    {
                        if (doc.File != null && doc.File.FILE_FileStorage.FirstOrDefault() != null)
                        {
                            doc.Data = doc.File.FILE_FileStorage.FirstOrDefault().FileImage;
                            doc.Filename = doc.File.FileName;
                            doc.Fileextension = doc.File.FileExtension;
                        }
                        else
                        {
                            doc.Data = null;
                            doc.Filename = null;
                            doc.Fileextension = null;
                        }

                        await HomeProvider.SetDocumentData(doc);
                    }

                    var docFormat = Data.HOME_Document_Data.Where(x => x.File != null).Select(x => x.File.FileExtension.ToLower()).Distinct().FirstOrDefault();

                    if (docFormat != null)
                    {
                        var format = Formats.Where(p => p.Format != null).FirstOrDefault(p => docFormat.Contains(p.Format.ToLower()));

                        if (format != null)
                        {
                            Data.HOME_Document_Format_ID = format.ID;
                        }
                    }
                }

                await HomeProvider.SetDocument(Data);

                if (Data.HOME_Document_Extended != null)
                {
                    foreach (var ext in Data.HOME_Document_Extended)
                    {
                        await HomeProvider.SetDocumentExtended(ext);
                    }
                }

                var existingThemes = await HomeProvider.GetDocument_Themes(Data.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();

                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveDocumentTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new HOME_Document_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.HOME_Document_ID = Data.ID;

                        await HomeProvider.SetDocumentTheme(newPerson);
                    }
                }

                var existingTypes = await HomeProvider.GetDocument_Document_Types(Data.ID);

                if (existingTypes != null && SelectedTypes != null)
                {
                    var itemsToDelete = existingTypes.Where(p => p.HOME_Document_Type_ID != null && !SelectedTypes.Contains(p.HOME_Document_Type_ID.Value)).ToList();
                    var itemsToAdd = SelectedTypes.Where(p => !existingTypes.Where(p => p.HOME_Document_Type_ID != null).Select(p => p.HOME_Document_Type_ID).Contains(p)).ToList();

                    foreach (var t in itemsToDelete)
                    {
                        await HomeProvider.RemoveDocumentDocumentType(t);
                    }

                    foreach (var t in itemsToAdd)
                    {
                        var newItem = new HOME_Document_Document_Type();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Document_Type_ID = t;
                        newItem.HOME_Document_ID = Data.ID;

                        await HomeProvider.SetDocumentDocumentType(newItem);
                    }
                }
                else if(existingTypes != null && SelectedTypes == null)
                {
                    foreach (var t in existingTypes)
                    {
                        await HomeProvider.RemoveDocumentDocumentType(t);
                    }
                }

                var existingAuth = await HomeProvider.GetDocument_Authorities(Data.ID);

                if (existingAuth != null && SelectedAuthorities != null)
                {
                    var itemsToDelete = existingAuth.Where(p => p.AUTH_Authority_ID != null && !SelectedAuthorities.Contains(p.AUTH_Authority_ID.Value)).ToList();
                    var itemsToAdd = SelectedAuthorities.Where(p => !existingAuth.Where(p => p.AUTH_Authority_ID != null).Select(p => p.AUTH_Authority_ID).Contains(p)).ToList();

                    foreach (var t in itemsToDelete)
                    {
                        await HomeProvider.RemoveDocumentAuthority(t);
                    }

                    foreach (var t in itemsToAdd)
                    {
                        var newItem = new HOME_Document_Authority();

                        newItem.ID = Guid.NewGuid();
                        newItem.AUTH_Authority_ID = t;
                        newItem.HOME_Document_ID = Data.ID;

                        await HomeProvider.SetDocumentAuthority(newItem);
                    }
                }

                await OnSave.InvokeAsync();
            }
        }
        private async void Cancel()
        {
            await OnCancel.InvokeAsync();
        }

    }
}
