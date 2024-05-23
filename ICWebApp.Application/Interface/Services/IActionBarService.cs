using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.ActionBar;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IActionBarService
    {
        public bool ShowShareButton { get; set; }
        public bool ShowDefaultButtons { get; set; }
        public List<V_HOME_Theme>? ThemeList { get; set; }
        public event Action OnShowShareButtonChanged;
        public event Action OnShowDefaultButtonsChanged;
        public event Action OnActionBarChanged;
        public event Action OnThemeListChanged;
        public List<ActionBarItem> GetActionBarItems();
        public void SetActionBarItem(int SortOrder, string Title, Action Action, string? Icon = null);
        public void ClearActionBar();
    }
}
