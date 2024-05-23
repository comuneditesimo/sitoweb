using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IFormBuilderHelper
    {
        public List<FORM_Definition_Field_Type> GetDefintionFieldTypeList();
        public List<FORM_Definition_Field_SubType> GetDefintionFieldSubTypeList();
        public FORM_Definition_Field_Type? GetDefintionFieldType(Guid ID);
        public FORM_Definition_Field_SubType? GetDefintionFieldSubType(Guid ID);
        public void SetDefinitionFieldType(List<FORM_Definition_Field_Type> Data);
        public void SetDefinitionFieldSubType(List<FORM_Definition_Field_SubType> Data);
        public string? FormElementType { get; set; }
        public string? FormElementRequired { get; set; }
        public string? LayoutElementColumn { get; set; }
        public string? Copy { get; set; }
        public string? Edit { get; set; }
        public string? Delete { get; set; }
        public string? FormElementSignatureDetail { get; set; }
        public string? CanBeRepeated { get; set; }
        public Guid? CurrentLanguage { get; set; }
        public FORM_Definition FormDefinition { get; set; }
        public List<LANG_Languages>? Languages { get; set; }
        public event Action OnCurrentLanguageChanged;
        public event Action OnFormDefinitionChanged;
        public event Action OnCurrentLanguageChangedSingleNotification;
        public void NotifySingleLanguageChanged();
    }
}
