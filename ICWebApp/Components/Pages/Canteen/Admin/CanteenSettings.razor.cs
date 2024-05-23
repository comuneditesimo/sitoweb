using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Pages.Canteen.Admin
{
    public partial class CanteenSettings

    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }

        [Inject] public SfDialogService Dialogs { get; set; }
        private CANTEEN_Configuration? Configuration { get; set; }
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
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CANTEEN_SETTINGS");
            SessionWrapper.PageDescription = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb(NavManager.Uri, "MAINMENU_BACKEND_CANTEEN_SETTINGS", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Configuration = await CanteenProvider.GetConfiguration(SessionWrapper.AUTH_Municipality_ID.Value);

                if(Configuration == null)
                {
                    Configuration = new CANTEEN_Configuration();
                    Configuration.ID = Guid.NewGuid();
                    Configuration.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    Configuration.BalanceLowWarning = 20;

                    Configuration.BalanceLowServiceStop = -50;

                    Configuration.RechargeMinAmount = 50;
                    Configuration.PosMode = false;
                    Configuration.NewPosCardPrice = 15;

                    await CanteenProvider.SetConfiguration(Configuration);
                }

                Languages = await LangProvider.GetAll();
                Configuration.CANTEEN_Configuration_Extended = await CanteenProvider.GetConfigurationExtended(Configuration.ID);

                if(Configuration.CANTEEN_Configuration_Extended != null && Configuration.CANTEEN_Configuration_Extended.Count < Languages.Count)
                {
                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Configuration.CANTEEN_Configuration_Extended == null)
                            {
                                Configuration.CANTEEN_Configuration_Extended = new List<CANTEEN_Configuration_Extended>();
                            }

                            if (Configuration.CANTEEN_Configuration_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new CANTEEN_Configuration_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    CANTEEN_Configuration_ID = Configuration.ID,
                                    LANG_Language_ID = l.ID
                                };

                                await CanteenProvider.SetConfigurationExtended(dataE);
                                Configuration.CANTEEN_Configuration_Extended.Add(dataE);
                            }
                        }
                    }
                }

                if (Languages != null)
                {
                    CurrentLanguage = Languages.FirstOrDefault().ID;
                }

                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

                if (IsHomepage)
                {
                    var themes = await CanteenProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value);

                    SelectedThemes = themes.Where(p => p.HOME_THEME_ID != null).Select(p => p.HOME_THEME_ID.Value).ToList();
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task<bool> SaveConfig()
        {
            if (Configuration != null && Configuration.PosMode && (Configuration.NewPosCardPrice < 0 || Configuration.NewPosCardPrice > 50))
            {
                await Dialogs.AlertAsync(TextProvider.Get("CANTEEN_NEW_POS_CARD_VALUE_INVALID"));
                return false;
            }
            
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (Configuration != null)
            {
                if(Configuration.CANTEEN_Configuration_Extended != null)
                {
                    foreach(var ext in Configuration.CANTEEN_Configuration_Extended)
                    {
                        await CanteenProvider.SetConfigurationExtended(ext);
                    }
                }

                Configuration.LastModificationDate = DateTime.Now;
                await CanteenProvider.SetConfiguration(Configuration);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var existingThemes = await CanteenProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_THEME_ID != null && !SelectedThemes.Contains(p.HOME_THEME_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_THEME_ID != null).Select(p => p.HOME_THEME_ID).Contains(p)).ToList();


                    foreach (var t in themesToDelete)
                    {
                        await CanteenProvider.RemoveTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newItem = new CANTEEN_Theme();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_THEME_ID = t;
                        newItem.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                        await CanteenProvider.SetTheme(newItem);
                    }
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            return true;
        }
    }
}
