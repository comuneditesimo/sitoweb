using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class CANTEEN_Subscriber
    {
        [NotMapped]
        public EditForm HTMLReference { get; set; }
        [NotMapped]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        [NotMapped][Range(0, 100, ErrorMessage = "OUT_OF_RANGE")]
        public int DistanceFromSchoolValidated
        {
            get
            {
                return DistanceFromSchool;
            }
            set
            {
                DistanceFromSchool = value;
            }
        }
        [NotMapped]
        [Required]
        public string FirstNameReq
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
        public string LastNameReq
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
        [RegularExpression(@"^([A-Z]{6}[0-9LMNPQRSTUV]{2}[ABCDEHLMPRST]{1}[0-9LMNPQRSTUV]{2}[A-Z]{1}[0-9LMNPQRSTUV]{3}[A-Z]{1})$|([0-9]{11})$", ErrorMessage = "NOT_RIGHT")]
        public string TaxNumberReq
        {
            get
            {
                return TaxNumber;
            }
            set
            {
                TaxNumber = value;
            }
        }
        [NotMapped]
        [Required]
        public Guid? CANTEEN_Canteen_IDReq
        {
            get
            {
                return CANTEEN_Canteen_ID;
            }
            set
            {
                CANTEEN_Canteen_ID = value;
            }
        }
        [NotMapped]
        [Required]
        public Guid? CANTEEN_School_IDReq
        {
            get
            {
                return CANTEEN_School_ID;
            }
            set
            {
                CANTEEN_School_ID = value;
            }
        }
        [NotMapped]
        public string? TaxNumberError { get; set; }
        [NotMapped]
        public string DaySelectionError { get; set; }
        [NotMapped]
        public string MedicalFileError { get; set; }
        [NotMapped]
        public string SchoolClassError { get; set; }
        [NotMapped]
        public List<FILE_FileInfo> MedicalFiles { get; set; }
        [NotMapped]
        private bool _enableDayMo = false;
        [NotMapped]
        private bool _enableDayThu = false;
        [NotMapped]
        private bool _enableDayTue = false;
        [NotMapped]
        private bool _enableDayWed = false;
        [NotMapped]
        private bool _enableDayFri = false;
        [NotMapped]
        private bool _enableDaySat = false;
        [NotMapped]
        private bool _enableDaySun = false;
        [NotMapped]
        public bool EnableDayMo {
            get
            {
                return _enableDayMo;

            }
            set
            {
                if (_enableDayMo != value)
                {
                    _enableDayMo = value;

                }

            }
        }
        [NotMapped]
        public bool EnableDayThu
        {
            get
            {
                return _enableDayThu;

            }
            set
            {
                if (_enableDayThu != value)
                {
                    _enableDayThu = value;

                }

            }
        }
        [NotMapped]
        public bool EnableDayTue
        {
            get
            {
                return _enableDayTue;

            }
            set
            {
                if (_enableDayTue != value)
                {
                    _enableDayTue = value;

                }

            }
        }
        [NotMapped]
        public bool EnableDayWed
        {
            get
            {
                return _enableDayWed;

            }
            set
            {
                if (_enableDayWed != value)
                {
                    _enableDayWed = value;

                }

            }
        }
        [NotMapped]
        public bool EnableDayFri
        {
            get
            {
                return _enableDayFri;

            }
            set
            {
                if (_enableDayFri != value)
                {
                    _enableDayFri = value;

                }

            }
        }
        [NotMapped]
        public bool EnableDaySat
        {
            get
            {
                return _enableDaySat;

            }
            set
            {
                if (_enableDaySat != value)
                {
                    _enableDaySat = value;

                }

            }
        }
        [NotMapped]
        public bool EnableDaySun
        {
            get
            {
                return _enableDaySun;

            }
            set
            {
                if (_enableDaySun != value)
                {
                    _enableDaySun = value;

                }

            }
        }
        [NotMapped]
        public String SearchBox
        {
            get
            {
                string result = "";
                result = result + this.SchoolName ?? "" + " ; ";
                result = result + this.CanteenMenu ?? "" + " ; ";
                result = result + this.FirstName ?? "" + " ; ";
                result = result + this.LastName ?? "" + " ; ";
                result = result + this.FullName ?? "" + " ; ";
                result = result + this.MenuName ?? "" + " ; ";
                result = result + this.TaxNumber ?? "" + " ; ";
                result = result + this.UserEmail ?? "" + " ; ";
                result = result + this.TelCode ?? "" + " ; ";
                result = result + this.SchoolClass ?? "" + " ; ";
                result = result + this.UserAdress ?? "" + " ; ";
                result = result + this.UserFirstName ?? "" + " ; ";
                result = result + this.UserLastName ?? "" + " ; ";

                return result;
            }
        }
        [NotMapped]
        public List<CANTEEN_SchoolClass> SchoolClasses { get; set; } = new List<CANTEEN_SchoolClass>();
        [NotMapped]
        public bool AddressNotFound { get; set; } = false;
        [NotMapped]
        public Guid SelectedMunicipality 
        {
            get
            {
                if(Child_Domicile_Municipal_ID == null)
                    return Guid.Empty;

                return Child_Domicile_Municipal_ID.Value;
            }
            set
            {
                Child_Domicile_Municipal_ID = value;
            } 
        }
        [NotMapped]
        [Required]
        public string? ReqDomicileMunicipality
        {
            get
            {
                return Child_DomicileMunicipality;
            }
            set
            {
                Child_DomicileMunicipality = value;
            }
        }
        [NotMapped]
        [Required]
        public string? ReqDomicileNation
        {
            get
            {
                return Child_DomicileNation;
            }
            set
            {
                Child_DomicileNation = value;
            }
        }
        [NotMapped]
        [Required]
        public string? ReqDomicilePostalCode
        {
            get
            {
                return Child_DomicilePostalCode;
            }
            set
            {
                Child_DomicilePostalCode = value;
            }
        }
        [NotMapped]
        [Required]
        public string? ReqDomicileProvince
        {
            get
            {
                return Child_DomicileProvince;
            }
            set
            {
                Child_DomicileProvince = value;
            }
        }
        [NotMapped]
        [Required]
        public string? ReqDomicileStreetAddress
        {
            get
            {
                return Child_DomicileStreetAddress;
            }
            set
            {
                Child_DomicileStreetAddress = value;
            }
        }
        [NotMapped]
        [Required]
        public DateTime? ReqDateOfBirth
        {
            get
            {
                return Child_DateOfBirth;
            }
            set
            {
                Child_DateOfBirth = value;
            }
        }
        [NotMapped]
        [Required]
        public string? ReqPlaceOfBirth
        {
            get
            {
                return Child_PlaceOfBirth;
            }
            set
            {
                Child_PlaceOfBirth = value;
            }
        }
        [NotMapped]
        public string MealTypeMessage { get; set; }
        [NotMapped]
        public int OrderCount { get; set; }
    }
}
