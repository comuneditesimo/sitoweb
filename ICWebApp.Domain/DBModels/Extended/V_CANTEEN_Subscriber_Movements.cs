using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_CANTEEN_Subscriber_Movements
    {
        [NotMapped]
        public string FirstNameLastName
        {
            get
            {
                return this.FirstName + " " + this.LastName;
            }
        }
        [NotMapped]
        public string LastNameFirstName
        {
            get
            {
                return this.LastName + " " + this.FirstName;
            }
        }
        [NotMapped]
        public string School
        {
            get
            {
                return this.SchoolName + " - " + this.SchoolClass;
            }
        }
    }
}
