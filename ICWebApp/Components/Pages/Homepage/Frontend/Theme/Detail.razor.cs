using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Theme
{
    public partial class Detail
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider {  get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] IHPServiceHelper HPServiceHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        [Parameter] public string ID { get; set; }

        private V_HOME_Theme? Theme;
        private V_HOME_Person? Person;
        private V_HOME_Authority? Authority;
        private List<V_HOME_Article>? Articles;
        private List<V_HOME_Document>? Documents;
        private List<V_HOME_Organisation>? Organizations;
        private List<ServiceItem>? Services;

        protected override async Task OnParametersSetAsync()
        {
            if (string.IsNullOrEmpty(ID))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/");
                StateHasChanged();
                return;
            }

            Theme = await HomeProvider.GetVTheme(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

            if (Theme != null)
            {
                SessionWrapper.PageTitle = null;
                SessionWrapper.ShowBreadcrumb = false;
                SessionWrapper.ShowHelpSection = true;

                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Hp/Theme", "HP_SECONDMENU_ALL_TOPICS", null, null, false);
                CrumbService.AddBreadCrumb("/Hp/Theme/ID", Theme.Title, null, null, true);

                if (Theme.Managed_HOME_Person_ID != null)
                {
                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        if (Theme.Managed_HOME_Person_ID != null)
                        {
                            Person = await HomeProvider.GetVPerson(Theme.Managed_HOME_Person_ID.Value, LangProvider.GetCurrentLanguageID());
                        }

                        if (Theme.Managed_AUTH_Authority_ID != null)
                        {
                            Authority = await HomeProvider.GetVAuthority(Theme.Managed_AUTH_Authority_ID.Value, LangProvider.GetCurrentLanguageID());
                        }
                    }
                }

                if (SessionWrapper.AUTH_Municipality_ID != null) 
                {
                    Articles = await HomeProvider.GetArticlesByTheme(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Theme.ID);
                    Organizations = await HomeProvider.GetOrganisationsByTheme(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Theme.ID);
                    Documents = await HomeProvider.GetDocumentsByTheme(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Theme.ID);
                    Services = await HPServiceHelper.GetServicesByThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Theme.ID);
                } 
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnParametersSetAsync();
        }
        private void GoToPerson()
        {
            if (Theme != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Person/" + Theme.Managed_HOME_Person_ID);
                StateHasChanged();
            }
        }
        private void GoToAuthority()
        {
            if (Theme != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Authority/" + Theme.Managed_AUTH_Authority_ID);
                StateHasChanged();
            }
        }
        private void GoToArticle(V_HOME_Article Item)
        {
            if (Theme != null && Item != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/News/" + Item.ID);
                StateHasChanged();
            }
        }
        private void GoToAllNews()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/News");
            StateHasChanged();
        }
        private void GoToAllOrganisations()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Organisation");
            StateHasChanged();
        }
        private void GoToOrganisationType(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Organisation/" + ID);
            StateHasChanged();
        }        
        private void GoToOrganisation(Guid ID)
        {
            if (Theme != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Organisation/" + ID);
                StateHasChanged();
            }
        }
        private void GoToAllServices()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Services");
            StateHasChanged();
        }
        private void GoToService(string? Url)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Url);
                StateHasChanged();
            }
        }
        private void GoToServiceType(string? Url)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Url);
                StateHasChanged();
            }
        }        
        private void GoToAllDocuments()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Document");
            StateHasChanged();
        }
        private void GoToDocuments(Guid ID)
        {
            if (Theme != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Document/" + ID);
                StateHasChanged();
            }
        }
    }
}
