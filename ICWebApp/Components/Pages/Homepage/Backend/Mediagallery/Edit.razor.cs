using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor.Components.Editor;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ICWebApp.Components.Pages.Homepage.Backend.Mediagallery
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

        private HOME_Media? Data;
        private FILE_FileInfo? ExistingFile;
        private Guid? CurrentLanguage;
        private List<LANG_Languages>? Languages;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Media", "MAINMENU_BACKEND_HOMEPAGE_MEDIA", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_MEDIA_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Media/Edit", "MAINMENU_BACKEND_HOMEPAGE_MEDIA_EDIT", null, null, true);

                    Data = await HomeProvider.GetMedia(Guid.Parse(ID));

                    if (Data != null && Data.FILE_FileInfo_ID != null)
                    {
                        ExistingFile = await FileProvider.GetFileInfoAsync(Data.FILE_FileInfo_ID.Value);
                    }

                    if (Data != null)
                    {
                        Data.HOME_Media_Extended = await HomeProvider.GetMediaExtended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Media_Extended != null && Data.HOME_Media_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Media_Extended == null)
                                    {
                                        Data.HOME_Media_Extended = new List<HOME_Media_Extended>();
                                    }

                                    if (Data.HOME_Media_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Media_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Media_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetMediaExtended(dataE);
                                        Data.HOME_Media_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_MEDIA_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Media/Edit", "MAINMENU_BACKEND_HOMEPAGE_MEDIA_NEW", null, null, true);

                    Data = new HOME_Media();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Media_Extended == null)
                            {
                                Data.HOME_Media_Extended = new List<HOME_Media_Extended>();
                            }

                            if (Data.HOME_Media_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Media_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Media_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Media_Extended.Add(dataE);
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
            if(ExistingFile != null && Data != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                await FileProvider.SetFileInfo(ExistingFile);

                Data.FILE_FileInfo_ID = ExistingFile.ID;
                Data.CreationDate = DateTime.Now;

                if (Data.SortOrder == null)
                {
                    Data.SortOrder = await HomeProvider.GetMediaSortOrder(SessionWrapper.AUTH_Municipality_ID.Value);
                }

                Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                if (Data.HOME_Media_Extended != null)
                {
                    foreach (var ext in Data.HOME_Media_Extended)
                    {
                        await HomeProvider.SetMediaExtended(ext);
                    }
                }

                await HomeProvider.SetMedia(Data);
            }

            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Media");
            StateHasChanged();            
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Media");
            StateHasChanged();
        }
    }
}
