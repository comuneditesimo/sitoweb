using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Definition_Payment_Position
    {
        [NotMapped] public string Description { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [NotMapped]
        public decimal? AmountFormatted
        {
            get
            {
                return Amount;
            }
            set
            {
                Amount = value;
            }
        }
    }
}
