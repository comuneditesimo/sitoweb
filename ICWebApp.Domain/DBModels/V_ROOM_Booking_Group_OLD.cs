using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Keyless]
    public partial class V_ROOM_Booking_Group_OLD
    {
        public Guid? ID { get; set; }
        public Guid? AUTH_User_ID { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? CreationDate { get; set; }
        [Column(TypeName = "money")]
        public decimal? TotalSum { get; set; }
        public Guid? FILE_FileInfo_ID { get; set; }
        public Guid? SessionKeyID { get; set; }
        public Guid? AUTH_MunicipalityID { get; set; }
        public int? TotalDays { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? FirstDate { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? LastDate { get; set; }
        public Guid? BookingStatusID { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? GdprConsent { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? PayedAt { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FiscalNumber { get; set; }
        public string? Email { get; set; }
        public string? CountyOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? DomicileMunicipality { get; set; }
        public string? DomicileNation { get; set; }
        public string? DomicilePostalCode { get; set; }
        public string? DomicileProvince { get; set; }
        public string? DomicileStreetAddress { get; set; }
        public string? Gender { get; set; }
        public string? MobilePhone { get; set; }
        public string? VatNumber { get; set; }
        public string? PECEmail { get; set; }
        public string? Phone { get; set; }
        public string? CodiceDestinatario { get; set; }
        public string? Fullname { get; set; }
        public string? Status { get; set; }
        public Guid? LANG_LanguagesID { get; set; }
        public string? SearchText { get; set; }
        public long? RoomCount { get; set; }
        public string? IconCSS { get; set; }
        public string? Rooms { get; set; }
        public string? Days { get; set; }
        [StringLength(300)]
        public string? Title { get; set; }
        public string? InfoMessage { get; set; }
        public string? mun_address { get; set; }
        public string? mun_location { get; set; }
        public string? mun_phone { get; set; }
        public string? mun_mail { get; set; }
    }
}
