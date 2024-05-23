using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor.Components.Editor;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.Theme
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
        private HOME_Theme? Data;
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
        private List<V_HOME_Theme_Type> Types = new List<V_HOME_Theme_Type>();

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Themes", "MANMENU_BACKEND_HOMEPAGE_THEMES", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_THEMES_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Themes/Edit", "MANMENU_BACKEND_HOMEPAGE_THEMES_EDIT", null, null, true);

                    Data = await HomeProvider.GetTheme(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Theme_Extended = await HomeProvider.GetTheme_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Theme_Extended != null && Data.HOME_Theme_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Theme_Extended == null)
                                    {
                                        Data.HOME_Theme_Extended = new List<HOME_Theme_Extended>();
                                    }

                                    if (Data.HOME_Theme_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Theme_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Theme_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetThemeExtended(dataE);
                                        Data.HOME_Theme_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_THEMES_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Themes/Edit", "MANMENU_BACKEND_HOMEPAGE_THEMES_NEW", null, null, true);

                    Data = new HOME_Theme();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Theme_Extended == null)
                            {
                                Data.HOME_Theme_Extended = new List<HOME_Theme_Extended>();
                            }

                            if (Data.HOME_Theme_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Theme_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Theme_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Theme_Extended.Add(dataE);
                            }
                        }
                    }
                }

                Types = await HomeProvider.GetTheme_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            if (Data != null && Data.FILE_FileInfo_ID != null)
            {
                Data.Image = await FileProvider.GetFileInfoAsync(Data.FILE_FileInfo_ID.Value);
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
                if (Data.Highlight)
                {
                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        var allowed = await HomeProvider.CheckThemeHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                        if (!allowed)
                        {
                            if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                                return;

                            Data.Highlight = false;
                        }
                    }
                }

                if (Data.Image != null)
                {
                    Data.FILE_FileInfo_ID = Data.Image.ID;
                    await FileProvider.SetFileInfo(Data.Image);
                }
                else
                {
                    Data.FILE_FileInfo_ID = null;
                }

                await HomeProvider.SetTheme(Data);

                if (Data.HOME_Theme_Extended != null)
                {
                    foreach (var ext in Data.HOME_Theme_Extended)
                    {
                        await HomeProvider.SetThemeExtended(ext);
                    }
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Themes");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Themes");
            StateHasChanged();
        }
    }
}
