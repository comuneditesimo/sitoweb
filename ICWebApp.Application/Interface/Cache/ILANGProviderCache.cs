using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Cache
{
    public interface ILANGProviderCache
    {
        public List<LANG_Languages>? Get();
        public LANG_Languages? Get(string LanguageCode);
        public LANG_Languages? Get(Guid ID);
        public void Save(LANG_Languages Data);
        public void Clear();
    }
}
