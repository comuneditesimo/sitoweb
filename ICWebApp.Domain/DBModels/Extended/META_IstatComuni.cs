using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class META_IstatComuni
    {
        [NotMapped]
        public string FilterFields
        {
            get
            {
                if (this.NameIT.ToLower().Trim() != this.NameDE.ToLower().Trim())
                {
                    return this.NameIT + "/" + this.NameDE;
                }
                return this.NameDE;
            }
        }
        [NotMapped]
        public string Name { get; set; } = "";
    }
}
