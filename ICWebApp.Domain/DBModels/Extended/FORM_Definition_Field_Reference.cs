using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Definition_Field_Reference
    {
        [NotMapped] public bool InEdit { get; set; } = false;
        [NotMapped] public List<FORM_Definition_Field_Option> OptionList { get; set; }
        [NotMapped] public bool TriggerValueBool
        {
            get
            {
                bool value = false;
                if (!string.IsNullOrEmpty(TriggerValue))
                {
                    if (bool.TryParse(TriggerValue, out value))
                    {
                        return value;
                    }
                }

                return false;
            }
            set
            {
                if (value != null)
                {
                    TriggerValue = value.ToString();
                }
            }
        }
        [NotMapped] public int? TriggerValueInt
        {
            get
            {
                int value = 0;
                if (!string.IsNullOrEmpty(TriggerValue))
                {
                    if (int.TryParse(TriggerValue, out value))
                    {
                        return value;
                    }
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    TriggerValue = value.ToString();
                }
                else
                {
                    TriggerValue = "";
                }
            }
        }
        [NotMapped] public decimal? TriggerValueDecimal
        {
            get
            {
                decimal value = 0;
                if (!string.IsNullOrEmpty(TriggerValue))
                {
                    if (decimal.TryParse(TriggerValue, out value))
                    {
                        return value;
                    }
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    TriggerValue = value.ToString();
                }
                else
                {
                    TriggerValue = "";
                }
            }
        }
        [NotMapped] public bool ToRemove { get; set; } = false;
        [NotMapped] public bool IsNew { get; set; } = false;
    }
}
