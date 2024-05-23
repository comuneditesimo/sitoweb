using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ICWebApp.Domain.Models.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_ROOM_Rooms
    {       
        [NotMapped]
        public bool IsSelected { get; set; } = false;
        [NotMapped]
        public bool Disabled { get; set; } = false;

        [NotMapped]
        public double[] LatLng
        { 
            get
            {
                if (Lat != null && Lng != null)
                {
                    return new double[] { Lat.Value, Lng.Value };
                }

                return new double[] { 0, 0 };
            }
        }
        [NotMapped]
        public string CombinedName
        {
            get
            {
                if(BuildingName != null)
                    return BuildingName + " - " + Name;

                return Name;
            }
        }
        [NotMapped]
        public List<string>? BookableErrors { get; set; }
    }
}
    