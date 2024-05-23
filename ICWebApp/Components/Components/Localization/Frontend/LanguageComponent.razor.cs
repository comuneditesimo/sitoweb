using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.DataStore;
using ICWebApp.Application.Interface.Services;
using System.Globalization;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Components.Components.Localization.Frontend
{
    public partial class LanguageComponent
    {
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }

        private LANG_Languages CurrentLanguage;
        private bool Ladinisch = false;

        protected override async Task OnInitializedAsync()
        {
            CurrentLanguage = LangProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            Ladinisch = await LangProvider.HasLadinisch();
            StateHasChanged();

            await base.OnInitializedAsync();
        }

        private async Task<bool> SetLanguage(string LanguageCode)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            await LangProvider.SetLanguage(LanguageCode);

            return true;
        }
    }
}
