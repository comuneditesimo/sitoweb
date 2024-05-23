using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Table("PAYMENTS_Type", Schema = "dbo")]
    public partial class PAYMENTS_Type
    {
        [Key]
        public Guid ID { get; set; }
        [StringLength(250)]
        public string? Name { get; set; }
    }
}
