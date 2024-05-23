using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class HOME_Questionnaire
    {
        [NotMapped]
        public double? StarsDouble 
        {
            get
            {
                return (double?)Stars;
            }
            set
            {
                Stars = (int?)value;
            }
        }
    }
}
