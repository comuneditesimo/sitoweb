﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class CANTEEN_Subscriber_Card_Status
{
    [Key]
    public int ID { get; set; }

    public string Description { get; set; }

    public string TEXT_SystemTexts_Code { get; set; }

    public string Icon { get; set; }

    public bool IsSelectable { get; set; }
}