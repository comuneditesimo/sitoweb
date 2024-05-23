using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class BreadCrumbItem
    {
        public int ID { get; set; }
        public string Icon { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public bool Disabled { get; set; } = false;
    }
}
