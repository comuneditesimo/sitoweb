using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.DBModels
{
    public partial class AUTH_Users
    {
        [NotMapped] public string SearchName { get; set; }
        [NotMapped] public string Fullname { get; set; }
        [NotMapped]
        public string FullnameComposed
        {
            get
            {
                if (Lastname != null)
                {
                    return Firstname + " " + Lastname;
                }

                return Firstname;
            }
        }
        [NotMapped]
        public string DA_Email
        {
            get 
            { 
                return Email; 
            }
            set 
            {
                Email = value; 
            }
        }
        [NotMapped]
        public string Password { get; set; }
        [NotMapped][Compare("Password", ErrorMessage = "NO_MATCH;REGISTRATION_PASSWORD")]
        public string ConfirmPassword { get; set; }
    }
}
