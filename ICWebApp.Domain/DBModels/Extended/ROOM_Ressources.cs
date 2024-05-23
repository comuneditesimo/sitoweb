using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class ROOM_Ressources
    {
        [NotMapped]
        public string? Description { get; set; }
    }
}
