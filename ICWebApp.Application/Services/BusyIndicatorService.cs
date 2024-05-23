using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class BusyIndicatorService : IBusyIndicatorService
    {
        private bool _IsBusy = false;
        private bool _StartupBusy = true;
        private double? _TotalProgress = null;
        private double? _CurrentProgress = null;
        private string _Description = null;
        public event Action<bool> OnBusyIndicatorChanged;
        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }
            set
            {
                _IsBusy = value;
                _StartupBusy = value;
                NotifyPropertyChanged();
            }
        }
        public bool StartupBusy
        {
            get
            {
                return _StartupBusy;
            }
        }
        public double? TotalProgress
        {
            get
            {
                return _TotalProgress;
            }
            set
            {
                _TotalProgress = value;
                NotifyPropertyChanged();
            }
        }
        public double? CurrentProgress
        {
            get
            {
                return _CurrentProgress;
            }
            set
            {
                _CurrentProgress = value;
                NotifyPropertyChanged();
            }
        }
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
                NotifyPropertyChanged();
            }
        }
        private async void NotifyPropertyChanged()
        {
            if (IsBusy == false)
            {
                await Task.Delay(100);
            }
            OnBusyIndicatorChanged?.Invoke(_IsBusy);
        }
    }
}
