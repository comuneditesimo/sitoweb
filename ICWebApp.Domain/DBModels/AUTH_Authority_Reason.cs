﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class AUTH_Authority_Reason
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_Authority_ID { get; set; }

    [ForeignKey("AUTH_Authority_ID")]
    [InverseProperty("AUTH_Authority_Reason")]
    public virtual AUTH_Authority AUTH_Authority { get; set; }

    [InverseProperty("AUTH_Authority_Reason")]
    public virtual ICollection<AUTH_Authority_Reason_Extended> AUTH_Authority_Reason_Extended { get; set; } = new List<AUTH_Authority_Reason_Extended>();
}