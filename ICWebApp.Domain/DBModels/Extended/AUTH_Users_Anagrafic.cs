using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class AUTH_Users_Anagrafic
    {
        [NotMapped]
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "NO_MAIL")]
        public string DA_Email { get; set; }

        [NotMapped]
        public string DA_MobilePhone { get; set; }

        [NotMapped][Required]public string Password { get; set; }
        [NotMapped][Required][Compare("Password", ErrorMessage = "NO_MATCH;REGISTRATION_PASSWORD")]
        public string ConfirmPassword { get; set; }
        [NotMapped]public string Fullname { get; set; }
        [NotMapped][Required]
        public string ReqFirstName
        {
            get
            {
                return FirstName;
            }
            set
            {
                FirstName = value;
            }
        }
        [NotMapped]
        [Required]
        public string ReqLastName
        {
            get
            {
                return LastName;
            }
            set
            {
                LastName = value;
            }
        }
        [NotMapped]
        [Required]
        [MinLength(6)]
        public string ReqPhoneNumber
        {
            get
            {
                return MobilePhone;
            }
            set
            {
                MobilePhone = value;
            }
        }
        [NotMapped][Required][RegularExpression(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$|^[0-9]{11}$", ErrorMessage = "VALIDATION_REGEX")]
        public string? ReqFiscalNumber
        {
            get
            {
                return FiscalNumber;
            }
            set
            {
                FiscalNumber = value;
            }
        }
        [NotMapped][Required] public string? ReqCountyOfBirth
        {
            get
            {
                return CountyOfBirth;
            }
            set
            {
                CountyOfBirth = value;
            }
        }
        [NotMapped][Required] public string? ReqPlaceOfBirth
        {
            get
            {
                return PlaceOfBirth;
            }
            set
            {
                PlaceOfBirth = value;
            }
        }
        [NotMapped][Required] public DateTime? ReqDateOfBirth
        {
            get
            {
                return DateOfBirth;
            }
            set
            {
                DateOfBirth = value;
            }
        }
        [NotMapped] public string? ReqAddress
        {
            get
            {
                return Address;
            }
            set
            {
                Address = value;
            }
        }
        [NotMapped][Required] public string? ReqDomicileMunicipality
        {
            get
            {
                return DomicileMunicipality;
            }
            set
            {
                DomicileMunicipality = value;
            }
        }
        [NotMapped][Required] public string? ReqDomicileNation
        {
            get
            {
                return DomicileNation;
            }
            set
            {
                DomicileNation = value;
            } 
        }
        [NotMapped][Required] public string? ReqDomicilePostalCode
        {
            get
            {
                return DomicilePostalCode;
            }
            set
            {
                DomicilePostalCode = value;
            }
        }
        [NotMapped][Required] public string? ReqDomicileProvince
        {
            get
            {
                return DomicileProvince;
            }
            set
            {
                DomicileProvince = value;
            }
        }
        [NotMapped][Required] public string? ReqDomicileStreetAddress
        {
            get
            {
                return DomicileStreetAddress;
            }
            set
            {
                DomicileStreetAddress = value;
            }
        }
        [NotMapped][Required] public string? ReqGender
        {
            get
            {
                return Gender;
            }
            set
            {
                Gender = value;
            }
        }
        [NotMapped] public string? IBANReq
        {
            get
            {
                return IBAN;
            }
            set
            {
                IBAN = value;
            }
        }
        [NotMapped] public string BanknameReq
        {
            get
            {
                return Bankname;
            }
            set
            {
                Bankname = value;
            }
        }
        [NotMapped] public string KontoInhaberReq
        {
            get
            {
                return KontoInhaber;
            }
            set
            {
                KontoInhaber = value;
            }
        }
        [NotMapped] public string GV_FirstName
        {
            get
            {
                return FirstName;
            }
            set
            {
                FirstName = value;
            }
        }
        [NotMapped] public string GV_LastName
        {
            get
            {
                return LastName;
            }
            set
            {
                LastName = value;
            }
        }
        [NotMapped] public string GV_FiscalNumber
        {
            get
            {
                return FiscalNumber;
            }
            set
            {
                FiscalNumber = value;
            }
        }
        [NotMapped] public string GV_Email
        {
            get
            {
                return Email;
            }
            set
            {
                Email = value;
            }
        }
        [NotMapped] public string GV_CountyOfBirth
        {
            get
            {
                return CountyOfBirth;
            }
            set
            {
                CountyOfBirth = value;
            }
        }
        [NotMapped] public string GV_PlaceOfBirth
        {
            get
            {
                return PlaceOfBirth;
            }
            set
            {
                PlaceOfBirth = value;
            }
        }
        [NotMapped] public DateTime? GV_DateOfBirth
        {
            get
            {
                return DateOfBirth;
            }
            set
            {
                DateOfBirth = value;
            }
        }
        [NotMapped] public string GV_Address
        {
            get
            {
                return Address;
            }
            set
            {
                Address = value;
            }
        }
        [NotMapped] public string GV_DomicileMunicipality
        {
            get
            {
                return DomicileMunicipality;
            }
            set
            {
                DomicileMunicipality = value;
            }
        }
        [NotMapped] public string GV_DomicileNation
        {
            get
            {
                return DomicileNation;
            }
            set
            {
                DomicileNation = value;
            }
        }
        [NotMapped] public string GV_DomicilePostalCode
        {
            get
            {
                return DomicilePostalCode;
            }
            set
            {
                DomicilePostalCode = value;
            }
        }
        [NotMapped] public string GV_DomicileProvince
        {
            get
            {
                return DomicileProvince;
            }
            set
            {
                DomicileProvince = value;
            }
        }
        [NotMapped] public string GV_DomicileStreetAddress
        {
            get
            {
                return DomicileStreetAddress;
            }
            set
            {
                DomicileStreetAddress = value;
            }
        }
        [NotMapped] public string GV_Gender
        {
            get
            {
                return Gender;
            }
            set
            {
                Gender = value;
            }
        }
        [NotMapped] public string GV_MobilePhone
        {
            get
            {
                return MobilePhone;
            }
            set
            {
                MobilePhone = value;
            }
        }
        [NotMapped] public string GV_RegisteredOffice
        {
            get
            {
                return RegisteredOffice;
            }
            set
            {
                RegisteredOffice = value;
            }
        }
        [NotMapped] public string GV_STRIPE_Customer_ID
        {
            get
            {
                return STRIPE_Customer_ID;
            }
            set
            {
                STRIPE_Customer_ID = value;
            }
        }
        [NotMapped] public string GV_VatNumber
        {
            get
            {
                return VatNumber;
            }
            set
            {
                VatNumber = value;
            }
        }
        [NotMapped] public string GV_PECEmail
        {
            get
            {
                return PECEmail;
            }
            set
            {
                PECEmail = value;
            }
        }
        [NotMapped] public string GV_CodiceDestinatario
        {
            get
            {
                return CodiceDestinatario;
            }
            set
            {
                CodiceDestinatario = value;
            }
        }
        [NotMapped] public string GV_Phone
        {
            get
            {
                return Phone;
            }
            set
            {
                Phone = value;
            }
        }
    }
}
