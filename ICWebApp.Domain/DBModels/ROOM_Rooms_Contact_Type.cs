﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class ROOM_Rooms_Contact_Type
{
    [Key]
    public Guid ID { get; set; }

    public Guid? ROOM_Rooms_Contact_ID { get; set; }

    public Guid? ROOM_Contact_Type_ID { get; set; }

    [ForeignKey("ROOM_Contact_Type_ID")]
    [InverseProperty("ROOM_Rooms_Contact_Type")]
    public virtual ROOM_Contact_Type ROOM_Contact_Type { get; set; }

    [ForeignKey("ROOM_Rooms_Contact_ID")]
    [InverseProperty("ROOM_Rooms_Contact_Type")]
    public virtual ROOM_Rooms_Contact ROOM_Rooms_Contact { get; set; }
}