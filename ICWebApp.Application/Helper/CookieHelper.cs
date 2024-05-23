using ICWebApp.Application.Interface.Helper;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class CookieHelper : ICookieHelper
    {
        private IJSRuntime _jsRuntime;
        public CookieHelper(IJSRuntime _jsRuntime)
        {
            this._jsRuntime = _jsRuntime;
        }
        public async Task Write(string Name, string Value)
        {
            await _jsRuntime.InvokeAsync<object>("WriteCookie.WriteCookie", Name, Value, DateTime.Now.AddHours(1));
        }
        public async Task<string> ReadCookies(string Name)
        {
            return await _jsRuntime.InvokeAsync<string>("ReadCookie.ReadCookie", Name);
        }
        public async Task Remove(string Name)
        {
            await _jsRuntime.InvokeAsync<string>("RemoveCookie.RemoveCookie", Name);
        }
    }
}
