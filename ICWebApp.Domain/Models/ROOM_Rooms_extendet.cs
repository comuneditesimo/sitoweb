using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.DBModels
{
    public partial class ROOM_Rooms
    {
        [NotMapped]
        public double[] LatLng
        {
            get
            {
                if (Lat != null && Lng != null)
                {
                    return new double[] { Lat.Value, Lng.Value };
                }

                return new double[] { 0,0 };
            }
        }
    }
}
