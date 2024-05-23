﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Request
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    public Guid? AUTH_Authority_ID { get; set; }

    public Guid? AUTH_Authority_Reason_ID { get; set; }

    public string Address { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateFrom { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateTo { get; set; }

    public string Details { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Fiscalnumber { get; set; }

    public string EMail { get; set; }

    public string Phone { get; set; }

    public Guid? AUTH_Users_ID { get; set; }
}