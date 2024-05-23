using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.AdobeSign
{
    public class Token
    {
        public string? expires_in { get; set; }
        public string? access_token { get; set; }
        public string? web_access_point { get; set; }
        public string? api_access_point { get; set; }
        public string? refresh_token { get; set; }
    }
}
