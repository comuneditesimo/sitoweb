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

namespace ICWebApp.Components.Pages.Homepage.Backend.Impressum
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
        private HOME_Impressum? Data;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Impressum", "MANMENU_BACKEND_HOMEPAGE_IMPRESSUM", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_IMPRESSUM_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Impressum/Edit", "MANMENU_BACKEND_HOMEPAGE_IMPRESSUM_EDIT", null, null, true);

                    Data = await HomeProvider.GetImpressum(Guid.Parse(ID));

                    if (Data != null)
                    {
                        Data.HOME_Impressum_Extended = await HomeProvider.GetImpressum_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Impressum_Extended != null && Data.HOME_Impressum_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Impressum_Extended == null)
                                    {
                                        Data.HOME_Impressum_Extended = new List<HOME_Impressum_Extended>();
                                    }

                                    if (Data.HOME_Impressum_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Impressum_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Impressum_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetImpressum_Extended(dataE);
                                        Data.HOME_Impressum_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_IMPRESSUM_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Impressum/Edit", "MANMENU_BACKEND_HOMEPAGE_IMPRESSUM_NEW", null, null, true);

                    Data = new HOME_Impressum();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                    

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Impressum_Extended == null)
                            {
                                Data.HOME_Impressum_Extended = new List<HOME_Impressum_Extended>();
                            }

                            if (Data.HOME_Impressum_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Impressum_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Impressum_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Impressum_Extended.Add(dataE);
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
                    Data.SortOrder = await HomeProvider.GetImpressumSortOrder(SessionWrapper.AUTH_Municipality_ID.Value);
                }

                await HomeProvider.SetImpressum(Data);

                if (Data.HOME_Impressum_Extended != null)
                {
                    foreach (var ext in Data.HOME_Impressum_Extended)
                    {
                        await HomeProvider.SetImpressum_Extended(ext);
                    }
                }                

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Impressum");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Impressum");
            StateHasChanged();
        }
    }
}
