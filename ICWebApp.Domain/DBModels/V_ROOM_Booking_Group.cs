﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_ROOM_Booking_Group
{
    public Guid ID { get; set; }

    public Guid? AUTH_User_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    public Guid? AUTH_MunicipalityID { get; set; }

    public Guid? ROOM_BookingStatus_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PayedAt { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FiscalNumber { get; set; }

    public string Email { get; set; }

    public string CountyOfBirth { get; set; }

    public string PlaceOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateOfBirth { get; set; }

    public string Address { get; set; }

    public string DomicileMunicipality { get; set; }

    public string DomicileNation { get; set; }

    public string DomicilePostalCode { get; set; }

    public string DomicileProvince { get; set; }

    public string DomicileStreetAddress { get; set; }

    public string Gender { get; set; }

    public string MobilePhone { get; set; }

    public string VatNumber { get; set; }

    public string PECEmail { get; set; }

    public string Phone { get; set; }

    public string CodiceDestinatario { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public Guid? FILE_FileInfo_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SubmitAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PrivacyDate { get; set; }

    public Guid? ROOT_AUTH_User_ID { get; set; }

    public string ROOT_FirstName { get; set; }

    public string ROOT_LastName { get; set; }

    public string ROOT_FiscalCode { get; set; }

    public string ROOT_CountryOfBirth { get; set; }

    public string ROOT_PlaceOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ROOT_DateOfBirth { get; set; }

    public Guid? GV_AUTH_User_ID { get; set; }

    public string GV_FirstName { get; set; }

    public string GV_LastName { get; set; }

    public string GV_FiscalCode { get; set; }

    public string GV_CountryOfBirth { get; set; }

    public string GV_PlaceOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? GV_DateOfBirth { get; set; }

    public string Cancellation_IBAN { get; set; }

    public string Cancellation_Owner { get; set; }

    public string Cancellation_Banc { get; set; }

    public string ROOT_CountyOfBirth { get; set; }

    public string ROOT_Address { get; set; }

    public string ROOT_DomicileMunicipality { get; set; }

    public string ROOT_DomicileNation { get; set; }

    public string ROOT_DomicilePostalCode { get; set; }

    public string ROOT_DomicileProvince { get; set; }

    public string ROOT_DomicileStreetAddress { get; set; }

    public string ROOT_Gender { get; set; }

    public string ROOT_MobilePhone { get; set; }

    public string ROOT_Email { get; set; }

    public string ROOT_Phone { get; set; }

    public string GV_CountyOfBirth { get; set; }

    public string GV_Address { get; set; }

    public string GV_DomicileMunicipality { get; set; }

    public string GV_DomicileNation { get; set; }

    public string GV_DomicilePostalCode { get; set; }

    public string GV_DomicileProvince { get; set; }

    public string GV_DomicileStreetAddress { get; set; }

    public string GV_Gender { get; set; }

    public string GV_MobilePhone { get; set; }

    public string GV_Email { get; set; }

    public string GV_Phone { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SignedAt { get; set; }

    public long? ProgressivYear { get; set; }

    public long? ProgressivNumber { get; set; }

    public Guid? ROOM_Booking_Type_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ProtocolDate { get; set; }

    public string Fullname { get; set; }

    public string Status { get; set; }

    public Guid? LANG_LanguagesID { get; set; }

    public string SearchText { get; set; }

    public int? RoomCount { get; set; }

    public string IconCSS { get; set; }

    public string Rooms { get; set; }

    [StringLength(4000)]
    public string Days { get; set; }

    public string mun_address { get; set; }

    public string mun_location { get; set; }

    public string mun_phone { get; set; }

    public string mun_mail { get; set; }

    public string TEXT_GV { get; set; }

    public string TEXT_ROOT_BIRTH { get; set; }

    public string TEXT_SUBMITTED_FROM { get; set; }

    public int HasRootUser { get; set; }

    public string TEXT_BIRTH { get; set; }

    public string ComposedName { get; set; }

    public string PhoneComposed { get; set; }

    public string ComposedMail { get; set; }

    public string GV_Name { get; set; }

    public int HasGVUser { get; set; }

    public string TEXT_GV_BIRTH { get; set; }

    public string ROOT_Name { get; set; }

    public string AMT_DE { get; set; }

    public string AMT_IT { get; set; }

    public string Authority_Mail { get; set; }

    public string Authority_Phone { get; set; }

    public string Authority_MailIT { get; set; }

    public string Text_IT { get; set; }

    public string Text { get; set; }

    public string FiscalData { get; set; }

    [Unicode(false)]
    public string ProgressivNumberCombined { get; set; }

    public int HasPayments { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }
}