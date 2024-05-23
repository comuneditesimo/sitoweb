using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections.Features;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.Models.Homepage.Settings;
using ICWebApp.Domain.Enums.Homepage.Settings;

namespace ICWebApp.Components.Pages.Homepage.Backend.Settings
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        private AUTH_Municipality? Data;
        private string? SaveMessage = null;

        private List<ThemeColor> Themes = new List<ThemeColor>()
        {
            new ThemeColor(){ Name="blue", Color="#19376D" },
            new ThemeColor(){ Name="lightblue", Color="#7895CB" },
            new ThemeColor(){ Name="red", Color="#BE3144" },
            new ThemeColor(){ Name="orange", Color="#9B3922" },
            new ThemeColor(){ Name="yellow", Color="#E5A921" },
            new ThemeColor(){ Name="green", Color="#0f7173" },
        };
        private List<PageSubtitleTypeItem> SubtitleTypes = new List<PageSubtitleTypeItem>()
        {
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Canteen, TEXT_SystemText_Code = "MAINMENU_CANTEEN"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Rooms, TEXT_SystemText_Code = "MAINMENU_ROOMS"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Maintenance, TEXT_SystemText_Code = "FRONTEND_MANTAINANCE_TITLE"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Administration, TEXT_SystemText_Code = "HP_MAINMENU_VERWALTUNG"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Authorities, TEXT_SystemText_Code = "HOMEPAGE_FRONTEND_AUTHORITIES"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Documents, TEXT_SystemText_Code = "HOMEPAGE_FRONTEND_DOCUMENTS"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Locations, TEXT_SystemText_Code = "HOMEPAGE_FRONTEND_LOCATION"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.MunicipalNewsletter, TEXT_SystemText_Code = "HOMEPAGE_FRONTEND_MUNICIPAL_NEWSLETTER"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Organisations, TEXT_SystemText_Code = "HOMEPAGE_FRONTEND_ORGANISATIONS"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.Venues, TEXT_SystemText_Code = "HOMEPAGE_FRONTEND_VENUES"},
            new PageSubtitleTypeItem(){ ID = PageSubtitleType.VillageLife, TEXT_SystemText_Code = "HP_MAINMENU_DORFLEBEN"},
        };
        private bool ShowTypeEditWindow = false;
        private List<AUTH_Municipality_Page_Subtitles>? TypeItems;
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
        public List<string> StripTags { get; set; } = new List<string>() { "font" };

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_SETTINGS");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Settings", "MAINMENU_BACKEND_HOMEPAGE_SETTINGS", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                if (Data != null && !string.IsNullOrEmpty(Data.ThemeColor))
                {
                    Data.ThemeColor = "green";
                }
            }

            CurrentLanguage = LanguageSettings.German;

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void Save()
        {
            if (Data != null)
            {             
                await AuthProvider.SetMunicipality(Data);

                SaveMessage = TextProvider.Get("BACKEND_HOMEPAGE_SAVE_SUCCESSFULL");
                StateHasChanged();

                Task.Run(async () => {
                    await Task.Delay(2000);
                    SaveMessage = null;
                    InvokeAsync(() => {
                        StateHasChanged();
                    });
                });
            }
        }
        private async void Edit(PageSubtitleTypeItem Type)
        {
            var languages = await LangProvider.GetAll();

            if (SessionWrapper.AUTH_Municipality_ID != null && languages != null)
            {
                TypeItems = await AuthProvider.GetPageSubtitles(SessionWrapper.AUTH_Municipality_ID.Value, (int)Type.ID);

                if (TypeItems == null)
                    TypeItems = new List<AUTH_Municipality_Page_Subtitles>();

                if(TypeItems != null && TypeItems.Count() < languages.Count())
                {
                    foreach (var lang in languages) 
                    {
                        var existing = TypeItems.FirstOrDefault(p => p.LANG_Language_ID == lang.ID);

                        if(existing == null)
                        {
                            TypeItems.Add(new AUTH_Municipality_Page_Subtitles()
                            {
                                AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value,
                                LANG_Language_ID = lang.ID,
                                TypeID = (int)Type.ID
                            });
                        }
                    }
                }

                ShowTypeEditWindow = true;
                StateHasChanged();
            }
        }
        private async void SaveType()
        {
            if(TypeItems != null)
            {
                foreach(var item in TypeItems)
                {
                    await AuthProvider.SetPageSubtitle(item);
                }

                TypeItems = null;
                ShowTypeEditWindow = false;
                StateHasChanged();
            }
        }
        private void CancelEditType()
        {
            TypeItems = null;
            ShowTypeEditWindow = false;
            StateHasChanged();
        }
    }
}
