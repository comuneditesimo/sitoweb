using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Textvorlagen
{
    public class StatusItem
    {
        public string ID { get; set; }
        public string Context { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public long SortOrder { get; set; }
    }
}
