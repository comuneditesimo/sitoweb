using ICWebApp.Application.Helper;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface ICanteenAdministrationHelper
    {
        public Administration_Filter_CanteenSubscriptions? Filter { get; set; }
    }
}
