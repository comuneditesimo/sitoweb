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

namespace ICWebApp.Components.Pages.Homepage.Backend.Faq
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
        private HOME_Faq? Data;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Faq", "MANMENU_BACKEND_HOMEPAGE_FAQ", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_FAQ_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Faq/Edit", "MANMENU_BACKEND_HOMEPAGE_FAQ_EDIT", null, null, true);

                    Data = await HomeProvider.GetFaq(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Faq_Extended = await HomeProvider.GetFaq_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Faq_Extended != null && Data.HOME_Faq_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Faq_Extended == null)
                                    {
                                        Data.HOME_Faq_Extended = new List<HOME_Faq_Extended>();
                                    }

                                    if (Data.HOME_Faq_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Faq_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Faq_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetFaq_Extended(dataE);
                                        Data.HOME_Faq_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_FAQ_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Faq/Edit", "MANMENU_BACKEND_HOMEPAGE_FAQ_NEW", null, null, true);

                    Data = new HOME_Faq();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Faq_Extended == null)
                            {
                                Data.HOME_Faq_Extended = new List<HOME_Faq_Extended>();
                            }

                            if (Data.HOME_Faq_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Faq_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Faq_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Faq_Extended.Add(dataE);
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
            if (Data != null)
            {
                await HomeProvider.SetFaq(Data);

                if (Data.HOME_Faq_Extended != null)
                {
                    foreach (var ext in Data.HOME_Faq_Extended)
                    {
                        await HomeProvider.SetFaq_Extended(ext);
                    }
                }                

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Faq");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Faq");
            StateHasChanged();
        }
    }
}
