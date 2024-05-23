using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.ActionBar
{
    public class ActionBarItem
    {
        public int SortOrder { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public Action Action { get; set; }
    }
}
