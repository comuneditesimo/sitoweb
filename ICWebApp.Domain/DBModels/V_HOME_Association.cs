﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_HOME_Association
{
    public Guid ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public string PhoneNr { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastChangeDate { get; set; }

    public Guid? Managed_HOME_Person_ID { get; set; }

    public Guid? Managed_AUTH_Authority_ID { get; set; }

    public bool Highlight { get; set; }

    public Guid? HOME_Association_Type_ID { get; set; }

    public Guid? FILE_FileInfo_ID { get; set; }

    public string PhoneNrMobile { get; set; }

    public double? Lat { get; set; }

    public double? Lang { get; set; }

    public string Description { get; set; }

    public string DescriptionShort { get; set; }

    public string EMail { get; set; }

    public string PecEmail { get; set; }

    public string Website { get; set; }

    public Guid? LANG_Language_ID { get; set; }

    public string Title { get; set; }

    public string Adress { get; set; }

    public string Type { get; set; }
}