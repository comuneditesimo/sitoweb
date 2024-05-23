using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using ICWebApp.Domain.Models.Homepage.Amministration;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Administration
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
        [Inject] NavigationManager NavManager { get; set; }


        private List<AmministrationtItem> ItemList = new List<AmministrationtItem>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HP_MAINMENU_VERWALTUNG");
            SessionWrapper.PageSubTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Administration);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageDescription = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageDescription = TextProvider.Get("HP_MAINMENU_VERWALTUNG_DESCRIPTION");
                }
            }

            SessionWrapper.ShowTitleSepparation = true;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.ShowQuestioneer = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, true);

            await GetData();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var person = await HomeProvider.GetPeople(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), true);

                if(person != null && person.Any())
                {
                    var p = person.OrderByDescending(p => p.LastChangeDate).FirstOrDefault();

                    if(p != null)
                    {
                        ItemList.Add(new AmministrationtItem()
                        {
                            Title = p.Fullname,
                            DescriptionShort = p.DescriptionShort,
                            Url = "/hp/Person/" + p.ID,
                            Highlight = true
                        });
                    }
                }

                var docs = await HomeProvider.GetDocuments(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), true);

                if (docs != null && docs.Any())
                {
                    var d = docs.OrderByDescending(p => p.LastChangeDate).FirstOrDefault();

                    if (d != null)
                    {
                        ItemList.Add(new AmministrationtItem()
                        {
                            Title = d.Title,
                            DescriptionShort = d.DescriptionShort,
                            Url = "/hp/Document/" + d.ID,
                            Highlight = true
                        });
                    }
                }

                var orgs = await HomeProvider.GetOrganisations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), true);

                if (orgs != null && orgs.Any())
                {
                    var d = orgs.OrderByDescending(p => p.LastChangeDate).FirstOrDefault();

                    if (d != null)
                    {
                        ItemList.Add(new AmministrationtItem()
                        {
                            Title = d.Title,
                            DescriptionShort = d.DescriptionShort,
                            Url = "/hp/Organisation/" + d.ID,
                            Highlight = true
                        });
                    }
                }

                var venues = await HomeProvider.GetVenues(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), true);

                if (venues != null && venues.Any())
                {
                    var d = venues.OrderByDescending(p => p.LastChangeDate).FirstOrDefault();

                    if (d != null)
                    {
                        ItemList.Add(new AmministrationtItem()
                        {
                            Title = d.Title,
                            DescriptionShort = d.DescriptionShort,
                            Url = "/hp/Venue/" + d.ID,
                            Highlight = true
                        });
                    }
                }

                var authorities = await HomeProvider.GetAuthorities(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), true);

                if (authorities != null && authorities.Any())
                {
                    var d = authorities.OrderByDescending(p => p.LastChangeDate).FirstOrDefault();

                    if (d != null)
                    {
                        ItemList.Add(new AmministrationtItem()
                        {
                            Title = d.Title,
                            DescriptionShort = d.DescriptionShort,
                            Url = "/hp/Authority/" + d.ID,
                            Highlight = true
                        });
                    }
                }

                var associations = await HomeProvider.GetAssociations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), true);

                if (associations != null && associations.Any())
                {
                    var d = associations.OrderByDescending(p => p.LastChangeDate).FirstOrDefault();

                    if (d != null)
                    {
                        ItemList.Add(new AmministrationtItem()
                        {
                            Title = d.Title,
                            DescriptionShort = d.DescriptionShort,
                            Url = "/hp/Association/" + d.ID,
                            Highlight = true
                        });
                    }
                }

                ItemList.Add(new AmministrationtItem()
                {
                    Title = TextProvider.Get("HP_MAINMENU_AMTSTAFEL"),
                    DescriptionShort = TextProvider.Get("HP_MAINMENU_AMTSTAFEL_DESCRIPTIONSHORT"),
                    Url = "/hp/Albopretorio"
                });

                var docItem = new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_DOCUMENTS"),
                    Url = "/hp/Document"
                };

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Documents);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        docItem.DescriptionShort = pagesubTitle.Description;
                    }
                    else
                    {
                        docItem.DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_DOCUMENTS_DESCRIPTIONSHORT");
                    }
                }

                ItemList.Add(docItem);

                ItemList.Add(new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_ASSOCIATIONS"),
                    DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_ASSOCIATIONS_DESCRIPTIONSHORT"),
                    Url = "/hp/Association"
                });

                var orgItem = new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_ORGANISATIONS"),
                    Url = "/hp/Organisation"
                };

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Organisations);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        orgItem.DescriptionShort = pagesubTitle.Description;
                    }
                    else
                    {
                        orgItem.DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_ORGANISATIONS_DESCRIPTIONSHORT");
                    }
                }

                ItemList.Add(orgItem);

                ItemList.Add(new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_EMPLOYEES"),
                    DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_EMPLOYEES_DESCRIPTION_SHORT"),
                    Url = "/hp/Type/Person/" + PersonType.Employee
                });

                ItemList.Add(new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_POLITICIANS"),
                    DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_POLITICIANS_DESCRIPTION_SHORT"),
                    Url = "/hp/Type/Person/" + PersonType.Politician
                });

                var authItem = new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_AUTHORITIES"),
                    Url = "/hp/authority"
                };

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Authorities);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        authItem.DescriptionShort = pagesubTitle.Description;
                    }
                    else
                    {
                        authItem.DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_AUTHORITIES_DESCRIPTION_SHORT");
                    }
                }

                ItemList.Add(authItem);

                var venueItem = new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_VENUES"),
                    Url = "/hp/venue"
                };

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Venues);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        venueItem.DescriptionShort = pagesubTitle.Description;
                    }
                    else
                    {
                        venueItem.DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_VENUES_DESCRIPTION_SHORT");
                    }
                }

                ItemList.Add(venueItem);

                ItemList.Add(new AmministrationtItem()
                {
                    Title = TextProvider.Get("HOMEPAGE_FRONTEND_APPOINTMENT"),
                    DescriptionShort = TextProvider.Get("HOMEPAGE_FRONTEND_APPOINTMENT_DESCRIPTION_SHORT"),
                    Url = "/hp/appointment"
                });
            }
        }
    }
}
