using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IAPPProvider
    {
        public Task<List<APP_Applications_Url>> GetAppUrl();
        public Task<bool> HasApplicationAsync(Guid MunicipalityID, Guid ApplicationID, bool WithPreparation = false);
        public bool HasApplication(Guid MunicipalityID, Guid ApplicationID, bool WithPreparation = false);
    }
}
