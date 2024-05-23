using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public partial class PushNotification
    {
        public string app_id { get; set; }
        public List<Filter> filters { get; set; }
        public Dictionary<string, string> data { get; set; }
        public Dictionary<string, string> contents { get; set; }
    }

    public partial class Filter
    {
        public string field { get; set; }
        public string key { get; set; }
        public string relation { get; set; }
        public string value { get; set; }
    }
}
