﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class CANTEEN_User
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_User_ID { get; set; }

    [StringLength(16)]
    public string TaxNumber { get; set; }

    [StringLength(400)]
    public string FullName { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Creationdate { get; set; }

    public Guid? MunicipalityID { get; set; }

    [StringLength(8)]
    public string TelPin { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastNotificationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ServiceDisabledDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastTaxReportSentDate { get; set; }

    [InverseProperty("CANTEEN_User")]
    public virtual ICollection<CANTEEN_User_Documents> CANTEEN_User_Documents { get; set; } = new List<CANTEEN_User_Documents>();
}