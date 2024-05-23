using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_ORG_Requests
    {
        [NotMapped]
        public bool HasUnreadChatMessages { get; set; } = false;
    }
}
