using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections.Features;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;
using ICWebApp.Application.Settings;

namespace ICWebApp.Components.Pages.Homepage.Backend.Maintenance
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
        private bool Italian
        {
            get
            {
                if (CurrentLanguage != null && CurrentLanguage.Value == Guid.Parse("e450421a-baff-493e-a390-71b49be6485f"))
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (CurrentLanguage != null && value == true)
                {
                    CurrentLanguage = Guid.Parse("e450421a-baff-493e-a390-71b49be6485f");
                    StateHasChanged();
                }
            }
        }
        private bool German
        {
            get
            {
                if (CurrentLanguage != null && CurrentLanguage.Value == Guid.Parse("b97f0849-fa25-4cd0-8c7b-43f90fbe4075"))
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (CurrentLanguage != null && value == true)
                {
                    CurrentLanguage = Guid.Parse("b97f0849-fa25-4cd0-8c7b-43f90fbe4075");
                    StateHasChanged();
                }
            }
        }
        private List<LANG_Languages>? Languages { get; set; }
        private MAIN_Detail? Data;
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
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_MAINTENANCE");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Maintenance", "MAINMENU_BACKEND_HOMEPAGE_MAINTENANCE", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                Data = await HomeProvider.GetMaintenanceDetail(SessionWrapper.AUTH_Municipality_ID.Value);

                if(Data == null)
                {
                    Data = new MAIN_Detail();

                    Data.ID = Guid.NewGuid();
                    Data.LastModificationDate = DateTime.Now;
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    Data.MAIN_Detail_Extended = new List<MAIN_Detail_Extended>();

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.MAIN_Detail_Extended == null)
                            {
                                Data.MAIN_Detail_Extended = new List<MAIN_Detail_Extended>();
                            }

                            if (Data.MAIN_Detail_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new MAIN_Detail_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    MAIN_Detail_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.MAIN_Detail_Extended.Add(dataE);
                            }
                        }
                    }

                    await HomeProvider.SetMaintenanceDetail(Data);
                }
                else
                {
                    Data.MAIN_Detail_Extended = await HomeProvider.GetMaintenanceDetail_Extended(Data.ID);

                    if (Languages != null)
                    {
                        if (Data.MAIN_Detail_Extended != null && Data.MAIN_Detail_Extended.Count < Languages.Count)
                        {
                            foreach (var l in Languages)
                            {
                                if (Data.MAIN_Detail_Extended == null)
                                {
                                    Data.MAIN_Detail_Extended = new List<MAIN_Detail_Extended>();
                                }

                                if (Data.MAIN_Detail_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                {
                                    var dataE = new MAIN_Detail_Extended()
                                    {
                                        ID = Guid.NewGuid(),
                                        MAIN_Detail_ID = Data.ID,
                                        LANG_Language_ID = l.ID
                                    };

                                    await HomeProvider.SetMaintenanceDetailExtended(dataE);
                                    Data.MAIN_Detail_Extended.Add(dataE);
                                }
                            }
                        }
                    }
                }

                var themes = await HomeProvider.GetMaintenance_Themes(SessionWrapper.AUTH_Municipality_ID.Value);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();
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
                Data.LastModificationDate = DateTime.Now;

                await HomeProvider.SetMaintenanceDetail(Data);

                if (Data.MAIN_Detail_Extended != null)
                {
                    foreach (var ext in Data.MAIN_Detail_Extended)
                    {
                        await HomeProvider.SetMaintenanceDetailExtended(ext);
                    }
                }

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var existingThemes = await HomeProvider.GetMaintenance_Themes(Data.ID);

                    if (existingThemes != null && SelectedThemes != null)
                    {
                        var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                        var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();


                        foreach (var t in themesToDelete)
                        {
                            await HomeProvider.RemoveMaintenance_Themes(t);
                        }

                        foreach (var t in themesToAdd)
                        {
                            var newItem = new MAIN_Theme();

                            newItem.ID = Guid.NewGuid();
                            newItem.HOME_Theme_ID = t;
                            newItem.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                            await HomeProvider.SetMaintenance_Themes(newItem);
                        }
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
