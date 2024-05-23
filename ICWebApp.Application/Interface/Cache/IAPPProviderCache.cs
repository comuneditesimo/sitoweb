using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Cache
{
    public interface IAPPProviderCache
    {
        public void Clear();
        public List<APP_Applications_Url> Get();
        public void Set(List<APP_Applications_Url> Data);
    }
}
