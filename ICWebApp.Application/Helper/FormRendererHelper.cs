using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class FormRendererHelper : IFormRendererHelper
    {
        public event Action OnChange;
        private List<FORM_Application_Field_Data> _ApplicationFields;
        private List<FORM_Definition_Field> _DefinitionFields;
        public List<FORM_Application_Field_Data> GetApplicationFields()
        {
            if(_ApplicationFields != null)
            {
                return _ApplicationFields;
            }

            return new List<FORM_Application_Field_Data>();
        }
        public List<FORM_Definition_Field> GetDefinitionFields()
        {
            if (_DefinitionFields != null)
            {
                return _DefinitionFields;
            }

            return new List<FORM_Definition_Field>();
        }
        public void SetApplicationFields(List<FORM_Application_Field_Data> Data)
        {
            _ApplicationFields = Data;
        }
        public void SetDefinitionFields(List<FORM_Definition_Field> Data)
        {
            _DefinitionFields = Data;
        }
        public void SetApplicationField(FORM_Application_Field_Data Data)
        {
            var field = _ApplicationFields.FirstOrDefault(p => p.ID == Data.ID);

            if(field != null)
            {
                field.Value = Data.Value;
                field.ERROR_CODE = Data.ERROR_CODE;
                NotifyStateChanged();
            }
        }
        public void NotifyStateChanged() => OnChange?.Invoke();
        public int AnchorOrderID { get; set; } = 0;
    }
}
