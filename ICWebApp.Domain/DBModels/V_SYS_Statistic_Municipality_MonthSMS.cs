﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_SYS_Statistic_Municipality_MonthSMS
{
    public Guid? ID { get; set; }

    public Guid AUTH_Municipality_ID { get; set; }

    public int? CreatedMonth { get; set; }

    public int? CreatedYear { get; set; }

    [StringLength(250)]
    public string InternalName { get; set; }

    public int? TotalNumber { get; set; }

    [StringLength(9)]
    [Unicode(false)]
    public string YearMonth { get; set; }
}