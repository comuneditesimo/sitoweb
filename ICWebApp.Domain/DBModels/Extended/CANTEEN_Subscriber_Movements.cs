using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public  partial class CANTEEN_Subscriber_Movements
    {
        [NotMapped]
        public string FullName
        {
            get
            {
                return this.CANTEEN_Subscriber.FirstName + " " + this.CANTEEN_Subscriber.LastName;
            }
        }
        [NotMapped]
        public string TaxNumber
        {
            get
            {
                return this.CANTEEN_Subscriber.TaxNumber;
            }
        }
        [NotMapped]
        public string UserEmail
        {
            get
            {
                return this.CANTEEN_Subscriber.UserEmail;
            }
        }
        [NotMapped]
        public bool IsGlutenIntolerance
        {
            get
            {
                return this.CANTEEN_Subscriber.IsGlutenIntolerance;
            }
        }
        [NotMapped]
        public bool IsWithoutPorkMeat
        {
            get
            {
                return this.CANTEEN_Subscriber.IsWithoutPorkMeat;
            }
        }
        [NotMapped]
        public bool IsLactoseIntolerance
        {
            get
            {
                return this.CANTEEN_Subscriber.IsLactoseIntolerance;
            }
        }
        [NotMapped]
        public bool IsVegetarian
        {
            get
            {
                return this.CANTEEN_Subscriber.IsVegetarian;
            }
        }
        [NotMapped]
        public string IsGlutenIntoleranceTxt
        {
            get
            {
                if (this.CANTEEN_Subscriber.IsGlutenIntolerance)
                {
                    return "JA/SI";
                }
                else
                {
                    return "";
                }
            }
        }
        [NotMapped]
        public string IsWithoutPorkMeatTxt
        {
            get
            {
                if (this.CANTEEN_Subscriber.IsWithoutPorkMeat)
                {
                    return "JA/SI";
                }
                else
                {
                    return "";
                }
            }
        }
        [NotMapped]
        public string IsLactoseIntoleranceTxt
        {
            get
            {
                if (this.CANTEEN_Subscriber.IsLactoseIntolerance)
                {
                    return "JA/SI";
                }
                else
                {
                    return "";
                }
            }
        }
        [NotMapped]
        public string IsVegetarianTxt
        {
            get
            {
                if (this.CANTEEN_Subscriber.IsVegetarian)
                {
                    return "JA/SI";
                }
                else
                {
                    return "";
                }
            }
        }
        [NotMapped]
        public string AdditionalIntolerance
        {
            get
            {
                return this.CANTEEN_Subscriber.AdditionalIntolerance;
            }
        }
        [NotMapped] 
        [Required]
        public string? DescriptionRequired 
        {
            get
            {
                return Description;
            } 
            set
            {
                Description = value;
            }
        }
        [NotMapped]
        [Required]
        public decimal? FeeRequired
        {
            get
            {
                return Fee;
            }
            set
            {
                Fee = value;
            }
        }
    }
}
