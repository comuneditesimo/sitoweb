using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class PAY_Transaction_Position
    {
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
