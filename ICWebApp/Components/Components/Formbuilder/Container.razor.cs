using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Popups;
using System.Reflection.Metadata;
using System.Security.Cryptography.Xml;
using Telerik.Blazor;

namespace ICWebApp.Components.Components.Formbuilder
{
    public partial class Container
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LanguageProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IFormBuilderHelper FormBuilderHelper { get; set; }

        [Parameter] public FORM_Definition Definition { get; set; }
        [Parameter] public Guid? CurrentLanguage { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        public bool HasChanges { get; set; } = false;
        private List<FORM_Definition_Field_Type> FieldTypes { get; set; }
        private FORM_Definition_Field_Type? DraggedFieldType { get; set; }
        private FORM_Definition_Field? DraggedField { get; set; }
        private bool IsBusy { get; set; } = true;
        private FORM_Definition_Field? EditModel { get; set; }

        protected override async Task OnInitializedAsync()
        {
            BusyIndicatorService.IsBusy = true;
            IsBusy = true;
            StateHasChanged();

            if (Definition.FORM_Definition_Field == null || Definition.FORM_Definition_Field.Count() == 0)
            {
                Definition.FORM_Definition_Field = await GetFields();

                if (Definition.FORM_Definition_Field == null)
                {
                    Definition.FORM_Definition_Field = new List<FORM_Definition_Field>();
                }
            }

            FieldTypes = FormDefinitionProvider.GetDefinitionFieldTypeList();
            var subTypes = FormDefinitionProvider.GetDefinitionFieldSubTypeList();

            foreach (var s in subTypes)
            {
                s.Name = TextProvider.Get(s.TEXT_SystemTEXT_Code);
            }

            FormBuilderHelper.SetDefinitionFieldType(FieldTypes);
            FormBuilderHelper.SetDefinitionFieldSubType(subTypes);

            FormBuilderHelper.FormElementType = TextProvider.Get("FORM_ELEMENT_TYPE");
            FormBuilderHelper.FormElementRequired = TextProvider.Get("FORM_ELEMENT_REQUIRED");
            FormBuilderHelper.LayoutElementColumn = TextProvider.Get("LAYOUT_ELEMENT_COLUMN");
            FormBuilderHelper.Edit = TextProvider.Get("LAYOUT_ELEMENT_EDIT");
            FormBuilderHelper.Delete = TextProvider.Get("LAYOUT_ELEMENT_REMOVE");
            FormBuilderHelper.Copy = TextProvider.Get("LAYOUT_ELEMENT_COPY");
            FormBuilderHelper.FormElementSignatureDetail = TextProvider.Get("FORM_ELEMENT_SIGNATURE_DETAIL");
            FormBuilderHelper.CanBeRepeated = TextProvider.Get("FORM_ELEMENT_CAN_BE_REPEATED");

            FormBuilderHelper.Languages = await LanguageProvider.GetAll();

            await base.OnInitializedAsync();
        }
        protected override void OnParametersSet()
        {
            if (CurrentLanguage != null)
            {
                FormBuilderHelper.CurrentLanguage = CurrentLanguage.Value;
            }

            if(Definition != null)
            {
                FormBuilderHelper.FormDefinition = Definition;
            }

            base.OnParametersSet();
        }
        public async Task<List<FORM_Definition_Field>> GetFields()
        {
            return await FormDefinitionProvider.GetDefinitionFieldList(Definition.ID);
        }
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                BusyIndicatorService.IsBusy = false;
                IsBusy = false;
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }
        private async Task<bool> AddElement(FORM_Definition_Field_Type Element, Formbuilder_DragAndDropItem? DropItem = null)
        {
            IsBusy = true;
            StateHasChanged();

            var definitionField = new FORM_Definition_Field();

            definitionField.IsNew = true;
            definitionField.FORM_Definition_Field_Extended = new List<FORM_Definition_Field_Extended>();
            definitionField.ID = Guid.NewGuid();
            definitionField.FORM_Definition_ID = Definition.ID;

            definitionField.FORM_Definition_Fields_Type_ID = Element.ID;

            definitionField.DatabaseName = Element.Description + "-" + (Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Fields_Type_ID == definitionField.FORM_Definition_Fields_Type_ID).Count() + 1).ToString();

