﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_FORM_Application_Ressource
{
    public Guid ID { get; set; }

    public Guid? FORM_Application_ID { get; set; }

    public Guid? FILE_FileInfo_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RemovedAt { get; set; }

    public bool UserUpload { get; set; }

    public bool BolloRequested { get; set; }

    public Guid? PAY_Transaction_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentDate { get; set; }
}