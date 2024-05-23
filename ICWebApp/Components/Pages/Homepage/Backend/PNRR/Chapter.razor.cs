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
    public partial class Chapter
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
        [Parameter] public string ID { get; set; }

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_PNRR_Chapter? Data;
        private List<V_HOME_PNRR_Chapter_Document> Documents = new List<V_HOME_PNRR_Chapter_Document>();

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR", "MAINMENU_BACKEND_HOMEPAGE_PNRR", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Edit/" + Guid.Parse(ProjectID), "MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT", null, null, false);
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Chapter/Edit", "MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT", null, null, true);

                    Data = await HomeProvider.GetPNRRChapter(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_PNRR_Chapter_Extended = await HomeProvider.GetPNRRChapter_Extended(Data.ID);
                        Documents = await HomeProvider.GetPNRRChapter_Document(Data.ID, LangProvider.GetCurrentLanguageID());

                        if (Languages != null)
                        {
                            if (Data.HOME_PNRR_Chapter_Extended != null && Data.HOME_PNRR_Chapter_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_PNRR_Chapter_Extended == null)
                                    {
                                        Data.HOME_PNRR_Chapter_Extended = new List<HOME_PNRR_Chapter_Extended>();
                                    }

                                    if (Data.HOME_PNRR_Chapter_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_PNRR_Chapter_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_PNRR_Chapter_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetPNRRChapter_Extended(dataE);
                                        Data.HOME_PNRR_Chapter_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PNRR_CHAPTER_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Edit/" + Guid.Parse(ProjectID), "MANIMENU_BACKEND_HOMEPAGE_PNRR_NEW", null, null, false);
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Chapter/Edit", "MAINMENU_BACKEND_HOMEPAGE_PNRR_CHAPTER_NEW", null, null, true);

                    Data = new HOME_PNRR_Chapter();

                    Data.ID = Guid.NewGuid();
                    Data.HOME_PNRR_ID = Guid.Parse(ProjectID);
                    

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_PNRR_Chapter_Extended == null)
                            {
                                Data.HOME_PNRR_Chapter_Extended = new List<HOME_PNRR_Chapter_Extended>();
                            }

                            if (Data.HOME_PNRR_Chapter_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_PNRR_Chapter_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_PNRR_Chapter_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_PNRR_Chapter_Extended.Add(dataE);
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
                if(Data.SortOrder == 0)
                {
                    Data.SortOrder = await HomeProvider.GetPNRRChapterSortOrder(Guid.Parse(ProjectID));
                }

                await HomeProvider.SetPNRRChapter(Data);

                if (Data.HOME_PNRR_Chapter_Extended != null)
                {
                    foreach (var ext in Data.HOME_PNRR_Chapter_Extended)
                    {
                        await HomeProvider.SetPNRRChapter_Extended(ext);
                    }
                }

                if (Redirect)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Backend/Homepage/PNRR/Edit/" + Guid.Parse(ProjectID));
                    StateHasChanged();
                }
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/PNRR/Edit/" + Guid.Parse(ProjectID));
            StateHasChanged();
        }
        private async void NewDocument()
        {
            if (Data != null)
            {
                await Save(false);
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/PNRR/Chapter/Document/Edit/" + ProjectID + "/" + Data.ID + "/New");
                StateHasChanged();
            }
        }
        private async void EditDocument(V_HOME_PNRR_Chapter_Document Item)
        {
            if (Data != null)
            {
                await Save(false);
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/PNRR/Chapter/Document/Edit/" + ProjectID + "/" + Data.ID + "/" + Item.ID.ToString());
                StateHasChanged();
            }
        }
        private async void DeleteDocument(V_HOME_PNRR_Chapter_Document Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_PNRR_CHAPTER_DOCUMENT"), TextProvider.Get("WARNING")))
                    return;

                StateHasChanged();

                await Save(false);
                await HomeProvider.RemovePNRRChapter_Document(Item.ID);
                Documents = await HomeProvider.GetPNRRChapter_Document(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                StateHasChanged();
            }
        }
    }
}
