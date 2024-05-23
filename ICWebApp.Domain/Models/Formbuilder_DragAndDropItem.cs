using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public partial class Formbuilder_DragAndDropItem
    {
        public long? Count { get; set; }
        public long? ColumnPos { get; set; }
        public Guid? ParentID { get; set; }
    }
}
