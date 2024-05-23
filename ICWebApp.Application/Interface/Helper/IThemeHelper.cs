using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IThemeHelper
    {
        public string GetCurrentColor();
        public void SetThemeColor(string Color);
        public string GetFaviconPath(bool GetDefault = false);
    }
}
