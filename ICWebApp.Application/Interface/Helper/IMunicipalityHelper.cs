using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IMunicipalityHelper
    {
        public Task<string> GetMunicipalBasePath(Guid AUTH_Municipality_ID);
    }
}
