using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections.Features;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;
using ICWebApp.Application.Settings;

namespace ICWebApp.Components.Pages.Homepage.Backend.Accessibility
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        private Guid? CurrentLanguage { get; set; }      
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_Accessibility? Data;
        private string? SaveMessage = null;
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
        private List<Guid> SelectedThemes = new List<Guid>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_ACCESSIBILITY");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Accessibility", "MAINMENU_BACKEND_HOMEPAGE_ACCESSIBILITY", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                Data = await HomeProvider.GetAccessibility(SessionWrapper.AUTH_Municipality_ID.Value);

                if(Data == null)
                {
                    Data = new HOME_Accessibility();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    Data.HOME_Accessibility_Extended = new List<HOME_Accessibility_Extended>();

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Accessibility_Extended == null)
                            {
                                Data.HOME_Accessibility_Extended = new List<HOME_Accessibility_Extended>();
                            }

                            if (Data.HOME_Accessibility_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Accessibility_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Accessibility_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Accessibility_Extended.Add(dataE);
                            }
                        }
                    }

                    await HomeProvider.SetAccessibility(Data);
                }
                else
                {
                    Data.HOME_Accessibility_Extended = await HomeProvider.GetAccessibilityExtended(Data.ID);

                    if (Languages != null)
                    {
                        if (Data.HOME_Accessibility_Extended != null && Data.HOME_Accessibility_Extended.Count < Languages.Count)
                        {
                            foreach (var l in Languages)
                            {
                                if (Data.HOME_Accessibility_Extended == null)
                                {
                                    Data.HOME_Accessibility_Extended = new List<HOME_Accessibility_Extended>();
                                }

                                if (Data.HOME_Accessibility_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                {
                                    var dataE = new HOME_Accessibility_Extended()
                                    {
                                        ID = Guid.NewGuid(),
                                        HOME_Accessibility_ID = Data.ID,
                                        LANG_Language_ID = l.ID
                                    };

                                    await HomeProvider.SetAccessibilityExtended(dataE);
                                    Data.HOME_Accessibility_Extended.Add(dataE);
                                }
                            }
                        }
                    }
                }
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
                await HomeProvider.SetAccessibility(Data);

                if (Data.HOME_Accessibility_Extended != null)
                {
                    foreach (var ext in Data.HOME_Accessibility_Extended)
                    {
                        await HomeProvider.SetAccessibilityExtended(ext);
                    }
                }

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
    }
}
