using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class MunicipalityDomainSelectableItem
    {
        public string? Prefix { get; set; }
        public Guid? AUTH_Municipality_ID { get; set; }
        public Guid? LANG_Language_ID { get; set; }
    }
}
