﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class AUTH_Authority_Dates_Closed
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_Authority_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Date { get; set; }

    [ForeignKey("AUTH_Authority_ID")]
    [InverseProperty("AUTH_Authority_Dates_Closed")]
    public virtual AUTH_Authority AUTH_Authority { get; set; }
}