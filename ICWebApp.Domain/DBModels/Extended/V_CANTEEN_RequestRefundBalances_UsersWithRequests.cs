using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_CANTEEN_RequestRefundBalances_UsersWithRequests
    {
        [NotMapped]
        public string EntireName
        {
            get
            {
                StringBuilder _result = new StringBuilder();
                _result.Append(this.Firstname);
                _result.Append(" ");
                _result.Append(this.Lastname);
                return _result.ToString().Trim();
            }
        }
    }
}
