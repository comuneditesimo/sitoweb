using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_CANTEEN_RequestRefundBalances
    {
        [NotMapped]
        public string SearchBox
        {
            get
            {
                StringBuilder _result = new StringBuilder ();
                _result.Append(this.UserFirstName);
                _result.Append(this.UserLastName);
                _result.Append(this.UserTaxNumber);
                if (this.UserGender.ToLower() == "m")
                {
                    _result.Append("Männlich");
                    _result.Append("maschile");
                }
                else if (this.UserGender.ToLower() == "w")
                {
                    _result.Append("Weiblich");
                    _result.Append("feminile");
                }
                _result.Append(this.UserCountryOfBirth);
                _result.Append(this.UserPlaceOfBirth);
                if (this.UserDateOfBirth != null)
                {
                    _result.Append(this.UserDateOfBirth.Value.ToString("dd.MM.yyyy HH:mm"));
                }
                _result.Append(this.UserEmail);
                _result.Append(this.UserMobilePhone);
                _result.Append(this.UserDomicileEntireAdress);
                _result.Append(this.UserDomicileMunicipality);
                _result.Append(this.Owner);
                _result.Append(this.Bank);
                _result.Append(this.Status_Text);
                return _result.ToString().ToLower();
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
        }
    }
}
