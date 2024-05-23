using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Application
    {
        [NotMapped]
        public string Mantainance_Title_ErrorCode
        {
            get; set;
        }
        [NotMapped]
        public double? Lat
        {
            get
            {
                if (!string.IsNullOrEmpty(Mantainance_Lat))
                {
                    return double.Parse(Mantainance_Lat);
                }

                return null;
            }
            set
            {
                if (value != null) 
                {
                    Mantainance_Lat = value.ToString();
                }
                else
                {
                    Mantainance_Lat = null;
                }
            }
        }
        [NotMapped]
        public double? Lng
        {
            get
            {
                if (!string.IsNullOrEmpty(Mantainance_Lan))
                {
                    return double.Parse(Mantainance_Lan);
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    Mantainance_Lan = value.ToString();
                }
                else
                {
                    Mantainance_Lan = null;
                }
            }
        }
        [NotMapped]
        public double?[] Mantainance_LanLat
        {
            get
            {
                double? lat = null;
                double? lan = null;

                if (!string.IsNullOrEmpty(Mantainance_Lat))
                {
                    lat = double.Parse(Mantainance_Lat);
                }
                if (!string.IsNullOrEmpty(Mantainance_Lan))
                {
                    lan = double.Parse(Mantainance_Lan);
                }

                return new double?[] { lat, lan };
            }
        }
        [NotMapped]
        public string? Mantainance_LanLat_Title
        {
            get; set;
        }
        [NotMapped]
        public bool PrivacyBool
        {
            get
            {
                if (PrivacyReadAt != null)
                    return true;


                return false;
            }
            set
            {
                if (value == true)
                {
                    PrivacyReadAt = DateTime.Now;
                }
                else
                {
                    PrivacyReadAt = null;
                }
            }
        }
        [NotMapped]
        public string? PrivacyErrorCSS { get; set; }
        [NotMapped]
        public bool ArchivedBool
        {
            get
            {
                if (Archived != null)
                    return true;


                return false;
            }
            set
            {
                if (value == true)
                {
                    Archived = DateTime.Now;
                }
                else
                {
                    Archived = null;
                }
            }
        }
        [NotMapped]
        public bool HasUnreadChatMessages { get; set; } = false;
    }
}

