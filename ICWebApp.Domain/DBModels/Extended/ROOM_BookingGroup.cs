using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class ROOM_BookingGroup
    {
        [NotMapped] public string Fullname 
        { 
            get
            {
                return FirstName + " " + LastName;
            } 
        }
        [NotMapped] public bool PrivacyBool
        {
            get
            {
                if (PrivacyDate != null)
                    return true;

                return false;
            }
            set
            {
                if(value == true)
                    PrivacyDate = DateTime.Now;
                else 
                    PrivacyDate = null;
            }
        }
        [NotMapped][Required] public string Cancellation_OwnerReq
        {
            get
            {
                return Cancellation_Owner;
            }
            set
            {
                Cancellation_Owner = value;
            }
        }
        [NotMapped][Required] public string Cancellation_BancReq
        {
            get
            {
                return Cancellation_Banc;
            }
            set
            {
                Cancellation_Banc = value;
            }
        }
        [NotMapped][Required] public string Cancellation_IBANReq
        {
            get
            {
                return Cancellation_IBAN;
            }
            set
            {
                Cancellation_IBAN = value;
            }
        }
    }
}
