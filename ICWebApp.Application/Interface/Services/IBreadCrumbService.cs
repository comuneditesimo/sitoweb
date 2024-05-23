using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IBreadCrumbService
    {
        public event Action OnBreadCrumbDataChanged;
        public void AddBreadCrumb(string? RelativeUrl, string? TEXT_SystemsTextCode, string? Icon, string? Text = null, bool Disabled = false);
        public void ClearBreadCrumb();
        public List<BreadCrumbItem> GetBreadCrumb();
        public bool ShowBreadCrumb { get; set; }
        public void NotifyPropertyChanged();
    }
}
