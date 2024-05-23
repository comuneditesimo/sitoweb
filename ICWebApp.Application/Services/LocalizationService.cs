using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalizationService(IJSRuntime jsRuntime)
        {
            this._jsRuntime = jsRuntime;
        }
        public async Task<string> GetBrowserCulture()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("browserJsFunctions.getLanguage");
            }
            catch
            {
                return "de-DE";
            }
        }
    }
}
