using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_TASK_Statistik_Dashboard
    {
        [NotMapped]
        public bool ShowSubContent { get; set; }
        [NotMapped]
        public List<V_TASK_Task?>? TaskList { get; set; }
    }
}
