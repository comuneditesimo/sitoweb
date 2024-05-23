using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class CanteenAdministrationHelper : ICanteenAdministrationHelper
    {
        private Administration_Filter_CanteenSubscriptions? _filter;
        public Administration_Filter_CanteenSubscriptions? Filter { get => _filter; set => _filter = value; }
    }
}
