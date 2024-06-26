﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Appointment_Theme
{
    [Key]
    public Guid ID { get; set; }

    public Guid? HOME_Appointment_ID { get; set; }

    public Guid? HOME_Theme_ID { get; set; }

    [ForeignKey("HOME_Appointment_ID")]
    [InverseProperty("HOME_Appointment_Theme")]
    public virtual HOME_Appointment HOME_Appointment { get; set; }

    [ForeignKey("HOME_Theme_ID")]
    [InverseProperty("HOME_Appointment_Theme")]
    public virtual HOME_Theme HOME_Theme { get; set; }
}