using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class AUTH_Municipality
    {
        [NotMapped] public string Name { get; set; }
        [NotMapped] public string Prefix { get; set; }
        [NotMapped] public double[] LatLan
        {
            get
            {
                if (Lat != null && Lng != null)
                {
                    return new double[] { Lat.Value, Lng.Value };
                }

                return new double[] {0,0};
            }
            set
            {

            }
        }
        [NotMapped] public Guid? LangID { get; set; }
    }
}
