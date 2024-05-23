using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Definition_Field_Option
    {
        [NotMapped] public string Description { get; set; }
        [NotMapped] public string DescriptionShort 
        {
            get
            {
                if (Description != null && Description.Length > 50)
                {
                    return Description.Substring(0, 50) + "...";
                }

                if (Description != null)
                {
                    return Description;
                }

                return "";
            }
        }
        [NotMapped]
        public string? StringID
        {
            get
            {
                if (ID != null)
                {
                    return ID.ToString();
                }

                return null;
            }
        }
        [NotMapped] public bool ToRemove { get; set; } = false;
        [NotMapped] public bool IsNew { get; set; } = false;
        [NotMapped] public Guid? CopyingGroupID { get; set; } = null;
        [NotMapped] public Guid? CopiedFromOptionID { get; set; } = null;
    }
}
