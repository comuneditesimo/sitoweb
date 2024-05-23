using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IFormRendererHelper
    {
        public event Action OnChange;
        public void NotifyStateChanged();
        public List<FORM_Application_Field_Data> GetApplicationFields();
        public List<FORM_Definition_Field> GetDefinitionFields();
        public void SetApplicationFields(List<FORM_Application_Field_Data> Data);
        public void SetApplicationField(FORM_Application_Field_Data Data);
        public void SetDefinitionFields(List<FORM_Definition_Field> Data);
        public int AnchorOrderID { get; set; }
    }
}
