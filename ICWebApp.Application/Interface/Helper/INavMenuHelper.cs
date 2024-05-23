using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface INavMenuHelper
    {
        public event Action OnChange;
        public void NotifyStateChanged();
    }
}
