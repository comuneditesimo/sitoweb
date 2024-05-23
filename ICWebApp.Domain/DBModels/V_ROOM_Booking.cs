﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_ROOM_Booking
{
    public Guid ID { get; set; }

    public Guid? ROOM_Room_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public Guid? ROOM_BookingGroup_ID { get; set; }

    public Guid? ROOM_BookingGroupBackend_ID { get; set; }

    public Guid? AUTH_User_ID { get; set; }

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

    public string RoomName { get; set; }

    public string BuildingName { get; set; }

    public Guid? LANG_Languages_ID { get; set; }

    public string Booking_Status { get; set; }

    [StringLength(50)]
    public string RoomColor { get; set; }

    public string StatusCSS { get; set; }

    public Guid? ROOM_Booking_Type_ID { get; set; }

    public bool? IsOrganization { get; set; }

    public bool? IsWholeDay { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDateWithBufferTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDateWithBufferTime { get; set; }
}