using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Amministration
{
    public class AmministrationtItem
    {
        public string? Title { get; set; }
        public string? DescriptionShort { get; set; }
        public string? Url { get; set; }
        public bool Highlight { get; set; } = false;
    }
}
