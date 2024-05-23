using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class JSRuntimeEventHelper
    {
        private readonly Func<string, Task> _callback;

        public JSRuntimeEventHelper(Func<string, Task> callback)
        {
            _callback = callback;
        }

        [JSInvokable]
        public Task OnEvent(string args) => _callback(args);
    }
}
