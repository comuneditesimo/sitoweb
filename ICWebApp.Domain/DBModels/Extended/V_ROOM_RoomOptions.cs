using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_ROOM_RoomOptions
    {
        [NotMapped]
        public bool IsSelected { get; set; } = false;
    }
}
