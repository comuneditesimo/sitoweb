using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_CANTEEN_Subscriber_Card_Request
    {
        [NotMapped]
        public String SearchBox
        {
            get
            {
                string result = "";
                result = result + this.SubscriberTaxNumber ?? "" + " ; ";
                result = result + this.PayedAt ?? "" + " ; ";
                result = result + this.Place ?? "" + " ; ";
                result = result + this.Address ?? "" + " ; ";
                result = result + this.PLZ ?? "" + " ; ";
                result = result + this.Municipality ?? "" + " ; ";
                result = result + this.Firstname ?? "" + " ; ";
                result = result + this.Lastname ?? "" + " ; ";
                result = result + this.SubscriberName ?? "" + " ; ";
                result = result + this.SchoolName ?? "" + " ; ";

                return result;
            }
        }
    }
}
