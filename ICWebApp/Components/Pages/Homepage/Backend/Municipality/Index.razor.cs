using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections.Features;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;
using ICWebApp.Application.Settings;
using System.Linq;

namespace ICWebApp.Components.Pages.Homepage.Backend.Municipality
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
        private HOME_Municipality? Data;
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
        public List<FILE_FileInfo> Images = new List<FILE_FileInfo>();
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_MUNICIPALITY");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Municipality", "MAINMENU_BACKEND_HOMEPAGE_MUNICIPALITY", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                Data = await HomeProvider.GetMunicipalityByAuthMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                if(Data == null)
                {
                    Data = new HOME_Municipality();

                    Data.ID = Guid.NewGuid();
                    Data.LastChangeDate = DateTime.Now;
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID;

                    Data.HOME_Municipality_Extended = new List<HOME_Municipality_Extended>();

                    await HomeProvider.SetMunicipality(Data);
                }
                else
                {
                    Data.HOME_Municipality_Extended = await HomeProvider.GetMunicipality_Extended(Data.ID);

                    if (Languages != null)
                    {
                        if (Data.HOME_Municipality_Extended != null && Data.HOME_Municipality_Extended.Count < Languages.Count)
                        {
                            foreach (var l in Languages)
                            {
                                if (Data.HOME_Municipality_Extended == null)
                                {
                                    Data.HOME_Municipality_Extended = new List<HOME_Municipality_Extended>();
                                }

                                if (Data.HOME_Municipality_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                {
                                    var dataE = new HOME_Municipality_Extended()
                                    {
                                        ID = Guid.NewGuid(),
                                        HOME_Municipality_ID = Data.ID,
                                        LANG_Language_ID = l.ID
                                    };

                                    await HomeProvider.SetMunicipalityExtended(dataE);
                                    Data.HOME_Municipality_Extended.Add(dataE);
                                }
                            }
                        }
                    }
                }
            }

            if (Data != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Images = new List<FILE_FileInfo>();

                var homeImages = await HomeProvider.GetMunicipality_Images(SessionWrapper.AUTH_Municipality_ID.Value);

                foreach (var img in homeImages) 
                {
                    if (img.FILE_FileImage_ID != null)
                    {
                        var file = await FileProvider.GetFileInfoAsync(img.FILE_FileImage_ID.Value);

                        if (file != null)
                        {
                            Images.Add(file);
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
            if (Data != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data.LastChangeDate = DateTime.Now;

                var existing = await HomeProvider.GetMunicipality_Images(SessionWrapper.AUTH_Municipality_ID.Value);

                if (existing != null && Images != null)
                {
                    var toDelete = existing.Where(p => p.FILE_FileImage_ID != null && !Images.Select(x => x.ID).Contains(p.FILE_FileImage_ID.Value)).ToList();
                    var toAdd = Images.Where(p => !existing.Where(p => p.FILE_FileImage_ID != null).Select(p => p.FILE_FileImage_ID.Value).Contains(p.ID)).ToList();


                    foreach (var t in toDelete)
                    {
                        await FileProvider.RemoveFileInfo(t.ID);
                        await HomeProvider.RemoveMunicipality_Images(t);
                    }

                    foreach (var t in toAdd)
                    {
                        await FileProvider.SetFileInfo(t);

                        var newItem = new HOME_Municipality_Images();

                        newItem.ID = Guid.NewGuid();
                        newItem.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                        newItem.FILE_FileImage_ID = t.ID;

                        await HomeProvider.SetMunicipality_Images(newItem);
                    }
                }

                await HomeProvider.SetMunicipality(Data);

                if (Data.HOME_Municipality_Extended != null)
                {
                    foreach (var ext in Data.HOME_Municipality_Extended)
                    {
                        await HomeProvider.SetMunicipalityExtended(ext);
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
