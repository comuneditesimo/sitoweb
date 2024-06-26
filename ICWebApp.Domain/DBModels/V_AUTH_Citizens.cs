﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_AUTH_Citizens
{
    public Guid ID { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string FiscalNumber { get; set; }

    public bool? EmailConfirmed { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool VeriffConfirmed { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginTimeStamp { get; set; }

    public string Email { get; set; }

    [StringLength(50)]
    public string PhoneNumber { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    [StringLength(250)]
    public string InternalName { get; set; }

    public string RegisteredOffice { get; set; }
}