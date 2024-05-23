﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Keyless]
    public partial class V_CANTEEN_Movements_Day1
    {
        public bool? DayMo { get; set; }
        public Guid? CANTEEN_School_ID { get; set; }
        public Guid? CANTEEN_Subscriber_Status_ID { get; set; }
        public bool? IsVegan { get; set; }
        [StringLength(50)]
        public string? LastName { get; set; }
        [Column(TypeName = "timestamp(0) without time zone")]
        public DateTime? Enddate { get; set; }
        public bool? IsGlutenIntolerance { get; set; }
        public Guid? ID { get; set; }
        public bool? DaySat { get; set; }
        public string? AdditionalIntolerance { get; set; }
        public bool? IsLactoseIntolerance { get; set; }
        public bool? DayThu { get; set; }
        [StringLength(50)]
        public string? TaxNumber { get; set; }
        public bool? DaySun { get; set; }
        public Guid? SubscriptionFamilyID { get; set; }
        public Guid? AUTH_Users_ID { get; set; }
        public bool? IsVegetarian { get; set; }
        public bool? DayTue { get; set; }
        [Column(TypeName = "timestamp(3) without time zone")]
        public DateTime? CreationDate { get; set; }
        public bool? DayWed { get; set; }
        public bool? DayFri { get; set; }
        [StringLength(50)]
        public string? FirstName { get; set; }
        public Guid? CANTEEN_Period_ID { get; set; }
        public int? DistanceFromSchool { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? RemovedDate { get; set; }
        public long? ReferenceID { get; set; }
        [Column(TypeName = "timestamp(0) without time zone")]
        public DateTime? Begindate { get; set; }
        public Guid? CANTEEN_Canteen_ID { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? SignedDate { get; set; }
        public bool? IsBothParentEmployed { get; set; }
        public Guid? AUTH_Municipality_ID { get; set; }
        public string? UserFirstName { get; set; }
        public Guid? CanteenMenuID { get; set; }
        public bool? IsWithoutPorkMeat { get; set; }
        public string? UserDomicilePostalCode { get; set; }
        public string? UserDomicileProvince { get; set; }
        public string? UserAdress { get; set; }
        public string? UserDomicileNation { get; set; }
        [StringLength(4)]
        public string? TelCode { get; set; }
        public string? UserEmail { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? UserDateOfBirth { get; set; }
        public string? UserLastName { get; set; }
        public string? UserTaxNumber { get; set; }
        public string? UserDomicileMunicipality { get; set; }
        public string? UserCountryOfBirth { get; set; }
        public string? UserMobilePhone { get; set; }
        public string? UserDomicileStreetAdress { get; set; }
        public string? UserGender { get; set; }
        public string? UserPlaceOfBirth { get; set; }
        [StringLength(20)]
        public string? SchoolyearDescription { get; set; }
        public Guid? SchoolyearID { get; set; }
        [StringLength(200)]
        public string? MenuName { get; set; }
        [StringLength(500)]
        public string? SchoolName { get; set; }
        [StringLength(50)]
        public string? SchoolClass { get; set; }
        public string? ReportName { get; set; }
        public string? authority_phone { get; set; }
        public string? authority_mail { get; set; }
        public string? mun_address { get; set; }
        public string? mun_location { get; set; }
        public string? mun_phone { get; set; }
        public string? mun_mail { get; set; }
        public byte[]? logo { get; set; }
        public Guid? canteen_subscriber_movementsid { get; set; }
        public Guid? CANTEEN_Subscriber_ID { get; set; }
        [Column(TypeName = "timestamp(0) without time zone")]
        public DateTime? Date { get; set; }
        [Column(TypeName = "money")]
        public decimal? Fee { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? CancelDate { get; set; }
        public Guid? CANTEEN_Subscriber_Movement_Type_ID { get; set; }
        public Guid? AUTH_User_ID { get; set; }
        public Guid? PaymentTransactionID { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? PaymentTransactionCompleted { get; set; }
        [StringLength(100)]
        public string? Info { get; set; }
        public bool? IsManual { get; set; }
    }
}
