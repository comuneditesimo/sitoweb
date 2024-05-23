using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IAnchorService
    {
        public void AddAnchor(string Title, string ID, int Order);
        public void ClearAnchors();
        public void RemoveAnchorByOrder(int Order);
        public List<AnchorItem> GetAnchors();
        public event Action OnAnchorChanged;
        public bool ForceShow { get; set; }
        public bool SkipForceReset { get; set; }
        public event Action OnFoceShowChanged;
    }
}
