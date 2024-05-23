using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class TemplateToTextItem
    {
        public string? TitleDE { get; set; } = "";
        public string? TitleIT { get; set; } = "";

        public string? TextDE { get; set; } = "";
        public string? TextIT { get; set; } = "";
        public string? Height { get; set; } = "300px";
        public bool ShowButton { get; set; } = false;
        public bool ShowGerman { get; set; } = true;
        public bool ShowItalian { get; set; } = true;
    }
}
