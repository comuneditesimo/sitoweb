using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Search;
using ICWebApp.Domain.Models.Searchbar;
using ICWebApp.Components.Pages.Form.Admin;
using ICWebApp.Components.Pages.Form.Frontend;
using Microsoft.AspNetCore.Components;
using SQLite;
using Telerik.DataSource.Extensions;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Search
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IHPServiceHelper HPServiceHelper { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        [Parameter] public string? SearchText { get; set; }

        private string? InputText;
        private List<V_HOME_Theme> Themes { get; set; }
        private List<SearchbarItem> SearchItems = new List<SearchbarItem>();
        private bool _filterOrganiations = false;
        private bool _filterEvents = false;
        private bool _filterVenues = false;
        private bool _filterDocuments = false;
        private bool _filterNews = false;
        private bool _filterServices = false;
        private bool _filterPeople = false;
        private bool FilterOrganiations
        {
            get
            {
                return _filterOrganiations;
            }
            set
            {
                _filterOrganiations = value;
                OnChanged();
            }
        }
        private bool FilterEvents
        {
            get
            {
                return _filterEvents;
            }
            set
            {
                _filterEvents = value;
                OnChanged();
            }
        }
        private bool FilterVenues
        {
            get
            {
                return _filterVenues;
            }
            set
            {
                _filterVenues = value;
                OnChanged();
            }
        }
        private bool FilterDocuments
        {
            get
            {
                return _filterDocuments;
            }
            set
            {
                _filterDocuments = value;
                OnChanged();
            }
        }
        private bool FilterNews
        {
            get
            {
                return _filterNews;
            }
            set
            {
                _filterNews = value;
                OnChanged();
            }
        }
        private bool FilterServices
        {
            get
            {
                return _filterServices;
            }
            set
            {
                _filterServices = value;
                OnChanged();
            }
        }
        private bool FilterPeople
        {
            get
            {
                return _filterPeople;
            }
            set
            {
                _filterPeople = value;
                OnChanged();
            }
        }
        private List<AUTH_MunicipalityApps>? AktiveApps = new List<AUTH_MunicipalityApps>();
        private List<SearchbarItem>? DefinitionList;
        private List<SearchbarItem>? AuthorityList;
        private int MaxCount = 10;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = null;
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Search", "HP_MAINMENU_SEARCH", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Themes = HomeProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                foreach(var t in Themes)
                {
                    t.OnChecked += OnChanged;
                }
            }

            await LoadData();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }

        private async Task LoadData()
        {
            SearchItems = new List<SearchbarItem>();

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                List<V_HOME_Organisation> orgs = await HomeProvider.GetOrganisations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                foreach (var org in orgs)
                {
                    var item = new SearchbarItem()
                    {
                        Url = "/hp/Organisation/" + org.ID,
                        Title = org.Title,
                        ShortText = org.DescriptionShort,
                        SubTitle = org.Type,
                        ItemType = SearchBarItemType.Organiations,
                        Themes = org.Themes
                    };

                    if (org.HOME_Organisation_Type_ID != null)
                    {
                        item.SubTitleUrl = "/hp/Type/Organisation/" + org.HOME_Organisation_Type_ID;
                    }

                    SearchItems.Add(item);
                }

                List<V_HOME_Appointment_Dates> data = (await HomeProvider.GetAppointmentsWithDates(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID())).Where(p => p.HOME_Appointment_Type_ID == HOMEAppointmentTypes.Event).ToList();

                foreach (var d in data)
                {
                    var item = new SearchbarItem()
                    {
                        Url = "/hp/Event/" + d.HOME_AppointmentID,
                        Title = d.Title,
                        ShortText = d.DescriptionShort,
                        SubTitle = d.Type,
                        ItemType = SearchBarItemType.Events,
                        Themes = d.Themes
                    };

                    if (d.HOME_Appointment_Type_ID != null)
                    {
                        item.SubTitleUrl = "/hp/Event/";
                    }

                    SearchItems.Add(item);
                }

                List<V_HOME_Venue> venues = await HomeProvider.GetVenues(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                
                foreach (var d in venues)
                {
                    var item = new SearchbarItem()
                    {
                        Url = "/hp/Venue/" + d.ID,
                        Title = d.Title,
                        ShortText = d.DescriptionShort,
                        SubTitle = d.Type,
                        ItemType = SearchBarItemType.Venues
                    };

                    if (d.HOME_Venue_Type_ID != null)
                    {
                        item.SubTitleUrl = "/hp/Type/Venue/" + d.HOME_Venue_Type_ID;
                    }

                    SearchItems.Add(item);
                }

                List<V_HOME_Document> docs = await HomeProvider.GetDocuments(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                
                foreach (var d in docs)
                {
                    var item = new SearchbarItem()
                    {
                        Url = "/hp/Document/" + d.ID,
                        Title = d.Title,
                        ShortText = d.DescriptionShort,
                        SubTitle = d.Type,
                        ItemType = SearchBarItemType.Documents,
                        Themes = d.Themes
                    };

                    if (d.Type_ID != null)
                    {
                        item.SubTitleUrl = "/hp/Type/Document/" + d.Type_ID;
                    }

                    SearchItems.Add(item);
                }

                List<V_HOME_Article> news = await HomeProvider.GetArticles(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                

                foreach (var d in news)
                {
                    var item = new SearchbarItem()
                    {
                        Url = "/hp/News/" + d.ID,
                        Title = d.Subject,
                        ShortText = d.ShortText,
                        SubTitle = d.Type,
                        ItemType = SearchBarItemType.News,
                        Themes = d.Themes
                    };

                    if (d.HOME_Article_Type_ID != null)
                    {
                        item.SubTitleUrl = "/hp/Type/News/" + d.HOME_Article_Type_ID;
                    }

                    SearchItems.Add(item);
                }
              
                await GetServices();

                if (DefinitionList != null && DefinitionList.Any())
                {
                    SearchItems.AddRange(DefinitionList);
                }
                if (AuthorityList != null && AuthorityList.Any())
                {
                    SearchItems.AddRange(AuthorityList);
                }
         
                List<V_HOME_Person> people = new List<V_HOME_Person>();

                people.AddRange(await HomeProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID()));

                foreach (var d in people)
                {
                    var item = new SearchbarItem()
                    {
                        Url = "/hp/Person/" + d.ID,
                        Title = d.Fullname,
                        ShortText = d.DescriptionShort,
                        SubTitle = d.Type,
                        ItemType = SearchBarItemType.People
                    };

                    if (d.HOME_Person_Type_ID != null)
                    {
                        item.SubTitleUrl = "/hp/Type/Person/" + d.HOME_Person_Type_ID;
                    }

                    SearchItems.Add(item);
                }                
            }

            return;
        }
        private async Task GetServices()
        {
            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                AktiveApps = await AuthProvider.GetMunicipalityApps();

                DefinitionList = new List<SearchbarItem>();

                var services = HPServiceHelper.GetServices(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                foreach (var item in services)
                {
                    DefinitionList.Add(new SearchbarItem()
                    {
                        Url = item.Url,
                        SubTitleUrl = item.Kategorie_Url,
                        Title = item.Title,
                        ShortText = item.ShortText,
                        SubTitle = item.Kategorie,
                        ItemType = SearchBarItemType.Services
                    });
                }
            }

            return;
        }
        protected override void OnParametersSet()
        {
            InputText = SearchText;

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private void SearchData()
        {
            StateHasChanged();
        }
        private void RemoveFilter()
        {
            FilterOrganiations = false;
            FilterEvents = false;
            FilterVenues = false;
            FilterDocuments = false;
            FilterNews = false;
            FilterServices = false;
            FilterPeople = false;
            MaxCount = 10;
            InputText = null;

            foreach (var t in Themes)
            {
                t.Checked = false;
            }

            StateHasChanged();
        }
        private void NavigateTo(string Url)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo(Url);
            StateHasChanged();
        }
        private void ShowMore()
        {
            MaxCount = MaxCount + 10;

            if(SearchItems.Count < MaxCount)
            {
                MaxCount = SearchItems.Count();
            }

            StateHasChanged();
        }
        private void OnChanged()
        {
            MaxCount = 10;
            StateHasChanged();
        }
    }
}