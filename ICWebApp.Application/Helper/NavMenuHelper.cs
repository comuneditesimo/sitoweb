using ICWebApp.Application.Interface.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public partial class NavMenuHelper : INavMenuHelper
    {
        public event Action OnChange;
        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
