using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_FORM_Application_Field
    {
        [NotMapped]
        public decimal? DecimalValue
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
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
        [NotMapped]
        public DateTime? DateValue
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                    return DateTime.Parse(Value);

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
        [NotMapped]
        public int? IntValue
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                    return int.Parse(Value);

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
        [NotMapped]
        public bool BoolValue
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                    return bool.Parse(Value);

                return false;
            }
            set
            {
                if (value != null)
                {
                    Value = value.ToString();
                }
            }
        }
        [NotMapped]
        public Guid? GuidValue
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                    return Guid.Parse(Value);

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
