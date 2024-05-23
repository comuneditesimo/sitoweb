using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Keyless]
    [Table("_import_cap", Schema = "dbo")]
    public partial class _import_cap
    {
        [StringLength(50)]
        public string? cat { get; set; }
        [StringLength(50)]
        public string? cap { get; set; }
    }
}
