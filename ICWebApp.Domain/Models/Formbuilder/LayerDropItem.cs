using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Formbuilder
{
    public class LayerDropItem
    {
        public Guid? ParentID { get; set; }
        public long? SortOrder { get; set; }
        public long? ColumnPos { get; set; }
    }
}
