using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class RequestAdministrationHelper : IRequestAdministrationHelper
    {
        private Administration_Filter_Request? _filter;
        private Administration_Filter_Organization? _OrgFilter;
        public Administration_Filter_Request? Filter { get => _filter; set => _filter = value; }
        public Administration_Filter_Organization? OrgFilter { get => _OrgFilter; set => _OrgFilter = value; }
    }
}
