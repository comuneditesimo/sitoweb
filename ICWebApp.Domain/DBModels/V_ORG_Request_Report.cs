﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_ORG_Request_Report
{
    public Guid ID { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string FiscalNumber { get; set; }

    public string VatNumber { get; set; }

    public string Email { get; set; }

    public string PECEmail { get; set; }

    public string Address { get; set; }

    public string DomicileMunicipality { get; set; }

    public string DomicileNation { get; set; }

    public string DomicilePostalCode { get; set; }

    public string DomicileProvince { get; set; }

    public string DomicileStreetAddress { get; set; }

    public string Phone { get; set; }

    public string MobilePhone { get; set; }

    public string CodiceDestinatario { get; set; }

    public Guid? AUTH_Company_LegalForm_ID { get; set; }

    public string HandelskammerEintragung { get; set; }

    public Guid? ORG_Request_Status_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    public Guid? AUTH_Users_ID { get; set; }

    public Guid? AUTH_Company_Type_ID { get; set; }

    public bool? GVEqualsUser { get; set; }

    public Guid? GV_AUTH_Users_ID { get; set; }

    public string GV_Firstname { get; set; }

    public string GV_Lastname { get; set; }

    public string GV_FiscalNumber { get; set; }

    public string GV_Email { get; set; }

    public string GV_DomicileMunicipality { get; set; }

    public string GV_DomicileNation { get; set; }

    public string GV_DomicilePostalCode { get; set; }

    public string GV_DomicileProvince { get; set; }

    public string GV_DomicileStreetAddress { get; set; }

    public string GV_Address { get; set; }

    public string GV_Phone { get; set; }

    public string GV_CountyOfBirth { get; set; }

    public string GV_PlaceOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? GV_DateOfBirth { get; set; }

    public string GV_Gender { get; set; }

    public bool? PrivacyConfirmed { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SubmitAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SignetAt { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public Guid? FILE_Fileinfo_ID { get; set; }

    public string User_Firstname { get; set; }

    public string User_Lastname { get; set; }

    public string User_FiscalNumber { get; set; }

    public string User_Email { get; set; }

    public string User_DomicileMunicipality { get; set; }

    public string User_DomicileNation { get; set; }

    public string User_DomicilePostalCode { get; set; }

    public string User_DomicileProvince { get; set; }

    public string User_DomicileStreetAddress { get; set; }

    public string User_Phone { get; set; }

    public string User_CountyOfBirth { get; set; }

    public string User_PlaceOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? User_DateOfBirth { get; set; }

    public string User_Gender { get; set; }

    public string User_Address { get; set; }

    [StringLength(250)]
    public string InternalName { get; set; }

    public string Text { get; set; }

    public byte[] Logo { get; set; }

    public double? Lat { get; set; }

    public double? Lng { get; set; }

    public string Text_IT { get; set; }

    public string MUN_Address { get; set; }

    public string MUN_Location { get; set; }

    public string MUN_Phone { get; set; }

    public string MUN_Mail { get; set; }

    public string MUN_ReportName { get; set; }

    public string PrivacyDescription { get; set; }

    public string PrivacyTitle { get; set; }

    public Guid? LANG_Languages_ID { get; set; }

    public string ReportTitle { get; set; }

    public string User_ComposedName { get; set; }

    public string TEXT_BIRTH { get; set; }

    public string PhoneComposed { get; set; }

    public string ComposedMail { get; set; }

    public string TEXT_COMITTED { get; set; }

    public string CompanyType { get; set; }

    public string CompanyLegalForm { get; set; }

    public int HasGV { get; set; }

    public string TEXT_FicsalCode { get; set; }

    public string TEXT_Denomination { get; set; }

    public string TEXT_Name { get; set; }

    public string TEXT_VatNumber { get; set; }

    public string TEXT_Address { get; set; }

    public string TEXT_Birthdata { get; set; }

    public string TEXT_Requests { get; set; }

    public string TEXT_GV { get; set; }

    public string User_Birthdata { get; set; }

    public string GV_Birthdata { get; set; }

    public string TEXT_PROTOCOLLNR { get; set; }

    public string GV_ComposedName { get; set; }

    public string ComposedName { get; set; }

    public int HasVat { get; set; }

    public Guid? AUTH_BolloFree_Reason_ID { get; set; }

    public bool BolloFree { get; set; }

    public string AUTH_BolloFree_Reason { get; set; }

    public string IBAN { get; set; }

    public string KontoInhaber { get; set; }

    public string Bankname { get; set; }
}