using ICWebApp.DataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface ILocalizationService
    {
        public Task<string> GetBrowserCulture();
    }
}