            if (DropItem != null && DropItem.ParentID != null)
            {
                definitionField.ColumnPos = DropItem.ColumnPos;
                definitionField.FORM_Definition_Field_Parent_ID = DropItem.ParentID;
            }
            else
            {
                definitionField.ColumnPos = null;
                definitionField.FORM_Definition_Field_Parent_ID = null;
            }

            if (definitionField != null)
            {
                if (FormBuilderHelper.Languages != null)
                {
                    foreach (var l in FormBuilderHelper.Languages)
                    {
                        if (definitionField.FORM_Definition_Field_Extended == null)
                        {
                            definitionField.FORM_Definition_Field_Extended = new List<FORM_Definition_Field_Extended>();
                        }

                        if (definitionField.FORM_Definition_Field_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                        {
                            var ext = new FORM_Definition_Field_Extended()
                            {
                                ID = Guid.NewGuid(),
                                FORM_Definition_Field_ID = definitionField.ID,
                                LANG_Languages_ID = l.ID
                            };

                            definitionField.FORM_Definition_Field_Extended.Add(ext);
                        }
                    }
                }

                if (DropItem == null || DropItem.Count == null)
                {
                    definitionField.SortOrder = Definition.FORM_Definition_Field.Where(p => p.ColumnPos == null).Count() + 1;
                }
                else
                {
                    long? columnPos = null;
                    long? count = null;

                    if (DropItem != null)
                    {
                        columnPos = DropItem.ColumnPos;
                        count = DropItem.Count;
                    }

                    Definition.FORM_Definition_Field.Where(x => x.ColumnPos == columnPos && x.SortOrder >= count).ToList().ForEach(x => x.SortOrder++);
                    definitionField.SortOrder = count;
                }

                if(Element.IsContainer == true)
                {
                    definitionField.ColumnCount = 2;
                }

                definitionField.Placeholder = TextProvider.Get("FORM_ELEMENT_PLACEHOLDER");

                if (Element.ID == FORMElements.Textbox)
                {
                    definitionField.FORM_Definition_Fields_SubType_ID = Guid.Parse("4b212e9c-b804-4ed4-9550-c31aad019b4d"); //Subtype Text
                }
                if (Element.ID == FORMElements.Number)
                {
                    definitionField.DecimalPlaces = 0;
                }
                if (Element.ID == FORMElements.CalculatingFields)
                {
                    definitionField.DecimalPlaces = 0;
                    definitionField.OperatorCalculatedField = 1;
                    FORM_Definition_Field_Reference newCalculatedFieldsReference = new FORM_Definition_Field_Reference();
                    newCalculatedFieldsReference.ID = Guid.NewGuid();
                    newCalculatedFieldsReference.FORM_Definition_Field_ID = definitionField.ID;
                    newCalculatedFieldsReference.FORM_Definition_Field_Source_ID = null;
                    newCalculatedFieldsReference.TriggerValue = null;
                    newCalculatedFieldsReference.IsCalculatingFieldReference = true;
                    newCalculatedFieldsReference.SortOrder = 0;
                    newCalculatedFieldsReference.Negate = false;
                    definitionField.FORM_Definition_Field_ReferenceFORM_Definition_Field.Add(newCalculatedFieldsReference);
                    newCalculatedFieldsReference = new FORM_Definition_Field_Reference();
                    newCalculatedFieldsReference.ID = Guid.NewGuid();
                    newCalculatedFieldsReference.FORM_Definition_Field_ID = definitionField.ID;
                    newCalculatedFieldsReference.FORM_Definition_Field_Source_ID = null;
                    newCalculatedFieldsReference.TriggerValue = null;
                    newCalculatedFieldsReference.IsCalculatingFieldReference = true;
                    newCalculatedFieldsReference.SortOrder = 1;
                    newCalculatedFieldsReference.Negate = false;
                    definitionField.FORM_Definition_Field_ReferenceFORM_Definition_Field.Add(newCalculatedFieldsReference);
                }

                Definition.FORM_Definition_Field.Add(definitionField);

                if (Element.IsEditable == true || Element.IsContainer == true)
                {
                    EditModel = definitionField;
                }
            }

            HasChanges = true;
            IsBusy = false;
            StateHasChanged();

            return true;
        }
        private async Task<bool> DropHandler(Formbuilder_DragAndDropItem DropItem)
        {
            IsBusy = true;
            StateHasChanged();

            if (DraggedFieldType != null)
            {
                await AddElement(DraggedFieldType, DropItem);
            }

            if (DraggedField != null)
            {
                Definition.FORM_Definition_Field.Where(x => x.ColumnPos == DropItem.ColumnPos && x.SortOrder >= DropItem.Count && x.FORM_Definition_Field_Parent_ID == DropItem.ParentID).ToList().ForEach(x => x.SortOrder++);

                DraggedField.SortOrder = DropItem.Count;
                DraggedField.ColumnPos = DropItem.ColumnPos;
                DraggedField.FORM_Definition_Field_Parent_ID = DropItem.ParentID;
            }

            await JSRuntime.InvokeVoidAsync("formBuilder_clearDraggableClass");
            await JSRuntime.InvokeVoidAsync("formBuilder_clearDraggableContainerClass");


            DraggedFieldType = null;
            DraggedField = null;

            if (DraggedFieldType == null)
            {
                ReorderList(DropItem.ParentID);
            }

            HasChanges = true;
            IsBusy = false;
            StateHasChanged();

            return true;
        }
        private async void DragStartHandler(FORM_Definition_Field_Type t)
        {
            await JSRuntime.InvokeVoidAsync("SetElementContext", "builder");
            DraggedFieldType = t;
            StateHasChanged();
        }
        private async Task<bool> DragStopHandler()
        {
            await JSRuntime.InvokeVoidAsync("formBuilder_clearDraggableClass");
            DraggedFieldType = null;
            StateHasChanged();

            return true;
        }
        private async void DragFieldStartHandler(FORM_Definition_Field f)
        {
            if (f != null)
            {
                await JSRuntime.InvokeVoidAsync("SetElementContext", "builder");
                DraggedField = f;
            }
        }
        private async Task<bool> DragFieldEndHandler(FORM_Definition_Field f)
        {
            if (f != null)
            {
                await JSRuntime.InvokeVoidAsync("formBuilder_clearDraggableClass");
                DraggedField = null;
            }
            return true;
        }
        private bool MoveUp(FORM_Definition_Field f)
        {
            StateHasChanged();
            IsBusy = true;

            var previousItem = Definition.FORM_Definition_Field.FirstOrDefault(p => p.FORM_Definition_Field_Parent_ID == f.FORM_Definition_Field_Parent_ID && p.ColumnPos == f.ColumnPos && p.SortOrder == f.SortOrder - 1);

            if (previousItem != null)
            {
                previousItem.SortOrder = previousItem.SortOrder + 1;
                f.SortOrder = f.SortOrder - 1;

            }

            ReorderList(f.FORM_Definition_Field_Parent_ID);

            f.NotifyOnChanged();

            HasChanges = true;
            IsBusy = false;
            StateHasChanged();
            return true;
        }
        private bool MoveDown(FORM_Definition_Field f)
        {
            IsBusy = true;
            StateHasChanged();

            var previousItem = Definition.FORM_Definition_Field.FirstOrDefault(p => p.FORM_Definition_Field_Parent_ID == f.FORM_Definition_Field_Parent_ID && p.ColumnPos == f.ColumnPos && p.SortOrder == f.SortOrder + 1);
            if (previousItem != null)
            {
                previousItem.SortOrder = previousItem.SortOrder - 1;
                f.SortOrder = f.SortOrder + 1;

            }

            ReorderList(f.FORM_Definition_Field_Parent_ID);

            f.NotifyOnChanged();

            HasChanges = true;
            IsBusy = false;
            StateHasChanged();
            return true;
        }
        private async Task<bool> OnRemoveField(FORM_Definition_Field f)
        {
            if (f != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("FORM_ELEMENT_DELETE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return false;


                IsBusy = true;
                StateHasChanged();

                RemoveFields(f);
                f.ToRemove = true;

                ReorderList(null);

                HasChanges = true;
                IsBusy = false;
                StateHasChanged();
            }
            return true;
        }
        private async Task<bool> OnRemoveFieldWithSubFields(FORM_Definition_Field f)
        {
            if (f != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("FORM_ELEMENT_DELETE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return false;

                IsBusy = true;
                StateHasChanged();

                RemoveFields(f);
                f.ToRemove = true;

                ReorderList(null);

                HasChanges = true;
                IsBusy = false;
                StateHasChanged();
            }
            return true;
        }
        private bool ReorderList(Guid? FORM_Definition_Field_Parent_ID)
        {

            foreach (var col in Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Field_Parent_ID == FORM_Definition_Field_Parent_ID).OrderBy(p => p.SortOrder).Select(p => p.ColumnPos).Distinct().ToList())
            {
                int Sort = 0;

                foreach (var d in Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Field_Parent_ID == FORM_Definition_Field_Parent_ID && p.ColumnPos == col).ToList().OrderBy(p => p.SortOrder))
                {
                    d.SortOrder = Sort;
                    Sort++;                    
                }
            }

            return true;
        }
        public async Task<bool> SaveForm()
        {
            IsBusy = true;
            StateHasChanged();

            await Task.Delay(1);

            foreach(var referenceToReset in Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Field_ReferenceFORM_Definition_Field != null && 
                                                                                        p.FORM_Definition_Field_ReferenceFORM_Definition_Field.FirstOrDefault(p => p.FORM_Definition_Field_Source != null 
                                                                                        && p.FORM_Definition_Field_Source.ToRemove == true) != null).ToList()
                   )
            {
                var valuesToReset = referenceToReset.FORM_Definition_Field_ReferenceFORM_Definition_Field.Where(p => p.FORM_Definition_Field_Source != null && p.FORM_Definition_Field_Source.ToRemove == true);

                foreach(var reset in valuesToReset)
                {
                    reset.FORM_Definition_Field_Source_ID = null;
                    reset.FORM_Definition_Field_Source = null;
                }
            }

