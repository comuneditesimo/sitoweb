using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Application_Field_Data
    {
        [NotMapped]
        public decimal? DecimalValue
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    decimal result;

                    var check = decimal.TryParse(Value, out result);

                    if (check)
                    {
                        return result;
                    }
                }

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
                {
                    DateTime result;

                    var check = DateTime.TryParse(Value, out result);

                    if (check)
                    {
                        return result;
                    }
                }

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
                {
                    int result;

                    var check = int.TryParse(Value, out result);

                    if (check)
                    {
                        return result;
                    }
                }

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
                {
                    bool result;

                    var check = bool.TryParse(Value, out result);

                    if (check)
                    {
                        return result;
                    }
                }

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
                {
                    Guid result;

                    var check = Guid.TryParse(Value, out result);

                    if (check)
                    {
                        return result;
                    }
                }

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
        public string? ERROR_CODE { get; set; }
        [NotMapped]
        public List<FILE_FileInfo>? FileValue { get; set; } = new List<FILE_FileInfo>();
    }
}