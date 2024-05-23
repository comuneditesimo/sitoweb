using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICWebApp.Domain.DBModels
{
    public partial class CANTEEN_RequestRefundBalances
    {
        [Required]
        [NotMapped]
        public string Req_Owner
        {
            get
            {
                return Owner;
            }
            set
            {
                Owner = value;
            }
        }
        [Required]
        [NotMapped]
        public string Req_Bank
        {
            get
            {
                return Bank;
            }
            set
            {
                Bank = value;
            }
        }
        [Required]
        [NotMapped]
        public string Req_IBAN
        {
            get
            {
                return IBAN;
            }
            set
            {
                IBAN = value;
            }
        }
        [Required]
        [NotMapped]
        [Range(1, 1000000, ErrorMessage = "OUT_OF_RANGE")]
        public decimal? Req_Fee
        {
            get
            {
                return Fee;
            }
            set
            {
                Fee = value;
            }
        }
        [Required]
        [NotMapped]
        [Range(1, 1, ErrorMessage = "PRIVACY_BOOL_ERROR")]
        public bool? Req_PrivacyPolices
        {
            get
            {
                return PrivacyPolices;
            }
            set
            {
                PrivacyPolices = value;
            }
        }
        [NotMapped]
        public string UserEntireName
        {
            get
            {
                StringBuilder _result = new StringBuilder();
                if (!string.IsNullOrEmpty(this.UserFirstName))
                {
                    _result.Append(this.UserFirstName);
                }
                _result.Append(" ");
                if (!string.IsNullOrEmpty(this.UserLastName))
                {
                    _result.Append(this.UserLastName);
                }

                return _result.ToString().Trim();
            }
        }
        [NotMapped]
        public bool ArchivedBool
        {
            get
            {
                if (this.ArchivedDate != null)
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (value == true)
                {
                    this.ArchivedDate = DateTime.Now;
                }
                else
                {
                    this.ArchivedDate = null;
                }
            }
        }
        [NotMapped]
        public DateTime PaymentRefundUntil { get; set; } = DateTime.Today.AddDays(60);
    }
}
