using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using sib_api_v3_sdk.Model;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class EnviromentService : IEnviromentService
    {
        private bool _isMobile;
        private bool _isTablet;
        private bool _ShowMunicipalPanoramic = false;
        private bool _ShowDownloadPreviewWindow = false;
        private string _LayoutAdditionalCSS = "";
        private readonly IJSRuntime _jSRuntime;
        private readonly NavigationManager NavManager;
        private DotNetObjectReference<JSRuntimeEventHelper>? Reference;
        private IFILEProvider FileProvider;
        private ISessionWrapper SessionWrapper;
        public bool ShowMunicipalPanoramic
        {
            get
            {
                return _ShowMunicipalPanoramic;
            }
            set
            {
                if (_ShowMunicipalPanoramic != value)
                {
                    _ShowMunicipalPanoramic = value;
                    NotifyOnPanoramicChanged();
                }
            }
        }
        public string LayoutAdditionalCSS
        {
            get
            {
                return _LayoutAdditionalCSS;
            }
            set
            {
                if (_LayoutAdditionalCSS != value)
                {
                    _LayoutAdditionalCSS = value;
                    NotifyLayoutAdditionalCSSChanged();
                }
            }
        }
        public event Action OnIsMobileChanged;
        public event Action OnIsTabletChanged;
        public event Action OnScreenClicked;
        public event Action OnShowPanoramicChanged;
        public event Action OnLayoutAdditionalCSSChanged;
        public event Action OnShowDownloadPreviewWindow;
        public event Action CloseAllCustomDropdowns;

        public bool IsMobile
        {
            get
            {
                return _isMobile;
            }
            set
            {
                if (_isMobile != value)
                {
                    _isMobile = value;
                    NotifyOnIsMobileChanged();
                }
            }
        }
        public bool IsTablet
        {
            get
            {
                return _isTablet;
            }
            set
            {
                if (_isTablet != value)
                {
                    _isTablet = value;
                    NotifyOnIsTabletChanged();
                }
            }
        }
        public bool ShowDownloadPreviewWindow
        {
            get
            {
                return _ShowDownloadPreviewWindow;
            }
            set
            {
                if (_ShowDownloadPreviewWindow != value)
                {
                    _ShowDownloadPreviewWindow = value;
                    NotifyShowDownloadPreviewWindow();
                }
            }
        }
        public byte[] PreviewFile { get; set; }

        public EnviromentService(IJSRuntime _jSRuntime, NavigationManager NavManager, IFILEProvider FileProvider, ISessionWrapper SessionWrapper)
        {
            this._jSRuntime = _jSRuntime;
            this.NavManager = NavManager;
            this.FileProvider = FileProvider;
            this.SessionWrapper = SessionWrapper;

            SetupScreenWidthEvent(args => HandleIsWidthChanged(args));
        }
        private ValueTask<string> SetupScreenWidthEvent(Func<string, System.Threading.Tasks.Task> callback)
        {
            Reference = DotNetObjectReference.Create(new JSRuntimeEventHelper(callback));
            var minInterval = 500;
            var result = _jSRuntime.InvokeAsync<string>("enviromentHelper_AddScreenWidthListener", Reference, minInterval);

            return result;
        }
        [JSInvokable]
        public async System.Threading.Tasks.Task HandleIsWidthChanged(string Args)
        {
            int Width = 1600;

            if (Args != null)
            {
                int.TryParse(Args.ToString(), out Width);
            }

            if (Width <= 1000)
            {
                if (IsMobile == false)
                {
                    IsMobile = true;
                    NotifyOnIsMobileChanged();
                }
            }
            else
            {
                if (IsMobile == true)
                {
                    IsMobile = false;
                    NotifyOnIsMobileChanged();
                }
            }

            if (Width <= 1700)
            {
                if (IsTablet == false)
                {
                    IsTablet = true;
                    NotifyOnIsTabletChanged();
                }
            }
            else
            {
                if (IsTablet == true)
                {
                    IsTablet = false;
                    NotifyOnIsTabletChanged();
                }
            }

            return;
        }
        private void NotifyOnIsMobileChanged() => OnIsMobileChanged?.Invoke();
        private void NotifyOnIsTabletChanged() => OnIsTabletChanged?.Invoke();
        private void NotifyOnPanoramicChanged() => OnShowPanoramicChanged?.Invoke();
        private void NotifyLayoutAdditionalCSSChanged() => OnLayoutAdditionalCSSChanged?.Invoke();
        public void NotifyOnScreenClicked()
        {
            OnScreenClicked?.Invoke();
        }
        public void NotifyShowDownloadPreviewWindow()
        {
            OnShowDownloadPreviewWindow?.Invoke();
        }
        public void NotifyCloseAllCustomDropdowns()
        {
            CloseAllCustomDropdowns?.Invoke();
        }
        public async Task<bool> DownloadFile(byte[] Filedata, string Filename, bool forceDownload = false)
        {
            var Path = "D:/Comunix/Cache/Download/" + Guid.NewGuid().ToString();

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            System.IO.File.WriteAllBytes(Path + "/" + Filename, Filedata);

            DownloadFile(Path + "/" + Filename, Filename, forceDownload);

            return true;
        }
        public bool DownloadFile(string Path, string Filename, bool forceDownload = false)
        {
            if (!string.IsNullOrEmpty(Path))
            {
                if (Filename != null && Filename.EndsWith(".pdf") && forceDownload == false)
                {
                    var byteArray = System.IO.File.ReadAllBytes(Path);

                    PreviewFile = byteArray;

                    ShowDownloadPreviewWindow = true;
                }
                else if (SessionWrapper.CurrentUser != null)
                {
                    FILE_Download_Log verifLog = new FILE_Download_Log();

                    verifLog.ID = Guid.NewGuid();
                    verifLog.AUTH_Users_ID = SessionWrapper.CurrentUser.ID;

                    verifLog.DownloadToken = Guid.NewGuid();
                    verifLog.DownloadExpirationDate = DateTime.Now.AddSeconds(10);

                    FileProvider.SetDownloadLog(verifLog);

                    _jSRuntime.InvokeAsync<object>("BlazorDownloadFileFromPath", Path, Filename, verifLog.DownloadToken.ToString());
                }
                else if (SessionWrapper.CurrentMunicipalUser != null)
                {
                    FILE_Download_Log verifLog = new FILE_Download_Log();

                    verifLog.ID = Guid.NewGuid();
                    verifLog.AUTH_Users_ID = SessionWrapper.CurrentMunicipalUser.ID;

                    verifLog.DownloadToken = Guid.NewGuid();
                    verifLog.DownloadExpirationDate = DateTime.Now.AddSeconds(10);

                    FileProvider.SetDownloadLog(verifLog);

                    _jSRuntime.InvokeAsync<object>("BlazorDownloadFileFromPath", Path, Filename, verifLog.DownloadToken.ToString());
                }
                else
                {
                    FILE_Download_Log verifLog = new FILE_Download_Log();

                    verifLog.ID = Guid.NewGuid();

                    verifLog.DownloadToken = Guid.NewGuid();
                    verifLog.DownloadExpirationDate = DateTime.Now.AddSeconds(10);

                    FileProvider.SetDownloadLog(verifLog);

                    _jSRuntime.InvokeAsync<object>("BlazorDownloadFileFromPath", Path, Filename, verifLog.DownloadToken.ToString());
                }

                return true;
            }

            return false;
        }
        public async Task<bool> MouseOverDiv(string ID)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                var result = await _jSRuntime.InvokeAsync<string>("enviromentHelper_MouseOverDiv", ID);

                if (result == "true")
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> ScrollToTop()
        {
            await _jSRuntime.InvokeVoidAsync("enviromentHelper_SmoothScrollToTop");

            return true;
        }
        public async Task<bool> ClickElement(string ID)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                var result = await _jSRuntime.InvokeAsync<object>("enviromentHelper_clickElement", ID);

                if (result != null && result.ToString() == "true")
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> OpenDropdown(string ID)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                var result = await _jSRuntime.InvokeAsync<object>("environmentHelper_openDropdown", ID);

                if (result != null && result.ToString() == "true")
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> ScrollToElement(string ID)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                var result = await _jSRuntime.InvokeAsync<object>("enviromentHelper_scrollToBottom", ID);

                if (result != null && result.ToString() == "true")
                {
                    return true;
                }
            }

            return false;

        }
        public async Task<bool> SetPageTitle(string Title)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                await _jSRuntime.InvokeAsync<object>("enviromentHelper_SetPageTitle", Title);
            }

            return true;
        }
        public async Task<string> GetHtml(string DivID)
        {
            return await _jSRuntime.InvokeAsync<string>("enviromentHelper_GetHtml", DivID);
        }
    }
}
