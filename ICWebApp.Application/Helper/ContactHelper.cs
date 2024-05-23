using ICWebApp.Application.Interface.Helper;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;

namespace ICWebApp.Application.Helper
{
    public class ContactHelper : IContactHelper
    {
        public string GetFilePath(string Name, bool? Gender = null, string? Stelle = null, string? Birthplace = null, DateTime? Birthdate = null, string? PhoneNumber = null, string? PhoneNumberPrivate = null, string? PhoneNumberMobile = null, string? EMail = null, string? PECMail = null, string? Base64Photo = null)
        {
            string Filename = Name.Replace("\"", "").Replace("/", "").Replace(":", "") + ".vcf";

            if (Birthdate != null)
            {
                Filename = Name + "_" + Birthdate.Value.ToString("ddMMyyyy") + ".vcf";
            }
            string nameDocument = HttpUtility.UrlEncode(Filename);

            if (System.IO.File.Exists("D:/Comunix/Contacts/" + nameDocument))
            {
                return "Contacts/" + nameDocument;
            }

            StringBuilder sb = new StringBuilder();

            //start the calendar item
            sb.AppendLine("BEGIN:VCARD");
            sb.AppendLine("VERSION:4.0");

            //add the event
            if (!string.IsNullOrEmpty(Name))
            {
                sb.AppendLine("FN:" + Name);
            }
            if (Gender != null && Gender == true)
            {
                sb.AppendLine("GENDER:F");
            }
            else if (Gender != null)
            {
                sb.AppendLine("GENDER:M");
            }
            if (!string.IsNullOrEmpty(Stelle))
            {
                sb.AppendLine("TITLE:" + Stelle);
            }
            if (!string.IsNullOrEmpty(Birthplace))
            {
                sb.AppendLine("BIRTHPLACE:" + Birthplace);
            }
            if (Birthdate != null)
            {
                sb.AppendLine("BDAY:" + Birthdate.Value.ToString("yyyyMMdd"));
            }
            if (PhoneNumber != null)
            {
                sb.AppendLine("TEL;TYPE=voice,work,pref:" + PhoneNumber);
            }
            if (PhoneNumberPrivate != null)
            {
                sb.AppendLine("TEL;TYPE=voice,home:" + PhoneNumberPrivate);
            }
            if (PhoneNumberMobile != null)
            {
                sb.AppendLine("TEL;TYPE=cell,home:" + PhoneNumberMobile);
            }
            if (EMail != null)
            {
                sb.AppendLine("EMAIL;TYPE=work:" + EMail);
            }
            if (PECMail != null)
            {
                sb.AppendLine("EMAIL;TYPE=pec:" + EMail);
            }
            if (Base64Photo != null)
            {
                sb.AppendLine("PHOTO:data:image/jpeg;base64,[:" + Base64Photo + "]");
            }

            //end calendar item
            sb.AppendLine("END: VCARD");

            System.IO.File.WriteAllText("D:/Comunix/Contacts/" + nameDocument, sb.ToString());

            return "Contacts/" + nameDocument;
        }
    }
}
