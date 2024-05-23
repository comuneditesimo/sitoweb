using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using System.Drawing.Text;

namespace ICWebApp.Components.Components.FormRenderer
{
    public partial class Element
    {
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IFormRendererHelper FormRendererHelper { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Parameter] public FORM_Definition_Field? DefinitionField { get; set; }
        [Parameter] public FORM_Application_Field_Data? ApplicationField { get; set; }
        [Parameter] public List<FORM_Definition_Field_Reference>? ReferenceList { get; set; }
        [Parameter] public FORM_Definition? Definition { get; set; }
        private Guid CurrentLanguage { get; set; }
        private bool IsReferenced = false;
        private List<FORM_Definition_Field_Option> FieldOptions = new List<FORM_Definition_Field_Option>();
        private FORM_Definition_Field_SubType? SubType;
        private bool HasReference = false;
        private bool CheckBoxValue
        {
            get
            {
                if (ApplicationField != null)
                {
                    return ApplicationField.BoolValue;
                }

                return false;
            }
            set
            {
                if (ApplicationField != null)
                {
                    ApplicationField.BoolValue = value;
                    Notify();
                }
            }
        }
        private Guid? GuidReferenceValue
        {
            get
            {
                if (ApplicationField != null)
                {
                    return ApplicationField.GuidValue;
                }

                return null;
            }
            set
            {
                if (ApplicationField != null)
                {
                    ApplicationField.GuidValue = value;
                    Notify();
                }
            }
        }
        private int? IntReferenceValue
        {
            get
            {
                if (ApplicationField != null)
                {
                    return ApplicationField.IntValue;
                }

                return null;
            }
            set
            {
                if (ApplicationField != null)
                {
                    ApplicationField.IntValue = value;
                    Notify();
                }
            }
        }
        private decimal? DecimalReferenceValue
        {
            get
            {
                if (ApplicationField != null)
                {
                    return ApplicationField.DecimalValue;
                }

                return null;
            }
            set
            {
                if (ApplicationField != null)
                {
                    if (DefinitionField != null && value != null && DefinitionField.DecimalPlaces != null)
                    {
                        ApplicationField.DecimalValue = Math.Round(value.Value, DefinitionField.DecimalPlaces.Value);
                    }
                    else
                    {
                        ApplicationField.DecimalValue = value;
                    }
                    Notify();
                }
            }
        }

        protected override void OnInitialized()
        {

            StateHasChanged();
            base.OnInitialized();
        }
        protected override async Task OnParametersSetAsync()
        {
            CurrentLanguage = LANGProvider.GetCurrentLanguageID();

            InitializeReferences();
            InitialzeValidation();
            await GetOptions();
            GetSubType();

            StateHasChanged();

            await base.OnParametersSetAsync();
        }
        private void InitializeReferences()
        {
            if (DefinitionField != null && ReferenceList != null)
            {
                var Reference = ReferenceList.FirstOrDefault(p => p.FORM_Definition_Field_Source_ID == DefinitionField.ID);

                if (Reference != null)
                {
                    IsReferenced = true;
                }
            }

            if (ApplicationField != null && ReferenceList != null)
            {
                var reference = ReferenceList.FirstOrDefault(p => p.FORM_Definition_Field_ID == ApplicationField.FORM_Definition_Field_ID && p.IsCalculatingFieldReference != true);

                if (reference != null)
                {
                    FormRendererHelper.OnChange += FormRendererHelper_OnChange;
                    HasReference = true;
                    CheckDifferenceReference();
                }
                else if (DefinitionField != null && DefinitionField.FORM_Definition_Fields_Type_ID == FORMElements.CalculatingFields)
                {
                    FormRendererHelper.OnChange += FormRendererHelper_OnChange;
                    CalcNewCalculatingFieldValue();
                }
            }
        }
        private void InitialzeValidation()
        {
            if (DefinitionField != null && DefinitionField.Required)
            {
                FormRendererHelper.OnChange += FormRendererHelper_OnChange;
            }
        }
        private void FormRendererHelper_OnChange()
        {
            CheckDifferenceReference();
            CalcNewCalculatingFieldValue();
            StateHasChanged();
        }
        private async Task<bool> GetOptions()
        {
            if (DefinitionField != null &&
                (DefinitionField.FORM_Definition_Fields_Type_ID == FORMElements.Dropdown
                 || DefinitionField.FORM_Definition_Fields_Type_ID == FORMElements.Radiobutton
                )
               )
            {
                FieldOptions = await FormDefinitionProvider.GetDefinitionFieldOptionList(DefinitionField.ID);

                foreach (var d in FieldOptions.OrderBy(p => p.SortOrder))
                {
                    if (d.FORM_Definition_Field_Option_Extended != null && d.FORM_Definition_Field_Option_Extended.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(d.FORM_Definition_Field_Option_Extended.FirstOrDefault(p => p.LANG_Languages_ID == CurrentLanguage).Description))
                        {
                            d.Description = d.FORM_Definition_Field_Option_Extended.FirstOrDefault(p => p.LANG_Languages_ID == CurrentLanguage).Description;
                        }
                        else if(!string.IsNullOrEmpty(d.FORM_Definition_Field_Option_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LanguageSettings.Default).Description))
                        {
                            d.Description = d.FORM_Definition_Field_Option_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LanguageSettings.Default).Description;
                        }
                    }
                }

