﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class SYS_Calendar
{
    [Key]
    public Guid ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CalenderDate { get; set; }

    public bool IsHoliday { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string Month { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string Year { get; set; }

    [StringLength(7)]
    [Unicode(false)]
    public string YearMonth { get; set; }
}