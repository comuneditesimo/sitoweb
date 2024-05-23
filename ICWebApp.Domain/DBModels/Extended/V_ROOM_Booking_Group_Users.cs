using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_ROOM_Booking_Group_Users
    {

        [NotMapped]
        public string? NameLong
        {
            get
            {
                return Name + " (" + FiscalNumber + ")";
            }
        }
    }
}
