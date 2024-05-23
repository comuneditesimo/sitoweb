using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IRequestAdministrationHelper
    {
        public Administration_Filter_Request? Filter { get; set; }
        public Administration_Filter_Organization OrgFilter { get; set; }
    }
}
