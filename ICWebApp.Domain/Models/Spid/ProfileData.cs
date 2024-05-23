using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Spid
{
    public class ProfileData
    {
        public string? sessionId { get; set; }
        public string? spidCode { get; set; }
        public string? name { get; set; }
        public string? familyName { get; set; }
        public string? placeOfBirth { get; set; }
        public string? countyOfBirth { get; set; }
        public string? dateOfBirth { get; set; }
        public string? gender { get; set; }
        public string? companyName { get; set; }
        public string? registeredOffice { get; set; }
        public string? fiscalNumber { get; set; }
        public string? ivaCode { get; set; }
        public string? idCard { get; set; }
        public string? mobilePhone { get; set; }
        public string? email { get; set; }
        public string? domicileStreetAddress { get; set; }
        public string? domicilePostalCode { get; set; }
        public string? domicileMunicipality { get; set; }
        public string? domicileProvince { get; set; }
        public string? address { get; set; }
        public string? domicileNation { get; set; }
        public string? expirationDate { get; set; }
        public string? digitalAddress { get; set; }
    }
}
