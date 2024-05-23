using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class CANTEEN_Canteen
    {
        [NotMapped] public string SchoolName { get; set; }

        [NotMapped]
        public string CanteenDisplaylName
        {
            get
            {
                string result = this.Name;
                if (this.AddressInfo != null && this.AddressInfo!="")
                {
                    result = result + " (" + this.AddressInfo + ")";
                }

                return result;
            }
            
        }
    }
}
