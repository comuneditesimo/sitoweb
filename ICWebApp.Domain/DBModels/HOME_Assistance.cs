﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Assistance
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string EMail { get; set; }

    public Guid? CategoryID { get; set; }

    public Guid? ServiceID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    public bool Completed { get; set; }

    public string Description { get; set; }

    public bool Privacy { get; set; }
}