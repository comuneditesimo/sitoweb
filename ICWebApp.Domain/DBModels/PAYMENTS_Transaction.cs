using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Table("PAYMENTS_Transaction", Schema = "dbo")]
    public partial class PAYMENTS_Transaction
    {
        [Key]
        public Guid ID { get; set; }
        public Guid? UserID { get; set; }
        public Guid? ApplicationID { get; set; }
        [Column(TypeName = "timestamp(3) without time zone")]
        public DateTime? PayDate { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        public double? AmountToPay { get; set; }
        public Guid? TypeID { get; set; }
    }
}
