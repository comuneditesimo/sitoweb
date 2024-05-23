using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Definition_Field
    {
        [NotMapped] public string Placeholder { get; set; }
        [NotMapped] public string Name { get; set; }
        [NotMapped] public string NameShort 
        {
            get
            {
                if(Name != null && Name.Length > 50)
                {
                    return Name.Substring(0, 50) + "...";
                }

                if(Name != null)
                {
                    return Name;
                }

                return "";
            }
        }
        [NotMapped] public bool ReferenceOk { get; set; } = true;
        [NotMapped] public decimal? DecimalReferenceValueLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(ReferenceValueLimit))
                    return decimal.Parse(ReferenceValueLimit);

                return null;
            }
            set
            {
                if (value != null)
                {
                    ReferenceValueLimit = value.ToString();
                }
                else
                {
                    ReferenceValueLimit = null;
                }
            }
        }
        public event Action OnChange;
        public void NotifyOnChanged() => OnChange?.Invoke();
        [NotMapped] public bool ToRemove { get; set; } = false;
        [NotMapped] public bool IsNew { get; set; } = false;
        [NotMapped] public Guid? CopyingGroupID { get; set; } = null;
        [NotMapped] public Guid? CopiedFromFieldID { get; set; } = null;
    }
}
