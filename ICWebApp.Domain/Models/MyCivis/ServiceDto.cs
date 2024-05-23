using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.MyCivis
{
    public class ServiceDto
    {
        public string? ServiceUid { get; set; }
        public MultiLang? Name { get; set; }
        public MultiLang? Url{ get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTill { get; set; }
    }
}