            foreach (var subf in Definition.FORM_Definition_Field.Where(p => p.ToRemove != true).ToList())
            {
                await FormDefinitionProvider.SetDefinitionField(subf);
            }

            foreach (var subf in Definition.FORM_Definition_Field.Where(p => p.ToRemove != true).ToList())
            {
                foreach (var ext in subf.FORM_Definition_Field_Extended)
                {
                    await FormDefinitionProvider.SetDefinitionFieldExtended(ext);
                }

                foreach (var option in subf.FORM_Definition_Field_Option.Where(p => p.ToRemove != true).ToList())
                {
                    await FormDefinitionProvider.SetDefinitionFieldOption(option);

                    foreach (var ext in option.FORM_Definition_Field_Option_Extended)
                    {
                        await FormDefinitionProvider.SetDefinitionFieldOptionExtended(ext);
                    }
                }
                
                foreach (var reference in subf.FORM_Definition_Field_ReferenceFORM_Definition_Field.ToList())
                {
                    if (reference.ToRemove != true)
                    {
                        await FormDefinitionProvider.SetDefinitionFieldReference(reference);
                    }
                    else
                    {
                        await FormDefinitionProvider.RemoveDefinitionFieldReference(reference.ID);
                    }
                }
            }

            foreach (var reference in Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Field_ReferenceFORM_Definition_Field.FirstOrDefault(p => p.ToRemove == true) != null).ToList())
            {
                foreach (var option in reference.FORM_Definition_Field_ReferenceFORM_Definition_Field.Where(p => p.ToRemove == true).ToList())
                {
                    await FormDefinitionProvider.RemoveDefinitionFieldOption(option.ID);
                }
            }

            foreach (var subf in Definition.FORM_Definition_Field.Where(p => p.ToRemove == true).ToList())
            {
                var sourceReference = await FormDefinitionProvider.GetDefinitionFieldReferenceBySourceList(subf.ID);

                foreach (var reference in sourceReference)
                {
                    await FormDefinitionProvider.RemoveDefinitionFieldReference(reference.ID);
                }
                var targetReference = await FormDefinitionProvider.GetDefinitionFieldReferenceList(subf.ID);

                foreach (var reference in targetReference)
                {
                    await FormDefinitionProvider.RemoveDefinitionFieldReference(reference.ID);
                }

                subf.FORM_Definition_Field_ReferenceFORM_Definition_Field_Source.Clear();
                subf.FORM_Definition_Field_ReferenceFORM_Definition_Field.Clear();

                foreach (var option in subf.FORM_Definition_Field_Option.ToList())
                {
                    await FormDefinitionProvider.RemoveDefinitionFieldOption(option.ID);
                }
            }

            foreach (var reference in Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Field_Option.FirstOrDefault(p => p.ToRemove == true) != null).ToList())
            {
                foreach (var option in reference.FORM_Definition_Field_Option.Where(p => p.ToRemove == true).ToList())
                {
                    await FormDefinitionProvider.RemoveDefinitionFieldOption(option.ID);
                }
            }

            foreach (var subf in Definition.FORM_Definition_Field.Where(p => p.ToRemove == true).ToList())
            {
                await FormDefinitionProvider.RemoveDefinitionField(subf.ID);
            }

            Definition.FORM_Definition_Field = await GetFields();

            HasChanges = false;
            IsBusy = false;
            StateHasChanged();
            return true;
        }
        private void RemoveFields(FORM_Definition_Field ParentField)
        {
            foreach (var subF in Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Field_Parent_ID == ParentField.ID).ToList())
            {
                if(Definition.FORM_Definition_Field.Where(p => p.FORM_Definition_Field_Parent_ID == subF.ID).Count() > 0)
                {
                    RemoveFields(subF);
                }

                subF.ToRemove = true;
            }
        }
        private async Task<FORM_Definition_Field> OnCopy(FORM_Definition_Field Model)
        {
            IsBusy = true;
            await InvokeAsync(StateHasChanged);

            Guid copyingGroupID = Guid.NewGuid();
            FORM_Definition_Field result = await Copy(Model, copyingGroupID);
            List<FORM_Definition_Field> definitionFields = Definition.FORM_Definition_Field.Where(p => p.CopyingGroupID == copyingGroupID).ToList();

            foreach (FORM_Definition_Field definitionField in definitionFields.Where(p => p.FORM_Definition_Field_ReferenceFORM_Definition_Field.Any()))
            {
                foreach (FORM_Definition_Field_Reference reference in definitionField.FORM_Definition_Field_ReferenceFORM_Definition_Field)
                {
                    FORM_Definition_Field? matchingElement = definitionFields.FirstOrDefault(p => p.CopiedFromFieldID == reference.FORM_Definition_Field_Source_ID);
                    if (matchingElement != null)
                    {
                        reference.FORM_Definition_Field_Source_ID = matchingElement.ID;
                        if (!string.IsNullOrEmpty(reference.TriggerValue))
                        {
                            FORM_Definition_Field_Option? option = matchingElement.FORM_Definition_Field_Option.FirstOrDefault(p => p.CopiedFromOptionID != null &&
                                                                                                                                    p.CopiedFromOptionID.Value.ToString().ToLower() == reference.TriggerValue.ToLower());
                            if (option != null)
                            {
                                reference.TriggerValue = option.ID.ToString();
                            }
                        }
                    }
                }
            }

            HasChanges = true;
            IsBusy = false;
            await InvokeAsync(StateHasChanged);

            return result;
        }
        private async Task<FORM_Definition_Field> Copy(FORM_Definition_Field Model, Guid CopyingGroupID, Guid? parentID = null)
        {
            // Hole alle Tabellen/Elemente von
            FORM_Definition_Field_Type? definition_Field_Type = FormDefinitionProvider.GetDefinitionFieldTypeByFieldID(Model.ID);
            List<FORM_Definition_Field_Extended> definitionFieldExtended = Model.FORM_Definition_Field_Extended.ToList();
            List<FORM_Definition_Field_Option> definitionFieldOptions = Model.FORM_Definition_Field_Option.ToList();
            List<FORM_Definition_Field_Reference> definitionFieldReferences = Model.FORM_Definition_Field_ReferenceFORM_Definition_Field.ToList();
            List<FORM_Definition_Field> actualList = Definition.FORM_Definition_Field.ToList();

            FORM_Definition_Field newDefinitionField = new FORM_Definition_Field();

            newDefinitionField.IsNew = true;
            newDefinitionField.ID = Guid.NewGuid();
            newDefinitionField.CopyingGroupID = CopyingGroupID;
            newDefinitionField.CopiedFromFieldID = Model.ID;
            newDefinitionField.FORM_Definition_Field_Extended = new List<FORM_Definition_Field_Extended>();
            newDefinitionField.FORM_Definition_Field_Option = new List<FORM_Definition_Field_Option>();
            newDefinitionField.FORM_Definition_ID = Model.FORM_Definition_ID;
            if (parentID != null)
            {
                newDefinitionField.FORM_Definition_Field_Parent_ID = parentID;
            }
            else
            {
                newDefinitionField.FORM_Definition_Field_Parent_ID = Model.FORM_Definition_Field_Parent_ID;
            }

            if (definition_Field_Type != null)
            {
                newDefinitionField.FORM_Definition_Fields_Type_ID = definition_Field_Type.ID;
                newDefinitionField.DatabaseName = definition_Field_Type.Description + "-" + (actualList.Where(p => p.FORM_Definition_Fields_Type_ID == Model.FORM_Definition_Fields_Type_ID).Count() + 1).ToString();
            }
            else
            {
                newDefinitionField.FORM_Definition_Fields_Type_ID = Model.FORM_Definition_Fields_Type_ID;
                newDefinitionField.DatabaseName = Model.DatabaseName.Split("-").FirstOrDefault() + "-" + (actualList.Where(p => p.FORM_Definition_Fields_Type_ID == Model.FORM_Definition_Fields_Type_ID).Count() + 1).ToString();
            }

            foreach (FORM_Definition_Field_Extended extended in definitionFieldExtended)
            {
                FORM_Definition_Field_Extended newExtended = new FORM_Definition_Field_Extended();
                newExtended.ID = Guid.NewGuid();
                newExtended.FORM_Definition_Field_ID = newDefinitionField.ID;
                newExtended.LANG_Languages_ID = extended.LANG_Languages_ID;
                if (!string.IsNullOrEmpty(extended.Name) &&
                    !extended.Name.EndsWith(" (Copia)") &&
                    extended.LANG_Languages_ID == LanguageSettings.Italian)
                {
                    //newExtended.Name = extended.Name + " (Copia)";
                    newExtended.Name = extended.Name;
                }
                else if (!string.IsNullOrEmpty(extended.Name) &&
                         !extended.Name.EndsWith(" (Kopie)"))
                {
                    //newExtended.Name = extended.Name + " (Kopie)";
                    newExtended.Name = extended.Name;
                }
                else
                {
                    newExtended.Name = extended.Name;
                }
                newDefinitionField.FORM_Definition_Field_Extended.Add(newExtended);
            }
            foreach (FORM_Definition_Field_Option option in definitionFieldOptions)
            {
                List<FORM_Definition_Field_Option_Extended> optionExtended = option.FORM_Definition_Field_Option_Extended.ToList();

                FORM_Definition_Field_Option newOption = new FORM_Definition_Field_Option();
                newOption.ID = Guid.NewGuid();
                newOption.CopyingGroupID = CopyingGroupID;
                newOption.CopiedFromOptionID = option.ID;
                newOption.FORM_Definition_Field_ID = newDefinitionField.ID;
                newOption.FORM_Definition_Field_Option_Extended = new List<FORM_Definition_Field_Option_Extended>();
                newOption.SortOrder = option.SortOrder;
                newOption.AdditionalCharge = option.AdditionalCharge;
                newOption.BolloFree = option.BolloFree;
                newOption.AdditionalChargePagoPaIdentifierId = option.AdditionalChargePagoPaIdentifierId;

                foreach (FORM_Definition_Field_Option_Extended extended in optionExtended)
                {
                    FORM_Definition_Field_Option_Extended newExtended = new FORM_Definition_Field_Option_Extended();
                    newExtended.ID = Guid.NewGuid();
                    newExtended.FORM_Definition_Field_Option_ID = newOption.ID;
                    newExtended.LANG_Languages_ID = extended.LANG_Languages_ID;
                    newExtended.Description = extended.Description;
                    newOption.FORM_Definition_Field_Option_Extended.Add(newExtended);
                }

                newDefinitionField.FORM_Definition_Field_Option.Add(newOption);
            }
            foreach (FORM_Definition_Field_Reference reference in definitionFieldReferences)
            {
                FORM_Definition_Field_Reference newReference = new FORM_Definition_Field_Reference();
                newReference.ID = Guid.NewGuid();
                newReference.FORM_Definition_Field_ID = newDefinitionField.ID;
                newReference.FORM_Definition_Field_Source_ID = reference.FORM_Definition_Field_Source_ID;
                newReference.IsCalculatingFieldReference = reference.IsCalculatingFieldReference;
                newReference.TriggerValue = reference.TriggerValue;
                newReference.SortOrder = reference.SortOrder;
                newReference.Negate = reference.Negate;

                newDefinitionField.FORM_Definition_Field_ReferenceFORM_Definition_Field.Add(newReference);
            }

            long? position = Model.SortOrder;
            if (position != null)
            {
                foreach (FORM_Definition_Field item in Definition.FORM_Definition_Field.Where(p => p.ColumnPos == Model.ColumnPos && p.SortOrder > position.Value))
                {
                    item.SortOrder++;
                }
            }
            newDefinitionField.Required = Model.Required;
            newDefinitionField.SortOrder = position + 1;
            newDefinitionField.ColumnPos = Model.ColumnPos;
            newDefinitionField.ColumnCount = Model.ColumnCount;
            newDefinitionField.HasBorder = Model.HasBorder;
            newDefinitionField.ReferenceValueLimit = Model.ReferenceValueLimit;
            newDefinitionField.IsAnchor = Model.IsAnchor;
            newDefinitionField.ShowOnMunicipalSite = Model.ShowOnMunicipalSite;
            newDefinitionField.CanBeRepeated = Model.CanBeRepeated;
            newDefinitionField.AdditionalCharge = Model.AdditionalCharge;
            newDefinitionField.HasAdditionalCharge = Model.HasAdditionalCharge;
            newDefinitionField.Ref_Signature_Name = Model.Ref_Signature_Name;
            newDefinitionField.Ref_Signature_Mail = Model.Ref_Signature_Mail;
            newDefinitionField.BolloFree = Model.BolloFree;
            newDefinitionField.UploadMultiple = Model.UploadMultiple;
            newDefinitionField.OnlyShowInFormRenderer = Model.OnlyShowInFormRenderer;
            newDefinitionField.DecimalPlaces = Model.DecimalPlaces;
            newDefinitionField.OperatorCalculatedField = Model.OperatorCalculatedField;
            newDefinitionField.Placeholder = Model.Placeholder;
            newDefinitionField.FORM_Definition_Fields_SubType_ID = Model.FORM_Definition_Fields_SubType_ID;

            Definition.FORM_Definition_Field.Add(newDefinitionField);
            foreach (FORM_Definition_Field item in actualList.Where(p => p.FORM_Definition_Field_Parent_ID == Model.ID))
            {
                await Copy(item, CopyingGroupID, newDefinitionField.ID);
            }

            return newDefinitionField;
        }
        private void OnEdit(FORM_Definition_Field Model)
        {
            EditModel = Model;
            StateHasChanged();
        }
        private void OnEditClose()
        {
            HasChanges = true;
            EditModel = null;
            StateHasChanged();
        }
        private void Refresh()
        {
            StateHasChanged();
        }
    }
}
