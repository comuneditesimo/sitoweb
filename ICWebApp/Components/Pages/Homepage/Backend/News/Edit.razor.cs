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

namespace ICWebApp.Components.Pages.Homepage.Backend.News
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
        private HOME_Article? Data;
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
        private List<Guid> SelectedThemes { get; set; }
        private List<Guid> SelectedDocuments { get; set; }
        private List<V_HOME_Article_Type> Types { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Articles", "MAINMENU_BACKEND_HOMEPAGE_ARTICLES", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ARTICLES_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Articles/Edit", "MAINMENU_BACKEND_HOMEPAGE_ARTICLES_EDIT", null, null, true);

                    Data = await HomeProvider.GetArticle(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Article_Extended = await HomeProvider.GetArticle_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Article_Extended != null && Data.HOME_Article_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Article_Extended == null)
                                    {
                                        Data.HOME_Article_Extended = new List<HOME_Article_Extended>();
                                    }

                                    if (Data.HOME_Article_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Article_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Article_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetArticleExtended(dataE);
                                        Data.HOME_Article_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ARTICLES_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Articles/Edit", "MAINMENU_BACKEND_HOMEPAGE_ARTICLES_NEW", null, null, true);

                    Data = new HOME_Article();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                    Data.CreationDate = DateTime.Now;
                    Data.ReleaseDate = DateTime.Now;
                    Data.Visible = true;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Article_Extended == null)
                            {
                                Data.HOME_Article_Extended = new List<HOME_Article_Extended>();
                            }

                            if (Data.HOME_Article_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Article_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Article_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Article_Extended.Add(dataE);
                            }
                        }
                    }
                }
            }

            if (Data != null)
            {
                var themes = await HomeProvider.GetArticle_Themes(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();

                if (Data.FILE_FileInfo_ID != null)
                {
                    Data.Image = await FileProvider.GetFileInfoAsync(Data.FILE_FileInfo_ID.Value);
                }

                var documents = await HomeProvider.GetArticle_Documents(Data.ID);

                SelectedDocuments = documents.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID.Value).ToList();
            }
            else
            {
                SelectedThemes = new List<Guid>();
                SelectedDocuments = new List<Guid>();
            }

            Types = await HomeProvider.GetArticle_Types(LangProvider.GetCurrentLanguageID());

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
                Data.LastModificationDate = DateTime.Now;

                if (Data.Image != null)
                {
                    await FileProvider.SetFileInfo(Data.Image);

                    Data.FILE_FileInfo_ID = Data.Image.ID;
                }
                else
                {
                    Data.FILE_FileInfo_ID = null;
                }

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckArticleHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_ARTICLE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await HomeProvider.SetArticle(Data);

                if (Data.HOME_Article_Extended != null)
                {
                    foreach (var ext in Data.HOME_Article_Extended)
                    {
                        await HomeProvider.SetArticleExtended(ext);
                    }
                }

                var existingThemes = await HomeProvider.GetArticle_Themes(Data.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();

                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveArticleTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new HOME_Article_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.HOME_Article_ID = Data.ID;

                        await HomeProvider.SetArticleTheme(newPerson);
                    }
                }

                var existingDocuments = await HomeProvider.GetArticle_Documents(Data.ID);

                if (existingDocuments != null && SelectedDocuments != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.HOME_Document_ID != null && !SelectedDocuments.Contains(p.HOME_Document_ID.Value)).ToList();
                    var docsToAdd = SelectedDocuments.Where(p => !existingDocuments.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID).Contains(p)).ToList();

                    foreach (var t in docsToDelete)
                    {
                        await HomeProvider.RemoveArticleDocument(t);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Article_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Document_ID = t;
                        newItem.HOME_Article_ID = Data.ID;

                        await HomeProvider.SetArticleDocument(newItem);
                    }
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Articles");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Articles");
            StateHasChanged();
        }
    }
}
