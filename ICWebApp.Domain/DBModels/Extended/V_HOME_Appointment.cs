using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_HOME_Appointment
    {
        [NotMapped]
        public DateTime? FromDateYearMonth
        {
            get
            {
                if (DateFrom != null)
                {
                    return new DateTime(DateFrom.Value.Year, DateFrom.Value.Month, 1, 0, 0, 0);
                }

                return null;
            }
        }
    }
}
