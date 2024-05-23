using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;
using System.Linq;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.Privacy
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
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_Privacy? Data;
        private List<FILE_FileInfo> Files = new List<FILE_FileInfo>();

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Privacy", "MANMENU_BACKEND_HOMEPAGE_PRIVACY", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_PRIVACY_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Privacy/Edit", "MANMENU_BACKEND_HOMEPAGE_PRIVACY_EDIT", null, null, true);

                    Data = await HomeProvider.GetPrivacy(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Privacy_Extended = await HomeProvider.GetPrivacy_Extended(Data.ID);
                        Data.HOME_Privacy_Document = await HomeProvider.GetPrivacy_Document(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Privacy_Extended != null && Data.HOME_Privacy_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Privacy_Extended == null)
                                    {
                                        Data.HOME_Privacy_Extended = new List<HOME_Privacy_Extended>();
                                    }

                                    if (Data.HOME_Privacy_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Privacy_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Privacy_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetPrivacy_Extended(dataE);
                                        Data.HOME_Privacy_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                        if (Data.HOME_Privacy_Document != null)
                        {
                            foreach (var doc in Data.HOME_Privacy_Document)
                            {
                                if (doc.FILE_FileInfo_ID != null)
                                {
                                    var file = await FileProvider.GetFileInfoAsync(doc.FILE_FileInfo_ID.Value);

                                    if (file != null)
                                    {
                                        Files.Add(file);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_PRIVACY_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Privacy/Edit", "MANMENU_BACKEND_HOMEPAGE_PRIVACY_NEW", null, null, true);

                    Data = new HOME_Privacy();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                    

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Privacy_Extended == null)
                            {
                                Data.HOME_Privacy_Extended = new List<HOME_Privacy_Extended>();
                            }

                            if (Data.HOME_Privacy_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Privacy_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Privacy_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Privacy_Extended.Add(dataE);
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
            if (Data != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data.LastModificationDate = DateTime.Now;

                if(Data.SortOrder == 0)
                {
                    Data.SortOrder = await HomeProvider.GetPrivacySortOrder(SessionWrapper.AUTH_Municipality_ID.Value);
                }

                await HomeProvider.SetPrivacy(Data);

                if (Data.HOME_Privacy_Extended != null)
                {
                    foreach (var ext in Data.HOME_Privacy_Extended)
                    {
                        await HomeProvider.SetPrivacy_Extended(ext);
                    }
                }

                var existingDocuments = await HomeProvider.GetPrivacy_Document(Data.ID);

                if (existingDocuments != null && Files != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.FILE_FileInfo_ID != null && !Files.Select(p => p.ID).Contains(p.FILE_FileInfo_ID.Value)).ToList();
                    var docsToAdd = Files.Where(p => !existingDocuments.Where(p => p.FILE_FileInfo_ID != null).Select(p => p.FILE_FileInfo_ID).Contains(p.ID)).ToList();

                    foreach (var t in docsToDelete)
                    {
                        if (t.FILE_FileInfo_ID != null)
                        {
                            await FileProvider.RemoveFileInfo(t.FILE_FileInfo_ID.Value);
                        }

                        await HomeProvider.RemovePrivacy_Document(t.ID);
                    }

                    foreach (var t in docsToAdd)
                    {
                        await FileProvider.SetFileInfo(t);

                        var newItem = new HOME_Privacy_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.FILE_FileInfo_ID = t.ID;
                        newItem.HOME_Privacy_ID = Data.ID;
                        newItem.CreationDate = DateTime.Now;

                        await HomeProvider.SetPrivacy_Document(newItem);
                    }
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Privacy");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Privacy");
            StateHasChanged();
        }
    }
}
