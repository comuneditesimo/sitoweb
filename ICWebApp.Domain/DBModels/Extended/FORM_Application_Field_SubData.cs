using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Application_Field_SubData
    {
        [NotMapped] public decimal? DecimalValue
        {
            get
            {
                if(!string.IsNullOrEmpty(Value))
                    return decimal.Parse(Value);

                return null;
            }
            set
            {
                if (value != null)
                {
                    Value = value.ToString();
                }
                else
                {
                    Value = null;
                }
            }
        }
    }
}
