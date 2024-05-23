﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_CANTEEN_Subscriber
{
    public Guid ID { get; set; }

    public Guid? AUTH_Users_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    [StringLength(50)]
    public string FirstName { get; set; }

    [StringLength(50)]
    public string LastName { get; set; }

    [StringLength(101)]
    public string FullName { get; set; }

    [StringLength(50)]
    public string TaxNumber { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Begindate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Enddate { get; set; }

    public bool DayMo { get; set; }

    public bool DayTue { get; set; }

    public bool DayWed { get; set; }

    public bool DayThu { get; set; }

    public bool DayFri { get; set; }

    public bool DaySat { get; set; }

    public bool DaySun { get; set; }

    public bool IsVegetarian { get; set; }

    public bool IsVegan { get; set; }

    public bool IsLactoseIntolerance { get; set; }

    public bool IsGlutenIntolerance { get; set; }

    public bool IsBothParentEmployed { get; set; }

    public int DistanceFromSchool { get; set; }

    public string AdditionalIntolerance { get; set; }

    public Guid? CANTEEN_Subscriber_Status_ID { get; set; }

    public long ReferenceID { get; set; }

    public Guid? CANTEEN_Canteen_ID { get; set; }

    public Guid? CANTEEN_School_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RemovedDate { get; set; }

    public Guid? CANTEEN_Period_ID { get; set; }

    public Guid? SubscriptionFamilyID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SignedDate { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public bool IsWithoutPorkMeat { get; set; }

    public string UserFirstName { get; set; }

    public string UserLastName { get; set; }

    public string UserTaxNumber { get; set; }

    public string UserEmail { get; set; }

    public string UserCountryOfBirth { get; set; }

    public string UserPlaceOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UserDateOfBirth { get; set; }

    public string UserAdress { get; set; }

    public string UserDomicileMunicipality { get; set; }

    public string UserDomicileNation { get; set; }

    public string UserDomicilePostalCode { get; set; }

    public string UserDomicileProvince { get; set; }

    public string UserDomicileStreetAdress { get; set; }

    public string UserMobilePhone { get; set; }

    public string UserGender { get; set; }

    public Guid? SchoolyearID { get; set; }

    [StringLength(20)]
    public string SchoolyearDescription { get; set; }

    public Guid? CanteenMenuID { get; set; }

    [StringLength(4)]
    public string TelCode { get; set; }

    public Guid? FILE_FileInfo_ID { get; set; }

    [StringLength(50)]
    public string SchoolClass { get; set; }

    [StringLength(500)]
    public string SchoolName { get; set; }

    [StringLength(200)]
    public string MenuName { get; set; }

    public Guid? FILE_FileInfo_SpecialMenu_ID { get; set; }

    public Guid? SchoolClassID { get; set; }

    [StringLength(200)]
    public string DayString { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Archived { get; set; }

    public bool? IsManualInput { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SubmitAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PrivacyDate { get; set; }

    public int? Version { get; set; }

    public Guid? ReplacementSubscriptionID { get; set; }

    public Guid? SuccessorSubscriptionID { get; set; }

    public Guid? PreviousSubscriptionID { get; set; }

    [StringLength(150)]
    public string StatusIcon { get; set; }

    public string StatusText { get; set; }

    public Guid? LANG_LanguagesID { get; set; }

    public string SchoolNameText { get; set; }

    public string CanteenNameText { get; set; }

    public string SchoolClassText { get; set; }

    public string MealMenuText { get; set; }

    public bool? InChange { get; set; }

    [Required]
    public string SearchBox { get; set; }

    public string Child_DomicileMunicipality { get; set; }

    public string Child_DomicileNation { get; set; }

    public string Child_DomicileProvince { get; set; }

    public string Child_DomicilePostalCode { get; set; }

    public string Child_DomicileStreetAddress { get; set; }

    public Guid? Child_Domicile_Municipal_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Child_DateOfBirth { get; set; }

    public string Child_PlaceOfBirth { get; set; }

    [Unicode(false)]
    public string ProgressivNumber { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReminderDate { get; set; }
}