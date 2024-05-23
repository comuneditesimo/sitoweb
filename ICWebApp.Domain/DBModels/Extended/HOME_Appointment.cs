using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class HOME_Appointment
    {
        [NotMapped]
        public FILE_FileInfo Image { get; set; }
    }
}
