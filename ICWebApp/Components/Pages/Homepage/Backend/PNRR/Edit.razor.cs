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
    public partial class Edit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private List<Guid> SelectedThemes { get; set; }
        private HOME_PNRR? Data;
        private List<V_HOME_PNRR_Chapter> Chapter = new List<V_HOME_PNRR_Chapter>();
        private List<V_HOME_PNRR_Type> Types = new List<V_HOME_PNRR_Type>();

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR", "MAINMENU_BACKEND_HOMEPAGE_PNRR", null, null, false);

            if (CurrentLanguage == null)
                CurrentLanguage = LanguageSettings.German;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Edit", "MAINMENU_BACKEND_HOMEPAGE_PNRR_EDIT", null, null, true);

                    Data = await HomeProvider.GetPNRR(Guid.Parse(ID));
                    Chapter = await HomeProvider.GetPNRRChapterList(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                    if (Data != null)
                    {
                        Data.HOME_PNRR_Extended = await HomeProvider.GetPNRR_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_PNRR_Extended != null && Data.HOME_PNRR_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_PNRR_Extended == null)
                                    {
                                        Data.HOME_PNRR_Extended = new List<HOME_PNRR_Extended>();
                                    }

                                    if (Data.HOME_PNRR_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_PNRR_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_PNRR_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetPNRR_Extended(dataE);
                                        Data.HOME_PNRR_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANIMENU_BACKEND_HOMEPAGE_PNRR_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR/Edit", "MANIMENU_BACKEND_HOMEPAGE_PNRR_NEW", null, null, true);

                    Data = new HOME_PNRR();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                    

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_PNRR_Extended == null)
                            {
                                Data.HOME_PNRR_Extended = new List<HOME_PNRR_Extended>();
                            }

                            if (Data.HOME_PNRR_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_PNRR_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_PNRR_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_PNRR_Extended.Add(dataE);
                            }
                        }
                    }
                }
            }

            if(Data != null)
            {
                var themes = await HomeProvider.GetPNRR_Themes(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();
            }
            else
            {
                SelectedThemes = new List<Guid>();
            }

            Types = await HomeProvider.GetPNRRTypes(LangProvider.GetCurrentLanguageID());

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task Save(bool Redirect = true)
        {
            if (Data != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data.LastModificationDate = DateTime.Now;
                Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                if(Data.SortOrder == 0)
                {
                    Data.SortOrder = await HomeProvider.GetPNRRSortOrder(SessionWrapper.AUTH_Municipality_ID.Value);
                }

                await HomeProvider.SetPNRR(Data);

                if (Data.HOME_PNRR_Extended != null)
                {
                    foreach (var ext in Data.HOME_PNRR_Extended)
                    {
                        await HomeProvider.SetPNRR_Extended(ext);
                    }
                }

                var existingThemes = await HomeProvider.GetPNRR_Themes(Data.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();


                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemovePNRRTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new HOME_PNRR_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.HOME_PNRR_ID = Data.ID;

                        await HomeProvider.SetPNRRTheme(newPerson);
                    }
                }

                if (Redirect)
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo("/Backend/Homepage/PNRR");
                    StateHasChanged();
                }
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/PNRR");
            StateHasChanged();
        }
        private async void NewChapter()
        {
            if (Data != null)
            {
                await Save(false);
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/PNRR/Chapter/Edit/" + Data.ID + "/New");
                StateHasChanged();
            }
        }
        private async void EditChapter(V_HOME_PNRR_Chapter Item)
        {
            if (Data != null)
            {
                await Save(false);
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/PNRR/Chapter/Edit/" + Data.ID + "/" + Item.ID.ToString());
                StateHasChanged();
            }
        }
        private async void DeleteChapter(V_HOME_PNRR_Chapter Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_PNRR_CHAPTER"), TextProvider.Get("WARNING")))
                    return;

                StateHasChanged();

                await Save(false);
                await HomeProvider.RemovePNRRChapter(Item.ID);
                Chapter = await HomeProvider.GetPNRRChapterList(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                StateHasChanged();
            }
        }
        private async void MoveUpChapter(V_HOME_PNRR_Chapter item)
        {
            if (Chapter != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetPNRRChapter(item.ID);

                if (dbItem != null)
                {
                    var otherItem = Chapter.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder - 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetPNRRChapter(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder - 1;

                            await HomeProvider.SetPNRRChapter(dbOtherItem);
                            await HomeProvider.SetPNRRChapter(dbItem);
                            Chapter = await HomeProvider.GetPNRRChapterList(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                            StateHasChanged();
                        }
                    }
                }
            }
        }
        private async void MoveDownChapter(V_HOME_PNRR_Chapter item)
        {
            if (Chapter != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetPNRRChapter(item.ID);

                if (dbItem != null)
                {
                    var otherItem = Chapter.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder + 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetPNRRChapter(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder + 1;

                            await HomeProvider.SetPNRRChapter(dbOtherItem);
                            await HomeProvider.SetPNRRChapter(dbItem);
                            Chapter = await HomeProvider.GetPNRRChapterList(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                            StateHasChanged();
                        }
                    }
                }
            }
        }
    }
}
