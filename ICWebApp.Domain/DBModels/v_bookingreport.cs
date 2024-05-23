using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Keyless]
    public partial class V_Bookingreport
    {
        public Guid? ID { get; set; }
    }
}
