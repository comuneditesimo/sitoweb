using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Spid
{
    public class RequestBody
    {
        public string applicationId { get; set; }
        public string returnURL { get; set; }
        public string returnURLError { get; set; }
        public bool autoRedirectOnError { get; set; }
        public string type { get; set; }
        public string attrSet { get; set; }
        public string serviceName { get; set; }
    }
}
