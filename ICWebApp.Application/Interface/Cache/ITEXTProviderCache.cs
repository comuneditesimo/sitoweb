using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Cache
{
    public interface ITEXTProviderCache
    {
        public TEXT_SystemTexts? Get(string Code, Guid LANG_Language_ID);
        public void Save(TEXT_SystemTexts Data);
        public void Clear();
    }
}
