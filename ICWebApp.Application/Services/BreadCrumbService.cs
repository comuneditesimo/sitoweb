using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class BreadCrumbService : IBreadCrumbService
    {
        private ISessionWrapper _sessionWrapper;
        private ITEXTProvider _textProvider;
        private NavigationManager _navManager;

        public BreadCrumbService(ISessionWrapper _sessionWrapper, NavigationManager _navManager, ITEXTProvider _textProvider)
        {
            this._sessionWrapper = _sessionWrapper;
            this._navManager = _navManager;
            this._textProvider = _textProvider;
        }

        private List<BreadCrumbItem> BreadCrumbList = new List<BreadCrumbItem>();
        public bool ShowBreadCrumb { get; set; } = true;
        public event Action OnBreadCrumbDataChanged;

        private BreadCrumbItem GetHomeItem()
        {
            var item = new BreadCrumbItem();

            item.ID = 0;
            item.Text = _textProvider.Get("BREADCRUMB_HOME");

            if (_sessionWrapper != null && !_sessionWrapper.PageIsPublic)
            {
                item.Url = _navManager.BaseUri.TrimEnd('/') + "/Backend/Landing";
            }
            else
            {
                item.Url = "/";
                item.Disabled = false;
            }

            return item;
        }
        public void NotifyPropertyChanged() => OnBreadCrumbDataChanged?.Invoke();
        public void AddBreadCrumb(string? RelativeUrl, string? TEXT_SystemsTextCode, string? Icon, string? Text = null, bool Disabled = false)
        {
            string BaseUri = _navManager.BaseUri.TrimEnd('/');
            ShowBreadCrumb = true;

            var item = new BreadCrumbItem();

            if (Icon != null)
            {
                item.Icon = Icon;
            }

            if (TEXT_SystemsTextCode != null && Text == null)
            {
                item.Text = _textProvider.GetOrReturnCode(TEXT_SystemsTextCode);
            }

            if(Text != null)
            {
                item.Text = Text;
            }

            if (RelativeUrl != null)
            {
                item.Url = Path.Combine(BaseUri, RelativeUrl);
            }

            if (Disabled)
            {
                item.Disabled = Disabled;
            }

            item.ID = BreadCrumbList.Count() + 1;

            BreadCrumbList.Add(item);
            NotifyPropertyChanged();
        }
        public void ClearBreadCrumb()
        {
            BreadCrumbList.Clear();
            NotifyPropertyChanged();
        }
        public List<BreadCrumbItem> GetBreadCrumb()
        {
            var HomeItem = GetHomeItem();

            if (BreadCrumbList.FirstOrDefault(p => p.ID == 0) == null)
            {
                BreadCrumbList.Insert(0, HomeItem);
            }

            return BreadCrumbList;
        }
    }
}
