using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class HOME_Person_Dates_Timeslots
    {
        [NotMapped]
        public DayOfWeek? DayOfweek
        {
            get 
            {
                if (Weekday == 0)
                    return DayOfWeek.Monday;
                if (Weekday == 1)
                    return DayOfWeek.Tuesday;
                if (Weekday == 2)
                    return DayOfWeek.Wednesday;
                if (Weekday == 3)
                    return DayOfWeek.Thursday;
                if (Weekday == 4)
                    return DayOfWeek.Friday;
                if (Weekday == 5)
                    return DayOfWeek.Saturday;
                if (Weekday == 6)
                    return DayOfWeek.Sunday;

                return null;
            }
        }
    }
}
