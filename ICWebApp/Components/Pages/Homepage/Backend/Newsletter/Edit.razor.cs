using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Syncfusion.Blazor.Popups;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Pages.Homepage.Backend.Newsletter
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
        private HOME_Municipal_Newsletter? Data;
        private List<V_HOME_Municipal_Newsletter_Type> Types = new List<V_HOME_Municipal_Newsletter_Type>();
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
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Newsletter", "MAINMENU_BACKEND_HOMEPAGE_NEWSLETTER", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_NEWSLETTER_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Newsletter/Edit", "MAINMENU_BACKEND_HOMEPAGE_NEWSLETTER_EDIT", null, null, true);

                    Data = await HomeProvider.GetMunicipal_Newsletter(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Municipal_Newsletter_Extended = await HomeProvider.GetMunicipal_Newsletter_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Municipal_Newsletter_Extended != null && Data.HOME_Municipal_Newsletter_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Municipal_Newsletter_Extended == null)
                                    {
                                        Data.HOME_Municipal_Newsletter_Extended = new List<HOME_Municipal_Newsletter_Extended>();
                                    }

                                    if (Data.HOME_Municipal_Newsletter_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Municipal_Newsletter_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Municipal_Newsletter_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetMunicipalNewsletterExtended(dataE);
                                        Data.HOME_Municipal_Newsletter_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_NEWSLETTER_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Newsletter/Edit", "MAINMENU_BACKEND_HOMEPAGE_NEWSLETTER_NEW", null, null, true);

                    Data = new HOME_Municipal_Newsletter();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                    Data.ReleaseDate = DateTime.Now;
                    Data.CreationDate = DateTime.Now;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Municipal_Newsletter_Extended == null)
                            {
                                Data.HOME_Municipal_Newsletter_Extended = new List<HOME_Municipal_Newsletter_Extended>();
                            }

                            if (Data.HOME_Municipal_Newsletter_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Municipal_Newsletter_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Municipal_Newsletter_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Municipal_Newsletter_Extended.Add(dataE);
                            }
                        }
                    }
                }

                Types = await HomeProvider.GetNewsletter_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            if(Data != null && Data.HOME_Municipal_Newsletter_Extended != null)
            {
                foreach(var ext in Data.HOME_Municipal_Newsletter_Extended)
                {
                    if(ext.Image_FILE_FileInfo_ID != null)
                    {
                        ext.Image = await FileProvider.GetFileInfoAsync(ext.Image_FILE_FileInfo_ID.Value);
                    }
                    if(ext.Download_FILE_FileInfo_ID != null)
                    {
                        ext.File = await FileProvider.GetFileInfoAsync(ext.Download_FILE_FileInfo_ID.Value);
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
                await HomeProvider.SetMunicipalNewsletter(Data);

                if (Data.HOME_Municipal_Newsletter_Extended != null)
                {
                    foreach (var ext in Data.HOME_Municipal_Newsletter_Extended)
                    {
                        if (ext.Image != null)
                        {
                            ext.Image_FILE_FileInfo_ID = ext.Image.ID;
                            await FileProvider.SetFileInfo(ext.Image);
                        }
                        else
                        {
                            ext.Image_FILE_FileInfo_ID = null;
                        }

                        if(ext.File != null)
                        {
                            ext.Download_FILE_FileInfo_ID = ext.File.ID;
                            await FileProvider.SetFileInfo(ext.File);
                        }
                        else
                        {
                            ext.Download_FILE_FileInfo_ID = null;
                        }

                        await HomeProvider.SetMunicipalNewsletterExtended(ext);
                    }
                }             

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Newsletter");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Newsletter");
            StateHasChanged();
        }
    }
}
