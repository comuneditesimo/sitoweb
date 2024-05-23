﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Index("ID", Name = "canteen_menu_id_uindex", IsUnique = true)]
public partial class CANTEEN_MealMenu
{
    [Key]
    public Guid ID { get; set; }

    [StringLength(400)]
    public string Name { get; set; }

    public string TEXT_SystemTexts_Code { get; set; }

    public int? SortOrder { get; set; }

    public bool Special { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public bool Standard { get; set; }

    public bool? NoPork { get; set; }

    public bool? NoMeat { get; set; }

    [InverseProperty("CanteenMenu")]
    public virtual ICollection<CANTEEN_Subscriber> CANTEEN_Subscriber { get; set; } = new List<CANTEEN_Subscriber>();
}