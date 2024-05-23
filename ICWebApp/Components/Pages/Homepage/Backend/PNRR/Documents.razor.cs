using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.PNRR
{
    public partial class Documents
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ProjectID { get; set; }
        [Parameter] public string ChapterID { get; set; }
        [Parameter] public string ID { get; set; }

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_PNRR_Chapter_Document? Data;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR", "MAINMENU_BACKEND_HOMEPAGE_PNRR", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PNRR_DOCUMENT_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Edit/" + Guid.Parse(ProjectID), "MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT", null, null, false);
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Chapter/Edit" + Guid.Parse(ProjectID) + "/" + Guid.Parse(ChapterID), "MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT", null, null, false);
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Chapter/Document/Edit", "MAINMENU_BACKEND_HOMEPAGE_PNRR_DOCUMENT_EDIT", null, null, true);

                    Data = await HomeProvider.GetPNRRChapter_Document(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_PNRR_Chapter_Document_Extended = await HomeProvider.GetPNRRChapterDocument_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_PNRR_Chapter_Document_Extended != null && Data.HOME_PNRR_Chapter_Document_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_PNRR_Chapter_Document_Extended == null)
                                    {
                                        Data.HOME_PNRR_Chapter_Document_Extended = new List<HOME_PNRR_Chapter_Document_Extended>();
                                    }

                                    if (Data.HOME_PNRR_Chapter_Document_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_PNRR_Chapter_Document_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_PNRR_Chapter_Document_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetPNRRChapterDocument_Extended(dataE);
                                        Data.HOME_PNRR_Chapter_Document_Extended.Add(dataE);
                                    }
                                }
                            }
                        }

                        if (Data.HOME_PNRR_Chapter_Document_Extended != null) 
                        {
                            foreach (var ext in Data.HOME_PNRR_Chapter_Document_Extended)
                            {
                                if (ext.FILE_FileInfo_ID != null)
                                {
                                    ext.File = FileProvider.GetFileInfo(ext.FILE_FileInfo_ID.Value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PNRR_CHAPTER_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Edit/" + Guid.Parse(ProjectID), "MANIMENU_BACKEND_HOMEPAGE_PNRR_NEW", null, null, false);
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Chapter/Edit" + Guid.Parse(ProjectID) + "/" + Guid.Parse(ChapterID), "MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT", null, null, false);
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Chapter/Document/Edit", "MAINMENU_BACKEND_HOMEPAGE_PNRR_DOCUMENT_NEW", null, null, true);

                    Data = new HOME_PNRR_Chapter_Document();

                    Data.ID = Guid.NewGuid();
                    Data.HOME_PNRR_Chapter_ID = Guid.Parse(ChapterID);
                    

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_PNRR_Chapter_Document_Extended == null)
                            {
                                Data.HOME_PNRR_Chapter_Document_Extended = new List<HOME_PNRR_Chapter_Document_Extended>();
                            }

                            if (Data.HOME_PNRR_Chapter_Document_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_PNRR_Chapter_Document_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_PNRR_Chapter_Document_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_PNRR_Chapter_Document_Extended.Add(dataE);
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
        private async Task Save(bool Redirect = true)
        {
            if (Data != null && SessionWrapper.AUTH_Municipality_ID != null)
            { 
                await HomeProvider.SetPNRRChapter_Document(Data);

                if (Data.HOME_PNRR_Chapter_Document_Extended != null)
                {
                    foreach (var ext in Data.HOME_PNRR_Chapter_Document_Extended)
                    {
                        if(ext.File != null)
                        {
                            await FileProvider.SetFileInfo(ext.File);
                            ext.FILE_FileInfo_ID = ext.File.ID;
                        }
                        else if(ext.FILE_FileInfo_ID != null)
                        {
                            await FileProvider.RemoveFileInfo(ext.FILE_FileInfo_ID.Value);
                            ext.FILE_FileInfo_ID = null;
                        }

                        await HomeProvider.SetPNRRChapterDocument_Extended(ext);
                    }
                }

                if (Redirect)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Backend/Homepage/PNRR/Chapter/Edit/" + Guid.Parse(ProjectID) + "/" + Guid.Parse(ChapterID));
                    StateHasChanged();
                }
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/PNRR/Chapter/Edit/" + Guid.Parse(ProjectID)+ "/" + Guid.Parse(ChapterID));
            StateHasChanged();
        }
    }
}
