using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.ActionBar;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class ActionBarService : IActionBarService
    {
        private bool _ShowShareButton = false;
        private bool _ShowDefaultButtons = false;
        private List<ActionBarItem> _Items = new List<ActionBarItem>();
        private List<V_HOME_Theme>? _themeList;
        public event Action OnActionBarChanged; 
        public event Action OnShowShareButtonChanged;
        public event Action OnShowDefaultButtonsChanged;
        public event Action OnThemeListChanged;

        public bool ShowShareButton
        {
            get
            {
                return _ShowShareButton;
            }
            set
            {
                _ShowShareButton = value;
                NotifyShowShareButtonChanged();
            }
        }
        public bool ShowDefaultButtons
        {
            get
            {
                return _ShowDefaultButtons;
            }
            set
            {
                _ShowDefaultButtons = value;
                NotifyShowDefaultButtonsChanged();
            }
        }

        public List<V_HOME_Theme>? ThemeList 
        {
            get
            {
                return _themeList;
            }
            set
            {
                _themeList = value;
                NotifyThemeListChangedChanged();
            }
        }

        public void ClearActionBar()
        {
            _Items.Clear();
            ShowDefaultButtons = false;
            ShowShareButton = false;
            ThemeList = null;
            NotifyActionBarChanged();
        }
        public List<ActionBarItem> GetActionBarItems()
        {
            return _Items;
        }

        public void SetActionBarItem(int SortOrder, string Title, Action Action, string? Icon = null)
        {
            _Items.Add(new ActionBarItem 
            { 
                SortOrder = SortOrder, 
                Title = Title,
                Icon = Icon,
                Action = Action
            });

            NotifyActionBarChanged();
        }  

        private void NotifyActionBarChanged() => OnActionBarChanged?.Invoke();
        private void NotifyShowShareButtonChanged() => OnShowShareButtonChanged?.Invoke();
        private void NotifyShowDefaultButtonsChanged() => OnShowDefaultButtonsChanged?.Invoke();
        private void NotifyThemeListChangedChanged() => OnThemeListChanged?.Invoke();
    }
}
