using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Services
{
    public class ServiceKategorieItems
    {
        public Guid ID { get; set; }
        public Guid? Auth_Municipality_ID { get; set; }
        public Guid? LANG_Language_ID { get; set; }
        public string Title { get; set; }
        public string? ShortText { get; set; }
        public string? Url { get; set; }
    }
}
