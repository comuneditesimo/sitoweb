using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Textvorlagen
{
    public class TemplateItem
    {
        public Guid LANG_Language_ID { get; set; }
        public string Content { get; set; }
    }
}
