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
    public class FormBuilderHelper : IFormBuilderHelper
    {
        private List<FORM_Definition_Field_Type> _DefinitionFieldTypes;
        private List<FORM_Definition_Field_SubType> _DefinitionFieldSubTypes;
        private Guid? _CurrentLanguage;
        private FORM_Definition _FormDefinition;
        public event Action OnCurrentLanguageChanged;
        public event Action OnCurrentLanguageChangedSingleNotification;
        public event Action OnFormDefinitionChanged;
        public List<FORM_Definition_Field_Type> GetDefintionFieldTypeList()
        {
            if (_DefinitionFieldTypes != null)
            {
                return _DefinitionFieldTypes;
            }

            return new List<FORM_Definition_Field_Type>();
        }
        public List<FORM_Definition_Field_SubType> GetDefintionFieldSubTypeList()
        {
            if (_DefinitionFieldSubTypes != null)
            {
                return _DefinitionFieldSubTypes;
            }

            return new List<FORM_Definition_Field_SubType>();
        }
        public FORM_Definition_Field_Type? GetDefintionFieldType(Guid ID)
        {
            if (_DefinitionFieldTypes != null)
            {
                return _DefinitionFieldTypes.FirstOrDefault(p => p.ID == ID);
            }

            return null;
        }
        public FORM_Definition_Field_SubType? GetDefintionFieldSubType(Guid ID)
        {
            if (_DefinitionFieldSubTypes != null)
            {
                return _DefinitionFieldSubTypes.FirstOrDefault(p => p.ID == ID);
            }

            return null;
        }
        public void SetDefinitionFieldType(List<FORM_Definition_Field_Type> Data)
        {
            _DefinitionFieldTypes = Data;
        }
        public void SetDefinitionFieldSubType(List<FORM_Definition_Field_SubType> Data)
        {
            _DefinitionFieldSubTypes = Data;
        }
        public string? FormElementType { get; set; }
        public string? FormElementRequired { get; set; }
        public string? LayoutElementColumn{ get; set; }
        public string? Copy { get; set; }
        public string? Edit { get; set; }
        public string? Delete { get; set; }
        public string? FormElementSignatureDetail { get; set; }
        public string? CanBeRepeated { get; set; }
        public Guid? CurrentLanguage 
        {
            get
            {
                return _CurrentLanguage;
            }
            set
            {
                _CurrentLanguage = value;
                NotifyCurrentLanguageChanged();
            }
        }
        public FORM_Definition FormDefinition
        {
            get
            {
                return _FormDefinition;
            }
            set
            {
                _FormDefinition = value;
                NotifyFormDefinitionChanged();
            }
        }
        public List<LANG_Languages>? Languages { get; set; }
        public void NotifyCurrentLanguageChanged()
        {
            OnCurrentLanguageChanged?.Invoke();
        }
        public void NotifySingleLanguageChanged()
        {
            OnCurrentLanguageChangedSingleNotification?.Invoke();
        }
        public void NotifyFormDefinitionChanged()
        {
            OnFormDefinitionChanged?.Invoke();
        }
    }
}
