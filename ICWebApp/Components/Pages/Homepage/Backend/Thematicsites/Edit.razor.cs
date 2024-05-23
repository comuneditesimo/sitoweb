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

namespace ICWebApp.Components.Pages.Homepage.Backend.Thematicsites
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
        private HOME_Thematic_Sites? Data;
        private List<V_HOME_Thematic_Sites_Type> Types = new List<V_HOME_Thematic_Sites_Type>();
        private List<FILE_FileInfo> DocumentsDE = new List<FILE_FileInfo>();
        private List<FILE_FileInfo> DocumentsIT = new List<FILE_FileInfo>();
        private List<FILE_FileInfo> DocumentsLA = new List<FILE_FileInfo>();
        private bool Ladinisch = false;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Thematicsites", "MAINMENU_BACKEND_HOMEPAGE_THEMATIC_SITES", null, null, false);

            Ladinisch = await LangProvider.HasLadinisch();

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_THEMATIC_SITES_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Thematicsites/Edit", "MAINMENU_BACKEND_HOMEPAGE_THEMATIC_SITES_EDIT", null, null, true);

                    Data = await HomeProvider.GetThematicsite(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Thematic_Sites_Extended = await HomeProvider.GetThematicsite_Extended(Data.ID);
                        Data.HOME_Thematic_Sites_Document = await HomeProvider.GetThematicsite_Documents(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Thematic_Sites_Extended != null && Data.HOME_Thematic_Sites_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Thematic_Sites_Extended == null)
                                    {
                                        Data.HOME_Thematic_Sites_Extended = new List<HOME_Thematic_Sites_Extended>();
                                    }

                                    if (Data.HOME_Thematic_Sites_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Thematic_Sites_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Thematic_Sites_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetThematicsite_Extended(dataE);
                                        Data.HOME_Thematic_Sites_Extended.Add(dataE);
                                    }
                                }
                            }

                            if (Data.HOME_Thematic_Sites_Document != null)
                            {
                                foreach (var doc in Data.HOME_Thematic_Sites_Document)
                                {
                                    if (doc.FILE_FileInfo_ID != null)
                                    {
                                        var file = await FileProvider.GetFileInfoAsync(doc.FILE_FileInfo_ID.Value);

                                        if (file != null)
                                        {
                                            if (doc.LANG_Language_ID == LanguageSettings.German)
                                            {
                                                DocumentsDE.Add(file);
                                            }
                                            else if(doc.LANG_Language_ID == LanguageSettings.Italian)
                                            {
                                                DocumentsIT.Add(file);
                                            }
                                            else if (doc.LANG_Language_ID == LanguageSettings.Ladinisch)
                                            {
                                                DocumentsLA.Add(file);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_THEMATIC_SITES_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Thematicsites/Edit", "MAINMENU_BACKEND_HOMEPAGE_THEMATIC_SITES_NEW", null, null, true);

                    Data = new HOME_Thematic_Sites();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Thematic_Sites_Extended == null)
                            {
                                Data.HOME_Thematic_Sites_Extended = new List<HOME_Thematic_Sites_Extended>();
                            }

                            if (Data.HOME_Thematic_Sites_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Thematic_Sites_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Thematic_Sites_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Thematic_Sites_Extended.Add(dataE);
                            }
                        }
                    }
                }
            }

            if(Data != null && Data.FILE_FileInfo_ID != null)
            {
                Data.Image = await FileProvider.GetFileInfoAsync(Data.FILE_FileInfo_ID.Value);
            }

            Types = await HomeProvider.GetThematicsitesTypes(LangProvider.GetCurrentLanguageID());
            
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
                if (Data.Image != null)
                {
                    Data.FILE_FileInfo_ID = Data.Image.ID;
                    await FileProvider.SetFileInfo(Data.Image);
                }
                else
                {
                    if (Data.FILE_FileInfo_ID != null)
                    {
                        await FileProvider.RemoveFileInfo(Data.FILE_FileInfo_ID.Value);
                        Data.FILE_FileInfo_ID = null;
                    }
                }

                await HomeProvider.SetThematicsites(Data);

                if (Data.HOME_Thematic_Sites_Extended != null)
                {
                    foreach (var ext in Data.HOME_Thematic_Sites_Extended)
                    {
                        await HomeProvider.SetThematicsite_Extended(ext);
                    }
                }

                var existingDocuments = await HomeProvider.GetThematicsite_Documents(Data.ID);

                if (existingDocuments != null && DocumentsDE != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.FILE_FileInfo_ID != null && p.LANG_Language_ID == LanguageSettings.German && !DocumentsDE.Select(p => p.ID).Contains(p.FILE_FileInfo_ID.Value)).ToList();
                    var docsToAdd = DocumentsDE.Where(p => !existingDocuments.Where(p => p.FILE_FileInfo_ID != null && p.LANG_Language_ID == LanguageSettings.German).Select(p => p.FILE_FileInfo_ID).Contains(p.ID)).ToList();


                    foreach (var t in docsToDelete)
                    {
                        if (t.FILE_FileInfo_ID != null)
                        {
                            await FileProvider.RemoveFileInfo(t.FILE_FileInfo_ID.Value);
                        }
                        await HomeProvider.RemoveThematicsite_Document(t.ID);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Thematic_Sites_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Thematic_Sites_ID = Data.ID;
                        newItem.LANG_Language_ID = LanguageSettings.German;
                        newItem.CreationDate = DateTime.Now;

                        await FileProvider.SetFileInfo(t);

                        newItem.FILE_FileInfo_ID = t.ID;

                        await HomeProvider.SetThematicsite_Document(newItem);
                    }
                }

                if (existingDocuments != null && DocumentsIT != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.FILE_FileInfo_ID != null && p.LANG_Language_ID == LanguageSettings.Italian && !DocumentsIT.Select(p => p.ID).Contains(p.FILE_FileInfo_ID.Value)).ToList();
                    var docsToAdd = DocumentsIT.Where(p => !existingDocuments.Where(p => p.FILE_FileInfo_ID != null && p.LANG_Language_ID == LanguageSettings.Italian).Select(p => p.FILE_FileInfo_ID).Contains(p.ID)).ToList();


                    foreach (var t in docsToDelete)
                    {
                        if (t.FILE_FileInfo_ID != null)
                        {
                            await FileProvider.RemoveFileInfo(t.FILE_FileInfo_ID.Value);
                        }
                        await HomeProvider.RemoveThematicsite_Document(t.ID);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Thematic_Sites_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Thematic_Sites_ID = Data.ID;
                        newItem.LANG_Language_ID = LanguageSettings.Italian;
                        newItem.CreationDate = DateTime.Now;

                        await FileProvider.SetFileInfo(t);

                        newItem.FILE_FileInfo_ID = t.ID;

                        await HomeProvider.SetThematicsite_Document(newItem);
                    }
                }

                if (existingDocuments != null && DocumentsLA != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.FILE_FileInfo_ID != null && p.LANG_Language_ID == LanguageSettings.Ladinisch && !DocumentsLA.Select(p => p.ID).Contains(p.FILE_FileInfo_ID.Value)).ToList();
                    var docsToAdd = DocumentsLA.Where(p => !existingDocuments.Where(p => p.FILE_FileInfo_ID != null && p.LANG_Language_ID == LanguageSettings.Ladinisch).Select(p => p.FILE_FileInfo_ID).Contains(p.ID)).ToList();


                    foreach (var t in docsToDelete)
                    {
                        if (t.FILE_FileInfo_ID != null)
                        {
                            await FileProvider.RemoveFileInfo(t.FILE_FileInfo_ID.Value);
                        }
                        await HomeProvider.RemoveThematicsite_Document(t.ID);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Thematic_Sites_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Thematic_Sites_ID = Data.ID;
                        newItem.LANG_Language_ID = LanguageSettings.Ladinisch;
                        newItem.CreationDate = DateTime.Now;

                        await FileProvider.SetFileInfo(t);

                        newItem.FILE_FileInfo_ID = t.ID;

                        await HomeProvider.SetThematicsite_Document(newItem);
                    }
                }
            }

            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Thematicsites");
            StateHasChanged();            
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Thematicsites");
            StateHasChanged();
        }
    }
}
