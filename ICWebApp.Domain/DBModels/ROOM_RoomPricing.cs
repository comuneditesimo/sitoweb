﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Index("ID", Name = "room_roompricing_id_uindex", IsUnique = true)]
public partial class ROOM_RoomPricing
{
    [Key]
    public Guid ID { get; set; }

    public Guid? ROOM_Rooms_ID { get; set; }

    public Guid? AUTH_Company_Type_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    [Column(TypeName = "money")]
    public decimal? MinPrice { get; set; }

    [Column(TypeName = "money")]
    public decimal? MaxPrice { get; set; }

    [Column(TypeName = "money")]
    public decimal? HourPrice { get; set; }

    public bool? Default { get; set; }

    [Column(TypeName = "money")]
    public decimal? FullDayPrice { get; set; }

    [Column(TypeName = "money")]
    public decimal? HalfDayPrice { get; set; }

    public long? HalfDayHourLimit { get; set; }

    [ForeignKey("AUTH_Company_Type_ID")]
    [InverseProperty("ROOM_RoomPricing")]
    public virtual AUTH_Company_Type AUTH_Company_Type { get; set; }

    [ForeignKey("ROOM_Rooms_ID")]
    [InverseProperty("ROOM_RoomPricing")]
    public virtual ROOM_Rooms ROOM_Rooms { get; set; }
}