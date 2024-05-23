using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class ORG_Request_Parameter
    {
        public string? FiscalCode { get; set; }
        public List<AUTH_Users>? Organisations { get; set; }
    }
}
