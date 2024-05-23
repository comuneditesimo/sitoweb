using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_HOME_Person_Request
    {
        [NotMapped]
        public Guid Id
        {
            get
            {
                return ID;
            }
            set
            {
                ID = value;
            } 
        }
        [NotMapped]
        public DateTime StartTime 
        {
            get
            {
                if(DateFrom != null)
                    return DateFrom.Value;

                return DateTime.Now;
            }
        }
        [NotMapped]
        public DateTime EndTime
        {
            get
            {
                if (DateTo != null)
                    return DateTo.Value;

                return DateTime.Now;
            }
        }
        [NotMapped]
        public string Subject
        {
            get
            {
                return Person_Fullname;
            }
        }
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
