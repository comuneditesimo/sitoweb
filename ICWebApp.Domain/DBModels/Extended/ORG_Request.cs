using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class ORG_Request
    {
        [NotMapped]
        [Required]
        public string? FirstnameReq
        {
            get
            {
                return Firstname;
            }
            set
            {
                Firstname = value;
            }
        }
        [NotMapped]
        [Required]
        public string? LastnameReq
        {
            get
            {
                return Lastname;
            }
            set
            {
                Lastname = value;
            }
        }
        [NotMapped]
        [Required]
        [RegularExpression(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$|^[0-9]{11}$", ErrorMessage = "VALIDATION_REGEX")]
        public string? FiscalNumberReq
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
        [NotMapped]
        [Required]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "VALIDATION_REGEX")]
        public string? VatNumberReq
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
        [NotMapped]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "VALIDATION_REGEX")]
        public string? VatNumberVal
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
        [NotMapped]
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? EmailReq
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
        [NotMapped]
        [DataType(DataType.EmailAddress)]
        public string? PECEmailReq
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
        [NotMapped]
        [Required]
        public string? DomicileMunicipalityReq
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
        [NotMapped]
        [Required]
        public string? DomicileNationReq
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
        [NotMapped]
        [Required]
        public string? DomicilePostalCodeReq
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
        [NotMapped]
        [Required]
        public string? DomicileProvinceReq
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
        [NotMapped]
        [Required]
        public string? DomicileStreetAddressReq
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
        [NotMapped]
        [Required]
        public string? PhoneReq
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
        [NotMapped]
        [Required]
        public string? MobilePhoneReq
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
        [NotMapped]
        public string? CodiceDestinatarioReq
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
        [NotMapped]
        [Required]
        public Guid? AUTH_Company_LegalForm_IDReq
        {
            get
            {
                return AUTH_Company_LegalForm_ID;
            }
            set
            {
                AUTH_Company_LegalForm_ID = value;
            }
        }
        [NotMapped]
        [Required]
        public string? HandelskammerEintragungReq
        {
            get
            {
                return HandelskammerEintragung;
            }
            set
            {
                HandelskammerEintragung = value;
            }
        }
        [NotMapped]
        [Required]
        public Guid? AUTH_Company_Type_IDReq
        {
            get
            {
                return AUTH_Company_Type_ID;
            }
            set
            {
                AUTH_Company_Type_ID = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_FirstnameReq
        {
            get
            {
                return GV_Firstname;
            }
            set
            {
                GV_Firstname = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_LastnameReq
        {
            get
            {
                return GV_Lastname;
            }
            set
            {
                GV_Lastname = value;
            }
        }
        [NotMapped]
        [Required]
        [RegularExpression(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$", ErrorMessage = "VALIDATION_REGEX")]
        public string? GV_FiscalNumberReq
        {
            get
            {
                return GV_FiscalNumber;
            }
            set
            {
                GV_FiscalNumber = value;
            }
        }
        [NotMapped]
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? GV_EmailReq
        {
            get
            {
                return GV_Email;
            }
            set
            {
                GV_Email = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_DomicileMunicipalityReq
        {
            get
            {
                return GV_DomicileMunicipality;
            }
            set
            {
                GV_DomicileMunicipality = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_DomicileNationReq
        {
            get
            {
                return GV_DomicileNation;
            }
            set
            {
                GV_DomicileNation = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_DomicilePostalCodeReq
        {
            get
            {
                return GV_DomicilePostalCode;
            }
            set
            {
                GV_DomicilePostalCode = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_DomicileProvinceReq
        {
            get
            {
                return GV_DomicileProvince;
            }
            set
            {
                GV_DomicileProvince = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_DomicileStreetAddressReq
        {
            get
            {
                return GV_DomicileStreetAddress;
            }
            set
            {
                GV_DomicileStreetAddress = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_PhoneReq
        {
            get
            {
                return GV_Phone;
            }
            set
            {
                GV_Phone = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_CountyOfBirthReq
        {
            get
            {
                return GV_CountyOfBirth;
            }
            set
            {
                GV_CountyOfBirth = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_PlaceOfBirthReq
        {
            get
            {
                return GV_PlaceOfBirth;
            }
            set
            {
                GV_PlaceOfBirth = value;
            }
        }
        [NotMapped]
        [Required]
        public DateTime? GV_DateOfBirthReq
        {
            get
            {
                return GV_DateOfBirth;
            }
            set
            {
                GV_DateOfBirth = value;
            }
        }
        [NotMapped]
        [Required]
        public string? GV_GenderReq
        {
            get
            {
                return GV_Gender;
            }
            set
            {
                GV_Gender = value;
            }
        }
        [NotMapped]
        public bool ArchivedBool
        {
            get
            {
                if(Archived != null)
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (value == true)
                {
                    Archived = DateTime.Now;
                }
                else
                {
                    Archived = null;
                }
            }
        }
        [NotMapped]
        [Required]
        public string EintragungNrReq
        {
            get
            {
                return EintragungNr;
            }
            set
            {
                EintragungNr = value;
            }
        }
        [NotMapped]
        [Required]
        public DateTime? EintragungDatumReq
        {
            get
            {
                return EintragungDatum;
            }
            set
            {
                EintragungDatum = value;
            }
        }
        [NotMapped]
        [Required]
        public string IBANReq
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
        [NotMapped]
        public bool IsNewOrgRequestInverted
        {
            get
            {
                return !IsNewOrgRequest;
            }
            set
            {
                IsNewOrgRequest = !value;
            }
        }
        [NotMapped]
        [Required]
        public string BanknameReq
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
        [NotMapped]
        [Required]
        public string KontoInhaberReq
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
        [NotMapped]
        [Required]
        public DateTime? BolloFreeRegistrationDateReq
        {
            get
            {
                return BolloFreeRegistrationDate;
            }
            set
            {
                BolloFreeRegistrationDate = value;
            }
        }
        [NotMapped]
        [Required]
        public string BolloFreeRegistrationNumberReq
        {
            get
            {
                return BolloFreeRegistrationNumber;
            }
            set
            {
                BolloFreeRegistrationNumber = value;
            }
        }
        [NotMapped]
        public bool HasUnreadChatMessages { get; set; } = false;
    }
}
