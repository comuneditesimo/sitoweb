using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Amtstafel
{
    public class TableElement
    {
        public string? Number { get; set; }
        public string? Date { get; set; }
        public string? Description { get; set; }
        public string? Commission { get; set; }
        public string? OnlineFrom { get; set; }
        public string? OnlineTo { get; set; }
        public string? DocumentUrl { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
