using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IBusyIndicatorService
    {
        public bool IsBusy { get; set; }
        public bool StartupBusy { get; }
        public double? TotalProgress { get; set; }
        public double? CurrentProgress { get; set; }
        public string Description { get; set; }
        public event Action<bool> OnBusyIndicatorChanged;
    }
}
