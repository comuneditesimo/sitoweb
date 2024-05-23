﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class AUTH_Authority
{
    [Key]
    public Guid ID { get; set; }

    public string TEXT_SystemText_Code { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public string Name { get; set; }

    public string Icon { get; set; }

    public long? SortOrder { get; set; }

    public string Mail { get; set; }

    public string Telefon { get; set; }

    public bool IsOfficial { get; set; }

    public bool DynamicForms { get; set; }

    public Guid? CONF_MainMenu_ID { get; set; }

    public bool IsSubstitution { get; set; }

    public bool IsRooms { get; set; }

    public bool IsMensa { get; set; }

    public string MailIT { get; set; }

    [StringLength(2)]
    public string IdentificationLetters { get; set; }

    public int? NextFormIndex { get; set; }

    public bool IsMantainence { get; set; }

    public string TEXT_SystemText_Code_Description { get; set; }

    public string Responsibility { get; set; }

    public Guid? HOME_Person_ID { get; set; }

    public Guid? HOME_Municipality_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastChangeDate { get; set; }

    public Guid? Managed_HOME_Person_ID { get; set; }

    public string Address { get; set; }

    public bool Highlight { get; set; }

    public int MinDaysAheadOfBooking { get; set; }

    public int MaxDaysBookable { get; set; }

    public string Color { get; set; }

    public bool AllowTimeslots { get; set; }

    public bool AllowOfficeHours { get; set; }

    public double? Lat { get; set; }

    public double? Lang { get; set; }

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<AUTH_Authority_Dates_Closed> AUTH_Authority_Dates_Closed { get; set; } = new List<AUTH_Authority_Dates_Closed>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<AUTH_Authority_Dates_Timeslot> AUTH_Authority_Dates_Timeslot { get; set; } = new List<AUTH_Authority_Dates_Timeslot>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<AUTH_Authority_Extended> AUTH_Authority_Extended { get; set; } = new List<AUTH_Authority_Extended>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<AUTH_Authority_Office_Hours> AUTH_Authority_Office_Hours { get; set; } = new List<AUTH_Authority_Office_Hours>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<AUTH_Authority_Reason> AUTH_Authority_Reason { get; set; } = new List<AUTH_Authority_Reason>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<AUTH_Authority_Theme> AUTH_Authority_Theme { get; set; } = new List<AUTH_Authority_Theme>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<AUTH_Municipal_Users_Authority> AUTH_Municipal_Users_Authority { get; set; } = new List<AUTH_Municipal_Users_Authority>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<FORM_Definition> FORM_Definition { get; set; } = new List<FORM_Definition>();

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<HOME_Document_Authority> HOME_Document_Authority { get; set; } = new List<HOME_Document_Authority>();

    [ForeignKey("HOME_Person_ID")]
    [InverseProperty("AUTH_Authority")]
    public virtual HOME_Person HOME_Person { get; set; }

    [InverseProperty("AUTH_Authority")]
    public virtual ICollection<HOME_Person_Authority> HOME_Person_Authority { get; set; } = new List<HOME_Person_Authority>();
}