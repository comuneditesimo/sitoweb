using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class AnchorItem
    {
        public string Title { get; set; }
        public string ID { get; set; }
        public int Order { get; set; } = 99;
    }
}
