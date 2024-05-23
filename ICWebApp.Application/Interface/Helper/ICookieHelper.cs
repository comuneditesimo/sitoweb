using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface ICookieHelper
    {
        public Task Write(string Name, string Value);
        public Task<string> ReadCookies(string Name);
        public Task Remove(string Name);
    }
}
