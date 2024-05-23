using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace ICWebApp.Components.Components.Localization.Backend
{
    public partial class LanguageInputSelector
    {
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }

        [Parameter] public Guid? CurrentLanguage { get; set; }
        [Parameter] public EventCallback<Guid?> CurrentLanguageChanged { get; set; }

        private Guid SelectedLanguage 
        { 
            get
            {
                if (CurrentLanguage == null)
                {
                    CurrentLanguage = LanguageSettings.German;
                    CurrentLanguageChanged.InvokeAsync(CurrentLanguage);
                }

                return CurrentLanguage.Value;
            }
            set
            {
                CurrentLanguage = value;
                CurrentLanguageChanged.InvokeAsync(CurrentLanguage);
            }
        }
        private bool Italian
        {
            get
            {
                if (SelectedLanguage == LanguageSettings.Italian)
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (value == true)
                {
                    SelectedLanguage = LanguageSettings.Italian;
                    StateHasChanged();
                }
            }
        }
        private bool German
        {
            get
            {
                if (SelectedLanguage == LanguageSettings.German)
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (value == true)
                {
                    SelectedLanguage = LanguageSettings.German;
                    StateHasChanged();
                }
            }
        }
        private bool Ladinisch
        {
            get
            {
                if (SelectedLanguage == LanguageSettings.Ladinisch)
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (value == true)
                {
                    SelectedLanguage = LanguageSettings.Ladinisch;
                    StateHasChanged();
                }
            }
        }
        private bool HasLadinisch = false;

        protected override async Task OnInitializedAsync()
        {
            HasLadinisch = await LangProvider.HasLadinisch();

            StateHasChanged();

            await base.OnInitializedAsync();
        }
    }
}
