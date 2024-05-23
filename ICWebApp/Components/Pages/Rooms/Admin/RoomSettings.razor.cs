using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;
using ICWebApp.Application.Provider;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Pages.Rooms.Admin
{
    public partial class RoomSettings
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IRoomProvider ROOMProvider { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] SfDialogService Dialogs { get; set; }

        private ROOM_Settings? Configuration { get; set; }
        private bool IsHomepage = false;
        private List<Guid> SelectedThemes = new List<Guid>();
        private List<LANG_Languages>? Languages { get; set; }
        private Guid? CurrentLanguage { get; set; }
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
        public List<string> StripTags { get; set; } = new List<string>() { "font", "h1", "h2", "h2", "h3", "h4", "h5", "h6", "img", "pre", "p", "span", "b", "strong", "ins", "del", "i", "em", "small", "mark", "sup", "div" };


        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_ROOM_SETTINGS");

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb(NavManager.Uri, "MAINMENU_BACKEND_ROOM_SETTINGS", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Configuration = await ROOMProvider.GetSettings(SessionWrapper.AUTH_Municipality_ID.Value);

                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

                if (IsHomepage)
                {
                    var themes = await ROOMProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value);

                    SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();
                }
            }

            if (Configuration != null)
            {
                Languages = await LangProvider.GetAll();
                Configuration.ROOM_Settings_Extended = await ROOMProvider.GetSettingsExtended(Configuration.ID);

                if (Configuration.ROOM_Settings_Extended != null && Configuration.ROOM_Settings_Extended.Count < Languages.Count)
                {
                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Configuration.ROOM_Settings_Extended == null)
                            {
                                Configuration.ROOM_Settings_Extended = new List<ROOM_Settings_Extended>();
                            }

                            if (Configuration.ROOM_Settings_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new ROOM_Settings_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    ROOM_Settings_ID = Configuration.ID,
                                    LANG_Language_ID = l.ID
                                };

                                await ROOMProvider.SetSettingsExtended(dataE);
                                Configuration.ROOM_Settings_Extended.Add(dataE);
                            }
                        }
                    }
                }

                if (Languages != null)
                {
                    CurrentLanguage = Languages.FirstOrDefault().ID;
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task<bool> SaveConfig()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (Configuration != null)
            {
                if (Configuration.ROOM_Settings_Extended != null)
                {
                    foreach (var ext in Configuration.ROOM_Settings_Extended)
                    {
                        await ROOMProvider.SetSettingsExtended(ext);
                    }
                }

                await ROOMProvider.SetSettings(Configuration);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var existingThemes = await ROOMProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();


                    foreach (var t in themesToDelete)
                    {
                        await ROOMProvider.RemoveTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newItem = new ROOM_Theme();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Theme_ID = t;
                        newItem.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                        await ROOMProvider.SetTheme(newItem);
                    }
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            return true;
        }
    }
}