                StateHasChanged();
            }

            return true;
        }
        private void GetSubType()
        {
            if (DefinitionField != null && DefinitionField.FORM_Definition_Fields_SubType_ID != null)
            {
                SubType = FormDefinitionProvider.GetDefinitionFieldSubType(DefinitionField.FORM_Definition_Fields_SubType_ID.Value);
                StateHasChanged();
            }
        }
        private void Notify()
        {
            if (IsReferenced)
            {
                if (ApplicationField != null)
                {
                    FormRendererHelper.SetApplicationField(ApplicationField);
                }
            }
        }
        private async void OnListValueChanged(FORM_Application_Field_Data Data)
        {
            if (IsReferenced)
            {
                if (Data != null)
                {
                    FormRendererHelper.SetApplicationField(Data);
                }
            }
        }
        private void CheckDifferenceReference()
        {
            if (ApplicationField != null && DefinitionField != null
                && DefinitionField.FORM_Definition_Fields_Type_ID == FORMElements.Difference
                && HasReference == true && FormRendererHelper.GetApplicationFields() != null
                && ReferenceList != null)
            {
                decimal Value = 0;

                var firstValue = ReferenceList.Where(p => p.FORM_Definition_Field_ID == DefinitionField.ID && p.IsCalculatingFieldReference != true).OrderBy(p => p.SortOrder).FirstOrDefault();
                if (firstValue != null)
                {
                    var applicationFirstRefField = FormRendererHelper.GetApplicationFields().FirstOrDefault(p => p.FORM_Definition_Field_ID == firstValue.FORM_Definition_Field_Source_ID);

                    if (applicationFirstRefField != null && applicationFirstRefField.DecimalValue != null)
                    {
                        Value = applicationFirstRefField.DecimalValue.Value;
                    }
                }

                foreach (var f in ReferenceList.Where(p => p.FORM_Definition_Field_ID == DefinitionField.ID && p.IsCalculatingFieldReference != true).OrderBy(p => p.SortOrder).Skip(1))
                {
                    var applicationRefField = FormRendererHelper.GetApplicationFields().FirstOrDefault(p => p.FORM_Definition_Field_ID == f.FORM_Definition_Field_Source_ID);

                    if (applicationRefField != null && applicationRefField.DecimalValue != null)
                    {
                        Value -= applicationRefField.DecimalValue.Value;
                    }
                }

                ApplicationField.DecimalValue = Value;
            }

            StateHasChanged();
        }
        private void CalcNewCalculatingFieldValue()
        {
            if (DefinitionField != null && DefinitionField.FORM_Definition_Fields_Type_ID == FORMElements.CalculatingFields && ApplicationField != null)
            {
                decimal? newValue = null;
                bool isCurrency = false;
                if (DefinitionField.OperatorCalculatedField != null && ReferenceList != null && ReferenceList.Any())
                {
                    var referencedFields = ReferenceList.Where(p => p.FORM_Definition_Field_ID == DefinitionField.ID && p.IsCalculatingFieldReference).OrderBy(p => p.SortOrder).ToList();
                    foreach (FORM_Definition_Field_Reference? referencedField in referencedFields)
                    {
                        if (referencedField.FORM_Definition_Field_Source_ID == null)
                        {
                            newValue = null;
                            break;
                        }
                        var applicationField = FormRendererHelper.GetApplicationFields().FirstOrDefault(p => p.FORM_Definition_Field_ID == referencedField.FORM_Definition_Field_Source_ID);
                        if (applicationField != null && applicationField.DecimalValue != null)
                        {
                            if (newValue == null)
                            {
                                newValue = applicationField.DecimalValue;
                            }
                            else if (DefinitionField.OperatorCalculatedField.Value == OperatorSetting.Multiplication)
                            {
                                newValue = newValue * applicationField.DecimalValue;
                            }
                            else if (DefinitionField.OperatorCalculatedField.Value == OperatorSetting.Addition)
                            {
                                newValue = newValue + applicationField.DecimalValue;
                            }
                            else if (DefinitionField.OperatorCalculatedField.Value == OperatorSetting.Division)
                            {
                                if (applicationField.DecimalValue == 0)
                                {
                                    newValue = null;
                                    break;
                                }
                                newValue = newValue / applicationField.DecimalValue;
                            }
                            else if (DefinitionField.OperatorCalculatedField.Value == OperatorSetting.Subtraktion)
                            {
                                newValue = newValue - applicationField.DecimalValue;
                            }
                        }
                        else
                        {
                            newValue = null;
                            break;
                        }
                        if (isCurrency == false)
                        {
                            var definitionField = FormRendererHelper.GetDefinitionFields().FirstOrDefault(p => p.ID == referencedField.FORM_Definition_Field_Source_ID);
                            if (definitionField != null && definitionField.FORM_Definition_Fields_Type_ID != null && definitionField.FORM_Definition_Fields_Type_ID == FORMElements.Money)
                            {
                                isCurrency = true;
                            }
                        }
                    }
                }
                if (newValue != null)
                {
                    if (DefinitionField != null && DefinitionField.DecimalPlaces != null)
                    {
                        newValue = Math.Round(newValue.Value, DefinitionField.DecimalPlaces.Value);
                    }
                    else
                    {
                        newValue = newValue.Value;
                    }
                }
                // Damit sollte eine Endlosschleife verhindert werden
                if (newValue != DecimalReferenceValue)
                {
                    if (ApplicationField != null)
                    {
                        ApplicationField.IsCurrency = isCurrency;
                    }
                    DecimalReferenceValue = newValue;
                }
            }
        }
    }
}
