using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;
using Syncfusion.Blazor.Popups;

namespace ICWebApp.Components.Pages.Homepage.Backend.Venue
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
        private HOME_Venue? Data;
        private List<V_HOME_Venue_Type> Types = new List<V_HOME_Venue_Type>();
        private List<Guid> SelectedPeople { get; set; }
        private List<Guid> SelectedThemes { get; set; }
        private List<Guid> SelectedDocuments { get; set; }
        private FILE_FileInfo? ExistingFile;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Venues", "MAINMENU_BACKEND_HOMEPAGE_VENUES", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_VENUES_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Venues/Edit", "MAINMENU_BACKEND_HOMEPAGE_VENUES_EDIT", null, null, true);

                    Data = await HomeProvider.GetVenue(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Venue_Extended = await HomeProvider.GetVenue_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Venue_Extended != null && Data.HOME_Venue_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Venue_Extended == null)
                                    {
                                        Data.HOME_Venue_Extended = new List<HOME_Venue_Extended>();
                                    }

                                    if (Data.HOME_Venue_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Venue_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Venue_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetVenueExtended(dataE);
                                        Data.HOME_Venue_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_VENUES_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Venues/Edit", "MAINMENU_BACKEND_HOMEPAGE_VENUES_NEW", null, null, true);

                    Data = new HOME_Venue();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Venue_Extended == null)
                            {
                                Data.HOME_Venue_Extended = new List<HOME_Venue_Extended>();
                            }

                            if (Data.HOME_Venue_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Venue_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Venue_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Venue_Extended.Add(dataE);
                            }
                        }
                    }
                }

                Types = await HomeProvider.GetVenue_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            if(Data != null)
            {
                var people = await HomeProvider.GetVenue_People(Data.ID);

                SelectedPeople = people.Where(p => p.HOME_Person_ID != null).Select(p => p.HOME_Person_ID.Value).ToList();

                var themes = await HomeProvider.GetVenue_Themes(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();

                if (Data.FILE_FileInfo_ID != null)
                {
                    ExistingFile = await FileProvider.GetFileInfoAsync(Data.FILE_FileInfo_ID.Value);
                }

                var documents = await HomeProvider.GetVenue_Documents(Data.ID);

                SelectedDocuments = documents.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID.Value).ToList();
            }
            else
            {
                SelectedPeople = new List<Guid>();
                SelectedThemes = new List<Guid>();
                SelectedDocuments = new List<Guid>();
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
                Data.LastChangeDate = DateTime.Now;

                if (ExistingFile != null)
                {
                    await FileProvider.SetFileInfo(ExistingFile);

                    Data.FILE_FileInfo_ID = ExistingFile.ID;
                }
                else
                {
                    if (Data.FILE_FileInfo_ID != null)
                    {
                        await FileProvider.RemoveFileInfo(Data.FILE_FileInfo_ID.Value);

                        Data.FILE_FileInfo_ID = null;
                    }
                }

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckVenueHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await HomeProvider.SetVenue(Data);

                if (Data.HOME_Venue_Extended != null)
                {
                    foreach (var ext in Data.HOME_Venue_Extended)
                    {
                        await HomeProvider.SetVenueExtended(ext);
                    }
                }

                var existingPeople = await HomeProvider.GetVenue_People(Data.ID);

                if (existingPeople != null && SelectedPeople != null)
                {
                    var peopleToDelete = existingPeople.Where(p => p.HOME_Person_ID != null && !SelectedPeople.Contains(p.HOME_Person_ID.Value)).ToList();
                    var peopleToAdd = SelectedPeople.Where(p => !existingPeople.Where(p => p.HOME_Person_ID != null).Select(p => p.HOME_Person_ID).Contains(p)).ToList();


                    foreach(var p in peopleToDelete)
                    {
                        await HomeProvider.RemoveVenuePerson(p);
                    }

                    foreach(var p in peopleToAdd)
                    {
                        var newPerson = new HOME_Venue_Person();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Person_ID = p;
                        newPerson.HOME_Venue_ID = Data.ID;

                        await HomeProvider.SetVenuePerson(newPerson);
                    }
                }

                var existingThemes = await HomeProvider.GetVenue_Themes(Data.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();

                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveVenueTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new HOME_Venue_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.HOME_Venue_ID = Data.ID;

                        await HomeProvider.SetVenueTheme(newPerson);
                    }
                }

                var existingDocuments = await HomeProvider.GetVenue_Documents(Data.ID);

                if(existingDocuments != null && SelectedDocuments != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.HOME_Document_ID != null && !SelectedDocuments.Contains(p.HOME_Document_ID.Value)).ToList();
                    var docsToAdd = SelectedDocuments.Where(p => !existingDocuments.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID).Contains(p)).ToList();

                    foreach (var t in docsToDelete)
                    {
                        await HomeProvider.RemoveVenueDocument(t);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Venue_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Document_ID = t;
                        newItem.HOME_Venue_ID = Data.ID;

                        await HomeProvider.SetVenueDocument(newItem);
                    }
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Venues");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Venues");
            StateHasChanged();
        }
    }
}
