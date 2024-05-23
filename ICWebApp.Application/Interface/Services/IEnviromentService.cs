using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IEnviromentService
    {
        public bool IsMobile { get; set; }
        public bool IsTablet { get; set; }
        public bool ShowDownloadPreviewWindow { get; set; }
        public byte[] PreviewFile { get; set; }
        public event Action OnIsMobileChanged;
        public event Action OnIsTabletChanged;
        public event Action OnScreenClicked;
        public event Action OnShowPanoramicChanged;
        public event Action OnLayoutAdditionalCSSChanged;
        public event Action OnShowDownloadPreviewWindow;
        public event Action CloseAllCustomDropdowns;
        public bool ShowMunicipalPanoramic { get; set; }
        public string LayoutAdditionalCSS { get; set; }
        public Task<bool> DownloadFile(byte[] Filedata, string Filename, bool forceDownload = false);
        public bool DownloadFile(string Path, string Filename, bool forceDownload = false);
        public Task<bool> MouseOverDiv(string ID);
        public void NotifyOnScreenClicked();
        public void NotifyCloseAllCustomDropdowns();
        public Task<bool> ScrollToTop();
        public Task<bool> ScrollToElement(string ID);
        public Task<bool> ClickElement(string ID);
        public Task<bool> OpenDropdown(string ID);
        public Task<bool> SetPageTitle(string Title); 
        public Task<string> GetHtml(string DivID);
    }
}
