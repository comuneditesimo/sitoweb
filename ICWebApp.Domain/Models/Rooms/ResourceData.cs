using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Rooms
{
    public class ResourceData
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
        public Guid ExternalID { get; set; }
        public string CSS { get; set; }
    }
}
