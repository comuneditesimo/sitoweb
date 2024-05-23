using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Services
{
    public class MetaItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string AudienceType { get; set; }
        public string Url { get; set; }
        public string? Authority { get; set; }
        public string? Cap { get; set; }
        public string? Address { get; set; }
    }
}
