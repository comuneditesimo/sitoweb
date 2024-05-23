using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IFormAdministrationHelper
    {
        public Administration_Filter_Item? Filter { get; set; }
    }
}
