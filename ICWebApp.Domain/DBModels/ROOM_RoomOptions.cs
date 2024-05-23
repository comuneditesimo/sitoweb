﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class ROOM_RoomOptions
{
    [Key]
    public Guid ID { get; set; }

    public Guid ROOM_Room_ID { get; set; }

    public double? BasePrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    public int? Pos { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public Guid? ROOM_RoomOptions_Category_ID { get; set; }

    public bool? Enabled { get; set; }

    public int? Quantity { get; set; }

    public bool? DailyPayment { get; set; }

    [ForeignKey("ROOM_Room_ID")]
    [InverseProperty("ROOM_RoomOptions")]
    public virtual ROOM_Rooms ROOM_Room { get; set; }

    [InverseProperty("ROOM_RoomOptions")]
    public virtual ICollection<ROOM_RoomOptions_Contact> ROOM_RoomOptions_Contact { get; set; } = new List<ROOM_RoomOptions_Contact>();

    [InverseProperty("ROOM_RoomOptions")]
    public virtual ICollection<ROOM_RoomOptions_Extended> ROOM_RoomOptions_Extended { get; set; } = new List<ROOM_RoomOptions_Extended>();

    [InverseProperty("ROOM_Rooms_Options")]
    public virtual ICollection<ROOM_RoomOptions_Positions> ROOM_RoomOptions_Positions { get; set; } = new List<ROOM_RoomOptions_Positions>();
}