﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_CANTEEN_Fiscal_Year_Sum
{
    public Guid ID { get; set; }

    public int? year { get; set; }

    public Guid? AUTH_Users_ID { get; set; }

    public string UserFirstName { get; set; }

    public string UserLastName { get; set; }

    public string UserTaxNumber { get; set; }

    public string UserCountryOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UserDateOfBirth { get; set; }

    public string UserDomicileMunicipality { get; set; }

    public string UserDomicilePostalCode { get; set; }

    public string UserDomicileStreetAdress { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string TaxNumber { get; set; }

    public string UserEmail { get; set; }

    [Column(TypeName = "money")]
    public decimal? feesum { get; set; }

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

    public string Child_DomicileMunicipality { get; set; }

    public string Child_DomicileNation { get; set; }

    public string Child_DomicileProvince { get; set; }

    public string Child_DomicilePostalCode { get; set; }

    public string Child_DomicileStreetAddress { get; set; }

    public Guid? Child_Domicile_Municipal_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Child_DateOfBirth { get; set; }

    public string Child_PlaceOfBirth { get; set; }
}