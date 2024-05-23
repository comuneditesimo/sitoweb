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
    public class AnchorService : IAnchorService
    {
        private NavigationManager _navManager;
        private List<AnchorItem> AnchorList = new List<AnchorItem>();
        private bool _forceShow = false;
        private bool _skipForceReset = false;

        public bool ForceShow 
        { 
            get
            {
                return _forceShow;
            }
            set
            {
                _forceShow = value;
                NotifyForceShowChanged();
            }
        }

        public bool SkipForceReset 
        { 
            get => _skipForceReset; 
            set => _skipForceReset = value; 
        }

        public event Action OnAnchorChanged;
        public event Action OnFoceShowChanged;

        public AnchorService(NavigationManager _navManager)
        {
            this._navManager = _navManager;
            _navManager.LocationChanged += _navManager_LocationChanged;
        }

        private void _navManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            AnchorList.Clear();
            if (!SkipForceReset)
            {
                ForceShow = false;
            }
            NotifyPropertyChanged();
        }

        public void AddAnchor(string Title, string ID, int Order)
        {
            if (AnchorList.FirstOrDefault(p => p.ID == ID) == null)
            {
                AnchorList.Add(new AnchorItem() { Title = Title, ID = ID, Order = Order });
                NotifyPropertyChanged();
            }
        }
        public List<AnchorItem> GetAnchors()
        {
            return AnchorList;
        }
        private void NotifyPropertyChanged() => OnAnchorChanged?.Invoke();
        private void NotifyForceShowChanged() => OnFoceShowChanged?.Invoke();
        public void ClearAnchors()
        {
            AnchorList.Clear();
            NotifyPropertyChanged();
        }
        public void RemoveAnchorByOrder(int Order)
        {
            var anchorItem = AnchorList.FirstOrDefault(p => p.Order == Order);

            if(anchorItem != null)
            {
                AnchorList.Remove(anchorItem);
            }

            NotifyPropertyChanged();
        }
    }
}
