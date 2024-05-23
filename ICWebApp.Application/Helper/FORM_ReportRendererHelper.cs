using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICWebApp.Application.Helper
{
    public class FORM_ReportRendererHelper : IFORM_ReportRendererHelper
    {
        private string BasePath = @"D:\Comunix\Reports\";
        private readonly IFORMDefinitionProvider _formDefinition;
        private readonly IFORMApplicationProvider _formApplication;
        private readonly ILANGProvider LangProvider;
        private readonly ITEXTProvider TextProvider;
        private readonly IHostingEnvironment HostingEnv;
        public FORM_ReportRendererHelper(IFORMDefinitionProvider _formDefinition, IFORMApplicationProvider _formApplication, ILANGProvider LangProvider,
                                         ITEXTProvider TextProvider, IHostingEnvironment HostingEnv, NavigationManager NavManager)
        {
            this._formDefinition = _formDefinition;
            this._formApplication = _formApplication;
            this.LangProvider = LangProvider;
            this.TextProvider = TextProvider;
            this.HostingEnv = HostingEnv;

            if (NavManager.BaseUri.Contains("localhost"))
            {
                BasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\";
            }
        }

        public async Task<int> CreateReportDefinition(Guid FORM_Definition_ID)
        {
            int reportsCount = 1;

            var report = new Report();

            var reportPackager = new ReportPackager();
            string reportPath = BasePath + @"\FormTemplateSubPage.trdp";
#if DEBUG
            reportPath = BasePath + @"\FormTemplateSubPage_Testserver.trdp";
#endif

            using (var targetStream = System.IO.File.OpenRead(reportPath))
            {
                report = reportPackager.Unpackage(targetStream);
            }

            if (report != null)
            {
                var DefintionFields = await _formDefinition.GetDefinitionFieldList(FORM_Definition_ID);
                DefintionFields = DefintionFields.Where(e => e.OnlyShowInFormRenderer != true).ToList();
                var Types = _formDefinition.GetDefinitionFieldTypeList();

                DetailSection mainSection;

                var sectionElement = report.Items.Find("dynamicSection", true)[0];

                if (sectionElement != null)
                {
                    mainSection = sectionElement as DetailSection;

                }
                else
                {
                    mainSection = new DetailSection();
                }

                mainSection.CanShrink = true;
                mainSection.KeepTogether = false;

                double CurrentYPosition = 0;
                double CurrentXPosition = 0;

                foreach (var def in DefintionFields.Where(p => p.FORM_Definition_Field_Parent_ID == null).OrderBy(p => p.SortOrder))
                {
                    if (def.FORM_Definition_Fields_Type_ID != FORMElements.PDFPager && def.FORM_Definition_Fields_Type_ID != FORMElements.FileUpload) //REPORT PAGER
                    {
                        var type = Types.FirstOrDefault(p => p.ID == def.FORM_Definition_Fields_Type_ID);

                        if (type != null)
                        {
                            if (type.IsContainer)
                            {
                                var panel = AddContainerElement(def, DefintionFields.ToList(), Types, CurrentYPosition, CurrentXPosition);

                                CurrentYPosition += panel.Height.Value;

                                mainSection.Items.Add(panel);
                            }
                            else
                            {
                                var item = AddElement(def, type, CurrentYPosition, CurrentXPosition);

                                CurrentYPosition += item.Height.Value;

                                mainSection.Items.Add(item);

                            }

                        }
                    }
                    else
                    {
                        mainSection.Height = new Unit(CurrentYPosition + 0.6, UnitType.Cm);

                        report.Items.Add(mainSection);

                        SaveReport(report, FORM_Definition_ID, reportsCount, false);

                        reportsCount++;

                        report = new Report();

                        reportPackager = new ReportPackager();

                        using (var targetStream = System.IO.File.OpenRead(reportPath))
                        {
                            report = reportPackager.Unpackage(targetStream);
                        }

                        sectionElement = report.Items.Find("dynamicSection", true)[0];

                        if (sectionElement != null)
                        {
                            mainSection = sectionElement as DetailSection;
                        }
                        else
                        {
                            mainSection = new DetailSection();
                        }

                        mainSection.CanShrink = true;
                        mainSection.KeepTogether = false;

                        CurrentYPosition = 0;
                        CurrentXPosition = 0;
                    }
                }

                mainSection.Height = new Unit(CurrentYPosition + 0.6, UnitType.Cm);

                report.Items.Add(mainSection);

                SaveReport(report, FORM_Definition_ID, reportsCount, false);
            }

            return reportsCount;
        }
        public async Task<string> ExecuteReport(Guid FORM_Definition_ID, Guid FORM_Application_ID)
        {
            var reportsCount = await CreateReportDefinition(FORM_Definition_ID);

            var reportBook = new ReportBook();
 
            for (int i = 1; i <= reportsCount; i++)
            {
                var report = new Report();
                report.PageNumberingStyle = PageNumberingStyle.Continue;

                var reportPackager = new ReportPackager();

                using (var targetStream = System.IO.File.OpenRead(BasePath + @"\" + FORM_Definition_ID + "_" + i + "_" + ".trdp"))
                {
                    report = reportPackager.Unpackage(targetStream);
                }

                DetailSection? mainSection;
                var mainElement = report.Items.Find("dynamicSection", true)[0];

                if (mainElement != null)
                {
                    mainSection = mainElement as DetailSection;
                }
                else
                {
                    mainSection = null;
                }

                if (report != null)
                {
                    report.PageNumberingStyle = PageNumberingStyle.Continue;

                    mainSection.PageBreak = PageBreak.After;

                    var Application = await _formApplication.GetApplication(FORM_Application_ID);
                    var dataToSet = await _formApplication.GetVApplicationFieldDataList(FORM_Application_ID);
                    var DefintionFields = await _formDefinition.GetDefinitionFieldList(FORM_Definition_ID);
                    DefintionFields = DefintionFields.Where(e => e.OnlyShowInFormRenderer != true).ToList();
                    

                    List<FORM_Definition_Field_Option> DefinitionOptionList = new List<FORM_Definition_Field_Option>();

                    foreach (var field in DefintionFields)
                    {
                        var result = await _formDefinition.GetDefinitionFieldOptionList(field.ID);

                        if (result != null)
                        {
                            DefinitionOptionList.AddRange(result);
                        }
                    }

                    var References = await _formDefinition.GetDefinitionFieldReferenceListByDefinition(FORM_Definition_ID);
                    var Types = _formDefinition.GetDefinitionFieldTypeList();

                    double CurrentYPosition = 0;

                    //SET REPETITION ELEMENTS IN REPORT
                    foreach (var repItems in dataToSet.Where(p => p.RepetitionParentID == FORM_Application_ID && p.RepetitionCount == 1).ToList())
                    {
                        var definition = DefintionFields.FirstOrDefault(p => p.ID == repItems.FORM_Definition_Field_ID);

                        if (definition != null)
                        {
                            var type = Types.FirstOrDefault(p => p.ID == definition.FORM_Definition_Fields_Type_ID);

                            if (type != null && type.ID == FORMElements.ColumnContainer)
                            {
                                var RepetitionItems = dataToSet.Where(p => p.RepetitionParentID == repItems.RepetitionParentID && p.FORM_Definition_Field_ID == repItems.FORM_Definition_Field_ID);

                                var containerElementList = report.Items.Find(repItems.FORM_Definition_Field_ID.ToString() + "__Container", true);

                                if (containerElementList != null && containerElementList.Count() > 0)
                                {
                                    var containerElement = containerElementList[0];

                                    if (containerElement != null)
                                    {
                                        var label = containerElement as Panel;

                                        if (mainSection != null)
                                        {
                                            mainSection.Items.Remove(label);
                                        }
                                    }

                                    foreach (var rep in RepetitionItems.OrderBy(p => p.RepetitionCount))
                                    {
                                        var panel = AddRepetitionElement(definition, DefintionFields.ToList(), Types, dataToSet, CurrentYPosition, 0, RepetitonID: rep.ID.ToString());

                                        if (mainSection != null)
                                        {
                                            mainSection.Items.Add(panel);

                                            CurrentYPosition = CurrentYPosition + panel.Height.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //SET REPORTDATA
                    foreach (var data in dataToSet.OrderBy(p => p.SortOrder))
                    {
                        var definition = DefintionFields.FirstOrDefault(p => p.ID == data.FORM_Definition_Field_ID);

                        if (definition != null && !(definition.OnlyShowInFormRenderer ?? false))
                        {
                            var defType = Types.FirstOrDefault(p => p.ID == definition.FORM_Definition_Fields_Type_ID);

                            if (defType != null && !defType.IsContainer)
                            {
                                ReportItemBase[]? foundElementList;
                                ReportItemBase[]? foundLabelElementList;

                                if (data.RepetitionParentID != null && data.RepetitionParentID != FORM_Application_ID)
                                {
                                    foundElementList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__" + data.RepetitionParentID, true);
                                }
                                else
                                {
                                    foundElementList = report.Items.Find(data.FORM_Definition_Field_ID.ToString(), true);
                                }

                                if (data.RepetitionParentID != null && data.RepetitionParentID != FORM_Application_ID)
                                {
                                    foundLabelElementList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__" + data.RepetitionParentID + "__label", true);
                                }
                                else
                                {
                                    foundLabelElementList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__label", true);
                                }

                                object? sectionElement = null;
                                object? sectionLabelElement = null;

                                if (foundElementList != null && foundElementList.Count() > 0)
                                {
                                    sectionElement = foundElementList[0];
                                }
                                if (foundLabelElementList != null && foundLabelElementList.Count() > 0)
                                {
                                    sectionLabelElement = foundLabelElementList[0];
                                }

                                if (sectionLabelElement != null)
                                {
                                    var label = sectionLabelElement as HtmlTextBox;

                                    if (label != null && definition.FORM_Definition_Field_Extended != null && definition.FORM_Definition_Field_Extended.Count > 0)
                                    {
                                        label.Value = WebUtility.HtmlEncode(definition.FORM_Definition_Field_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LangProvider.GetCurrentLanguageID()).Name);

                                        if (defType != null && defType.ID == FORMElements.Bullet) //BULLET
                                        {
                                            label.Value = "<ul><li>" + label.Value + "</li></ul>";
                                        }
                                    }
                                }

                                if (sectionElement != null)
                                {
                                    if (defType != null && defType.ID == FORMElements.Checkbox)  //CHECKBOX
                                    {
                                        var item = sectionElement as CheckBox;

                                        if (item != null)
                                        {
                                            item.Value = data.BoolValue;
                                        }
                                    }
                                    else if (defType != null && defType.ID == FORMElements.Difference)  //DIFFERENCE
                                    {
                                        var item = sectionElement as HtmlTextBox;

                                        if (item != null)
                                        {
                                            if (data.DecimalValue != null)
                                            {
                                                decimal? limitValue = 0;

                                                if (!string.IsNullOrEmpty(definition.ReferenceValueLimit))
                                                {
                                                    limitValue = decimal.Parse(definition.ReferenceValueLimit);
                                                }

                                                if (limitValue != null)
                                                {
                                                    if (limitValue != 0)
                                                    {
                                                        item.Value = data.DecimalValue.Value.ToString("C") + "/" + limitValue.Value.ToString("C");
                                                    }
                                                    else
                                                    {
                                                        item.Value = data.DecimalValue.Value.ToString("C");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (defType != null && defType.ID == FORMElements.Dropdown)  //DROPDOWN
                                    {
                                        var item = sectionElement as HtmlTextBox;

                                        if (item != null)
                                        {
                                            if (data.GuidValue != null)
                                            {
                                                var val = DefinitionOptionList.FirstOrDefault(p => p.ID == data.GuidValue);

                                                if (val != null)
                                                {
                                                    item.Value = val.Description;
                                                }
                                            }
                                        }
                                    }
                                    else if (defType != null && defType.ID == FORMElements.Radiobutton)  //RADIOBUTTON
                                    {
                                        var item = sectionElement as HtmlTextBox;

                                        if (item != null)
                                        {
                                            if (data.GuidValue != null)
                                            {
                                                var val = DefinitionOptionList.FirstOrDefault(p => p.ID == data.GuidValue);

                                                if (val != null)
                                                {
                                                    item.Value = val.Description;
                                                }
                                            }
                                        }
                                    }
                                    else if (defType != null && (defType.ID == FORMElements.CalculatingFields || defType.ID == FORMElements.Money))  //CALCULATING FIELD
                                    {
                                        var item = sectionElement as HtmlTextBox;

                                        if (item != null)
                                        {
                                            item.Value = data.Value;
                                        }

                                        if (item != null && (data.IsCurrency == true || defType.ID == FORMElements.Money))
                                        {
                                            var formattedString = "";
                                            if (!string.IsNullOrEmpty(data.Value))
                                            {
                                                formattedString = string.Format("{0:C2} €", data.Value);
                                            }
                                            item.Value = formattedString;
                                        }
                                    }
                                    else
                                    {
                                        var item = sectionElement as HtmlTextBox;

                                        if(item != null)
                                        {
                                            item.Value = data.Value;
                                        }

                                        if (item != null && !string.IsNullOrEmpty(item.Value))
                                        {
                                            var formattedString = "";
                                            if (!string.IsNullOrEmpty(data.Value))
                                            {
                                                formattedString = data.Value.Replace("\r\n", "<br />").Replace("&", "und")
                                                    .Replace("\n", "<br />").Replace("\f", "<br />")
                                                    .Replace("\t", "  ");
                                            }
                                            item.Value = formattedString;
                                        }
                                    }
                                }
                                else if (defType != null && defType.ID == FORMElements.List && data.ID != null)  //Sum List
                                {
                                    var subData = await _formApplication.GetApplicationFieldSubDataList(data.ID);

                                    ReportItemBase[]? headerDescriptionElementList;
                                    ReportItemBase[]? headerValueElementList;
                                    ReportItemBase[]? panelElementList;

                                    if (data.RepetitionParentID != null && data.RepetitionParentID != FORM_Application_ID)
                                    {
                                        headerDescriptionElementList = report.Items.Find(definition.ID.ToString() + "__" + data.RepetitionParentID + "_HEADER_DESCRIPTION", true);
                                    }
                                    else
                                    {
                                        headerDescriptionElementList = report.Items.Find(definition.ID.ToString() + "_HEADER_DESCRIPTION", true);
                                    }

                                    if (data.RepetitionParentID != null && data.RepetitionParentID != FORM_Application_ID)
                                    {
                                        headerValueElementList = report.Items.Find(definition.ID.ToString() + "__" + data.RepetitionParentID + "_HEADER_VALUE", true);
                                    }
                                    else
                                    {
                                        headerValueElementList = report.Items.Find(definition.ID.ToString() + "_HEADER_VALUE", true);
                                    }

                                    if (data.RepetitionParentID != null && data.RepetitionParentID != FORM_Application_ID)
                                    {
                                        panelElementList = report.Items.Find(definition.ID.ToString() + "__" + data.RepetitionParentID + "__panel", true);
                                    }
                                    else
                                    {
                                        panelElementList = report.Items.Find(definition.ID.ToString() + "__panel", true);
                                    }

                                    TextBox? headerDescriptionElement = null;
                                    TextBox? headerValueElement = null;
                                    Panel? panelElement = null;

                                    if (headerDescriptionElementList != null && headerDescriptionElementList.Count() > 0)
                                    {
                                        headerDescriptionElement = headerDescriptionElementList[0] as TextBox;
                                    }

                                    if (headerValueElementList != null && headerValueElementList.Count() > 0)
                                    {
                                        headerValueElement = headerValueElementList[0] as TextBox;
                                    }
                                    if (panelElementList != null && panelElementList.Count() > 0)
                                    {
                                        panelElement = panelElementList[0] as Panel;
                                    }

                                    if (headerDescriptionElement != null && headerValueElement != null && panelElement != null)
                                    {
                                        headerDescriptionElement.Value = TextProvider.Get("REPORT_TABLE_COLUMN_DESCRIPTION");
                                        headerValueElement.Value = TextProvider.Get("REPORT_TABLE_COLUMN_VALUE");

                                        var WidthUnit = headerDescriptionElement.Width + headerValueElement.Width;
                                        var HeightUnit = headerDescriptionElement.Height;
                                        var SeparatorUnit = new Unit(0.4, UnitType.Cm);

                                        int itemCount = 1;

                                        foreach (var subDataItem in subData)
                                        {
                                            string? value = "0,00 €";

                                            if (subDataItem != null)
                                            {
                                                if (subDataItem.Value != null)
                                                {
                                                    value = decimal.Parse(subDataItem.Value).ToString("C");
                                                }
                                            }

                                            var row = GetTableRow(WidthUnit, HeightUnit, subDataItem.ID, subDataItem.Description, value);
                                            row.Width = WidthUnit;
                                            row.Location = new PointU(new Unit(0, UnitType.Cm), (HeightUnit * itemCount));

                                            panelElement.Items.Add(row);

                                            itemCount++;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (definition != null)
                                {
                                    foreach (var f in References.Where(p => p.FORM_Definition_Field_ID == definition.ID && p.IsCalculatingFieldReference != true))
                                    {
                                        var applicationRefField = dataToSet.FirstOrDefault(p => p.FORM_Definition_Field_ID == f.FORM_Definition_Field_Source_ID);

                                        if (applicationRefField != null)
                                        {
                                            bool ValueOK = false;

                                            var sourcefield = DefintionFields.FirstOrDefault(p => p.ID == f.FORM_Definition_Field_Source_ID);

                                            if (sourcefield != null && sourcefield.FORM_Definition_Fields_Type_ID == FORMElements.Checkbox) 
                                            {
                                                if (f.Negate != true)
                                                {
                                                    if (f.TriggerValueBool != applicationRefField.BoolValue)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if (f.TriggerValueBool == applicationRefField.BoolValue)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                            }
                                            else if (sourcefield != null && sourcefield.FORM_Definition_Fields_Type_ID == FORMElements.Radiobutton)   //RADIOBUTTON
                                            {
                                                if (f.Negate != true)
                                                {
                                                    if (f.TriggerValue != applicationRefField.Value)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if (f.TriggerValue == applicationRefField.Value)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                            }
                                            else if (sourcefield != null && sourcefield.FORM_Definition_Fields_Type_ID == FORMElements.Dropdown)   //DROPDOWN
                                            {
                                                if (f.Negate != true)
                                                {
                                                    if (f.TriggerValue != applicationRefField.Value)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if (f.TriggerValue == applicationRefField.Value)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                            }
                                            else if (sourcefield != null && sourcefield.FORM_Definition_Fields_Type_ID == FORMElements.Number)   //Number
                                            {
                                                int value2 = 0;

                                                int.TryParse(applicationRefField.Value, out value2);
                                                if (f.Negate != true)
                                                {
                                                    if (f.TriggerValueInt < value2)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if (f.TriggerValueInt > value2)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                            }
                                            else if (sourcefield != null && sourcefield.FORM_Definition_Fields_Type_ID == FORMElements.CalculatingFields)   //CalculatedFields
                                            {
                                                decimal value2 = 0;

                                                decimal.TryParse(applicationRefField.Value, out value2);
                                                if (f.Negate != true)
                                                {
                                                    if (f.TriggerValueDecimal < value2)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if (f.TriggerValueDecimal > value2)
                                                    {
                                                        ValueOK = true;
                                                    }
                                                }
                                            }

                                            var RepetitionItems = dataToSet.Where(p => p.FORM_Definition_Field_ID == data.FORM_Definition_Field_ID);

                                            foreach (var repSubItem in RepetitionItems)
                                            {
                                                ReportItemBase[]? containerElementList;

                                                containerElementList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__" + repSubItem.ID + "__Container", true);

                                                if (containerElementList != null && containerElementList.Count() > 0)
                                                {
                                                    var containerElement = containerElementList[0];

                                                    if (containerElement != null)
                                                    {
                                                        var label = containerElement as Panel;

                                                        if (label != null)
                                                        {
                                                            label.Visible = !ValueOK;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    CurrentYPosition = 0;

                    //REORDER ITEMS ON REPORT
                    foreach (var data in dataToSet.Where(p => p.RepetitionParentID == FORM_Application_ID).OrderBy(p => p.SortOrder).ThenBy(p => p.RepetitionCount))
                    {
                        var definition = DefintionFields.FirstOrDefault(p => p.ID == data.FORM_Definition_Field_ID);

                        if (definition != null)
                        {
                            var defType = Types.FirstOrDefault(p => p.ID == definition.FORM_Definition_Fields_Type_ID);

                            ReportItemBase[]? CurrentPanelList;

                            if (definition.FORM_Definition_Fields_Type_ID != FORMElements.ColumnContainer)
                            {
                                CurrentPanelList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__panel", true);

                                if (CurrentPanelList == null || CurrentPanelList.Count() == 0)
                                {
                                    CurrentPanelList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__Container", true);
                                }
                            }
                            else
                            {
                                CurrentPanelList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__" + data.ID + "__panel", true);

                                if (CurrentPanelList == null || CurrentPanelList.Count() == 0)
                                {
                                    CurrentPanelList = report.Items.Find(data.FORM_Definition_Field_ID.ToString() + "__" + data.ID + "__Container", true);
                                }
                            }

                            if (CurrentPanelList != null && CurrentPanelList.Count() > 0)
                            {
                                var Panel = CurrentPanelList[0] as Panel;

                                if (Panel != null && data.ColumnPos == null)
                                {
                                    Panel.Location = new PointU(new Unit(Panel.Location.X.Value, UnitType.Cm), new Unit(CurrentYPosition, UnitType.Cm));

                                    if (defType != null && defType.ID == FORMElements.List)
                                    {
                                        CurrentYPosition = CurrentYPosition + Panel.Height.Value + 0.4;
                                    }
                                    else if (defType != null && defType.ID == FORMElements.SubTitle)
                                    {
                                        Panel.Location = new PointU(new Unit(Panel.Location.X.Value, UnitType.Cm), new Unit(CurrentYPosition + 0.4, UnitType.Cm));

                                        CurrentYPosition = CurrentYPosition + Panel.Height.Value + 0.4;
                                    }
                                    else
                                    {
                                        CurrentYPosition = CurrentYPosition + Panel.Height.Value;
                                    }
                                }
                            }
                        }
                    }

                    if (i == reportsCount)
                    {
                        var footerreport = new Report();
                        footerreport.PageNumberingStyle = PageNumberingStyle.Continue;

                        reportPackager = new ReportPackager();
                        string reportPath = BasePath + @"\FormTemplateFooter.trdp";
#if DEBUG
                        reportPath = BasePath + @"\FormTemplateFooter_Testserver.trdp";
#endif

                        using (var targetStream = System.IO.File.OpenRead(reportPath))
                        {
                            footerreport = reportPackager.Unpackage(targetStream);
                        }

                        var footer = footerreport.Items.Find("reportFooterSection1", true)[0];

                        mainSection.PageBreak = PageBreak.None;
                        report.Items.Add(footer);
                    }

                    reportBook.ReportSources.Add(report);
                }
            }

            var RelativePath = SaveReport(reportBook, FORM_Definition_ID, FORM_Application_ID);

            return RelativePath;
        }
        public bool SaveReport(Report report, Guid FORM_Definition_ID, bool CreateBackup = true)
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            var Path = BasePath + FORM_Definition_ID.ToString() + ".trdp";

            if (File.Exists(Path) && CreateBackup)
            {
                var DestinationPath = BasePath + FORM_Definition_ID.ToString() + "__BU_" + DateTime.Now.ToString("dd_MM_yyyy__HH_mm_ss") + ".trdp";

                File.Move(Path, DestinationPath);
            }

            var reportPackager = new ReportPackager();

            using (var targetStream = System.IO.File.Create(Path))
            {
                reportPackager.Package(report, targetStream);
            }

            return true;
        }
        private bool SaveReport(Report report, Guid FORM_Definition_ID, int ReportCount = 1, bool CreateBackup = true)
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            var Path = BasePath + FORM_Definition_ID.ToString() + "_" + ReportCount + "_" + ".trdp";

            if (File.Exists(Path) && CreateBackup)
            {
                var DestinationPath = BasePath + FORM_Definition_ID.ToString() + "_" + ReportCount + "__BU_" + DateTime.Now.ToString("dd_MM_yyyy__HH_mm_ss") + ".trdp";

                File.Move(Path, DestinationPath);
            }

            var reportPackager = new ReportPackager();

            using (var targetStream = System.IO.File.Create(Path))
            {
                reportPackager.Package(report, targetStream);
            }

            return true;
        }
        private string SaveReport(ReportBook report, Guid FORM_Definition_ID, Guid FORM_Application_ID)
        {
            if (!Directory.Exists(BasePath + "/ExecutedReports"))
            {
                Directory.CreateDirectory(BasePath + "/ExecutedReports");
            }

            var Path = BasePath + "/ExecutedReports/" + FORM_Definition_ID.ToString() + "_" + FORM_Application_ID.ToString() + ".trdp";
            var RelativePath = "ExecutedReports/" + FORM_Definition_ID.ToString() + "_" + FORM_Application_ID.ToString() + ".trdp";

            var reportPackager = new ReportPackager();

            using (var targetStream = System.IO.File.Create(Path))
            {
                reportPackager.Package(report, targetStream);
            }

            return RelativePath;
        }
        public Report? LoadReport(Guid FORM_Definition_ID)
        {
            var report = new Report();
            var reportPackager = new ReportPackager();
            var Path = BasePath + FORM_Definition_ID.ToString() + ".trdp";

            using (var targetStream = System.IO.File.OpenRead(Path))
            {
                report = reportPackager.Unpackage(targetStream);
            }

            return report;
        }
        public bool ReportExists(Guid FORM_Definition_ID)
        {
            bool exists = false;

            var Path = BasePath + FORM_Definition_ID.ToString() + ".trdp";

            if (File.Exists(Path))
            {
                exists = true;
            }

            return exists;
        }
        private Panel AddContainerElement(FORM_Definition_Field def, List<FORM_Definition_Field> Fields,
                                          List<FORM_Definition_Field_Type> Types,
                                          double CurrentYPosition = 0, double CurrentXPosition = 0,
                                          double Width = 17, string? RepetitonID = null)
        {
            var WidthUnit = new Unit(Width, UnitType.Cm);

            Panel panel = new Panel();
            panel.Location = new PointU(new Unit(CurrentXPosition, UnitType.Cm), new Unit(CurrentYPosition, UnitType.Cm));
            panel.CanShrink = true;
            panel.KeepTogether = false;
            panel.Width = WidthUnit;

            if (RepetitonID != null) 
            {
                panel.Name = def.ID.ToString() + "__" + RepetitonID + "__Container";
            }
            else
            {
                panel.Name = def.ID.ToString() + "__Container";
            }

            double localXPosition = 0;
            double computedHeight = 0;

            double Gap = 0.2;
            double columnCount = 1;

            if(def.ColumnCount != null)
            {
                columnCount = def.ColumnCount.Value;
            }

            double GapWidth = (Gap * (columnCount - 1));
            double ColumnWidth = (Width - GapWidth) / columnCount;

            if (columnCount > 0)
            {
                for (int i = 1; i <= columnCount; i++)
                {
                    double localYPosition = 0;
                    double localHeight = 0;

                    foreach (var field in Fields.Where(p => p.ColumnPos == i - 1 && p.FORM_Definition_Field_Parent_ID == def.ID).ToList().OrderBy(p => p.SortOrder))
                    {
                        var type = Types.FirstOrDefault(p => p.ID == field.FORM_Definition_Fields_Type_ID);

                        if (type != null && type.ID != FORMElements.PDFPager && type.ID != FORMElements.FileUpload) //REPORT PAGER
                        {
                            if (type != null)
                            {
                                if (type.IsContainer)
                                {
                                    var subPanel = AddContainerElement(field, Fields.Where(p => p.FORM_Definition_Field_Parent_ID == field.ID).ToList(), Types, localYPosition, localXPosition, ColumnWidth, RepetitonID);

                                    localYPosition += subPanel.Height.Value;

                                    localHeight = localYPosition;
                                    panel.Items.Add(subPanel);
                                }
                                else
                                {
                                    var subitem = AddElement(field, type, localYPosition, localXPosition, ColumnWidth, RepetitonID: RepetitonID);

                                    localYPosition += subitem.Height.Value;

                                    localHeight = localYPosition;

                                    panel.Items.Add(subitem);
                                }
                            }
                        }
                    }

                    if (localHeight > computedHeight)
                    {
                        computedHeight = localHeight;
                    }

                    localXPosition = (i * ColumnWidth) + (i * Gap);
                }
            }


            panel.Height = new Unit(computedHeight, UnitType.Cm);

            return panel;
        }
        private Panel AddRepetitionElement(FORM_Definition_Field def, List<FORM_Definition_Field> Fields, List<FORM_Definition_Field_Type> Types,
                                           List<V_FORM_Application_Field>? AppFields, double CurrentYPosition = 0, double CurrentXPosition = 0, double Width = 17, string? RepetitonID = null)
        {
            var WidthUnit = new Unit(Width, UnitType.Cm);

            Panel panel = new Panel();
            panel.Location = new PointU(new Unit(CurrentXPosition, UnitType.Cm), new Unit(CurrentYPosition, UnitType.Cm));
            panel.CanShrink = true;
            panel.KeepTogether = false;
            panel.Width = WidthUnit;

            if (RepetitonID != null)
            {
                panel.Name = def.ID.ToString() + "__" + RepetitonID + "__Container";
            }
            else
            {
                panel.Name = def.ID.ToString() + "__Container";
            }

            double localXPosition = 0;
            double computedHeight = 0;

            double Gap = 0.2;
            double columnCount = 1;

            if (def.ColumnCount != null)
            {
                columnCount = def.ColumnCount.Value;
            }

            double GapWidth = (Gap * (columnCount - 1));
            double ColumnWidth = (Width - GapWidth) / columnCount;

            if (columnCount > 0)
            {
                for (int i = 1; i <= columnCount; i++)
                {
                    double localYPosition = 0;
                    double localHeight = 0;

                    foreach (var field in Fields.Where(p => p.ColumnPos == i - 1 && p.FORM_Definition_Field_Parent_ID == def.ID).ToList().OrderBy(p => p.SortOrder))
                    {
                        var type = Types.FirstOrDefault(p => p.ID == field.FORM_Definition_Fields_Type_ID);

                        if (type != null && type.ID != FORMElements.PDFPager && type.ID != FORMElements.FileUpload) //REPORT PAGER
                        {
                            if (type != null)
                            {
                                if (type.IsContainer)
                                {
                                    if (AppFields != null)
                                    {
                                        var RepetitionItems = AppFields.Where(p => p.RepetitionParentID.ToString() == RepetitonID);

                                        foreach (var rep in RepetitionItems.OrderBy(p => p.RepetitionCount))
                                        {
                                            var subRepPanels = AddRepetitionElement(field, Fields.Where(p => p.FORM_Definition_Field_Parent_ID == field.ID).ToList(), Types, AppFields,
                                                                                    localYPosition, localXPosition, ColumnWidth, rep.ID.ToString());

                                            if (subRepPanels != null)
                                            {
                                                panel.Items.Add(subRepPanels);

                                                localYPosition += subRepPanels.Height.Value;
                                                localHeight = localYPosition;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var subitem = AddElement(field, type, localYPosition, localXPosition, ColumnWidth, RepetitonID: RepetitonID);

                                    localYPosition += subitem.Height.Value;

                                    localHeight = localYPosition;

                                    panel.Items.Add(subitem);
                                }
                            }
                        }
                    }

                    if (localHeight > computedHeight)
                    {
                        computedHeight = localHeight;
                    }

                    localXPosition = (i * ColumnWidth) + (i * Gap);
                }
            }

            panel.Height = new Unit(computedHeight, UnitType.Cm);            

            return panel;
        }
        private Panel AddElement(FORM_Definition_Field def, FORM_Definition_Field_Type? Type, double CurrentYPosition = 0,
                                 double CurrentXPosition = 0, double Width = 17, double Height = 0.5, double SeparatorHeight = 0.4, string? RepetitonID = null)
        {            
            var HeightUnit = new Unit(Height, UnitType.Cm);
            var WidthUnit = new Unit(Width, UnitType.Cm);
            var SeparatorUnit = new Unit(SeparatorHeight, UnitType.Cm);

            Panel panel = new Panel();
            // panel1
            panel.Location = new PointU(new Unit(CurrentXPosition, UnitType.Cm), new Unit(CurrentYPosition, UnitType.Cm));
            panel.CanShrink = true;
            panel.KeepTogether = true;
            panel.Width = WidthUnit;
            panel.Style.Font.Size = new Unit(14, UnitType.Pixel);

            if (RepetitonID != null) 
            {
                panel.Name = def.ID.ToString() + "__" + RepetitonID + "__panel";
            }
            else
            {
                panel.Name = def.ID + "__panel";
            }
            // label
            HtmlTextBox label = new HtmlTextBox();

            label.Left = new Unit(0, UnitType.Cm);
            label.Size = new SizeU(WidthUnit, HeightUnit);
            label.Style.Font.Size = new Unit(14, UnitType.Pixel);

            if (RepetitonID != null)
            {
                label.Name = def.ID.ToString() + "__" + RepetitonID + "__label";
            }
            else
            {
                label.Name = def.ID.ToString() + "__label";
            }

            label.CanGrow = true;
            label.KeepTogether = false;
            label.CanShrink = true;

            if (def.FORM_Definition_Field_Extended != null && def.FORM_Definition_Field_Extended.Count > 0)
            {
                label.Value = WebUtility.HtmlEncode(def.FORM_Definition_Field_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LangProvider.GetCurrentLanguageID()).Name);
            }

            if (Type != null && Type.ID == FORMElements.Title) //TITLE
            {
                label.Location = new PointU(new Unit(0, UnitType.Cm), new Unit(0.4, UnitType.Cm));

                panel.CanShrink = false;

                label.Style.Font.Size = new Unit(22, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";

                label.Height = label.Height + new Unit(SeparatorHeight * 2, UnitType.Cm);
            }
            else if (Type != null && Type.ID == FORMElements.SubTitle) //SUBTITLE
            {
                label.Location = new PointU(new Unit(0, UnitType.Cm), new Unit(0.4, UnitType.Cm));

                panel.CanShrink = false;

                label.Style.Font.Size = new Unit(18, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";

                label.Height = label.Height + new Unit(SeparatorHeight * 2, UnitType.Cm);
            }
            else if (Type != null && Type.ID == FORMElements.Spacer) //SPACER
            {
                label.CanGrow = false;
                label.CanShrink = false;
                label.Size = new SizeU(WidthUnit, new Unit(0.4, UnitType.Cm));
            }
            else if (Type != null && Type.ID == FORMElements.SmallSpacer) //SMALL SPACER
            {
                label.CanGrow = false;
                label.CanShrink = false;
                label.Size = new SizeU(WidthUnit, new Unit(0.2, UnitType.Cm));
            }
            else if (Type != null && Type.ID == FORMElements.Details) //DETAILS
            {
                label.Style.Font.Size = new Unit(10, UnitType.Pixel);
                label.Style.Font.Bold = false;
                label.Style.Font.Name = "Ubuntu Light";
            }
            else if (Type != null && Type.ID == FORMElements.Checkbox)   
            {
                label.Left = new Unit(0.7, UnitType.Cm);
                label.Top = new Unit(0.025, UnitType.Cm);
                label.Size = new SizeU(WidthUnit - new Unit(0.7, UnitType.Cm), HeightUnit + new Unit(SeparatorHeight, UnitType.Cm));
                label.CanGrow = true;
                label.Style.Font.Name = "Ubuntu Light";
            }
            else if (Type != null && Type.ID == FORMElements.Difference)  //DIFFERENCE
            {
                HtmlTextBox textBox = new HtmlTextBox();

                if (!string.IsNullOrEmpty(label.Value))
                {
                    textBox.Top = HeightUnit + new Unit(0.1, UnitType.Cm);
                }

                textBox.Size = new SizeU(WidthUnit, HeightUnit);
                textBox.Style.Font.Size = new Unit(14, UnitType.Pixel);

                if (RepetitonID != null)
                {
                    textBox.Name = def.ID.ToString() + "__" + RepetitonID;
                }
                else
                {
                    textBox.Name = def.ID.ToString();
                }

                textBox.CanGrow = true;
                textBox.Style.BorderStyle.Default = BorderType.Solid;
                textBox.Style.BorderColor.Default = Color.FromArgb(217, 217, 217);
                textBox.Style.BorderWidth.Default = new Unit(0, UnitType.Pixel);
                textBox.Style.BorderWidth.Bottom = new Unit(0.5, UnitType.Pixel);
                textBox.Style.VerticalAlign = VerticalAlign.Middle;
                textBox.Style.Font.Name = "Ubuntu Light";

                panel.Items.Add(textBox);

                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.Bullet) //BULLET
            {
                label.Value = "<ul><li>" + label.Value + "</li></ul>";
            }
            else if (Type != null && Type.ID == FORMElements.PDFPager) //PDF PAGER
            {
                panel.Visible = false;
            }
            else if (Type != null && Type.ID == FORMElements.Signature) 
            {
                panel.Visible = false;
            }
            else if (Type != null && Type.ID == FORMElements.FileUpload)
            {
                panel.Visible = false;
            }
            else if (Type != null && Type.ID == FORMElements.Textbox) 
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.Textarea) //TEXTAREA
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.Radiobutton) //RADIOBUTTONS
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if(Type != null && Type.ID == FORMElements.Dropdown)
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.Timepicker)
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.Datepicker)
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.Number)
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.CalculatingFields)
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.Money)
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else if (Type != null && Type.ID == FORMElements.List)
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Bold = true;
                label.Style.Font.Name = "Ubuntu";
            }
            else
            {
                label.Style.Font.Size = new Unit(14, UnitType.Pixel);
                label.Style.Font.Name = "Ubuntu Light";
            }

            if (!string.IsNullOrEmpty(label.Value))
            {
                label.Style.VerticalAlign = VerticalAlign.Middle;

                panel.Items.Add(label);
                panel.Height = label.Height + new Unit(SeparatorHeight, UnitType.Cm);
            }
            else
            {
                label.Height = new Unit(0, UnitType.Cm);
                label.Visible = false;
            }

            if (Type != null && Type.ReadOnly != true)
            {
                if (Type != null && Type.ID == FORMElements.Checkbox)  //CHECKBOX 
                {
                    CheckBox checkBox = new CheckBox();

                    var basePath = HostingEnv.WebRootPath;

                    var checkedImage = Image.FromFile(basePath + @"\Images\Reporting\checked.jpg");
                    var uncheckedImage = Image.FromFile(basePath + @"\Images\Reporting\unchecked.jpg");

                    checkBox.CheckedImage = checkedImage;
                    checkBox.UncheckedImage = uncheckedImage;
                    checkBox.IndeterminateImage = uncheckedImage;
                    checkBox.Style.Font.Name = "Ubuntu Light";

                    checkBox.Left = new Unit(0, UnitType.Cm);

                    if (RepetitonID != null)
                    {
                        checkBox.Name = def.ID.ToString() + "__" + RepetitonID;
                    }
                    else
                    {
                        checkBox.Name = def.ID.ToString();
                    }

                    checkBox.Size = new SizeU(new Unit(0.5, UnitType.Cm), new Unit(0.5, UnitType.Cm));
                    checkBox.Value = false;

                    panel.Items.Add(checkBox);
                }
                else if (Type != null && Type.ID == FORMElements.List)  //Sum List
                {
                    //HeaderRow
                    //Description
                    TextBox descriptionHeader = new TextBox();

                    descriptionHeader.Left = new Unit(0, UnitType.Cm);

                    if (!string.IsNullOrEmpty(label.Value))
                    {
                        descriptionHeader.Top = HeightUnit + SeparatorUnit;
                    }

                    var RowHeight = HeightUnit + SeparatorUnit;

                    descriptionHeader.Size = new SizeU(WidthUnit - new Unit(3, UnitType.Cm), RowHeight);

                    if (RepetitonID != null)
                    {
                        descriptionHeader.Name = def.ID.ToString() + "__" + RepetitonID + "_HEADER_DESCRIPTION";
                    }
                    else
                    {
                        descriptionHeader.Name = def.ID.ToString() + "_HEADER_DESCRIPTION";
                    }

                    descriptionHeader.Value = TextProvider.Get("REPORT_TABLE_COLUMN_DESCRIPTION");
                    descriptionHeader.Style.BorderStyle.Default = BorderType.Solid;
                    descriptionHeader.Style.BorderColor.Default = Color.FromArgb(217, 217, 217);
                    descriptionHeader.Style.BorderWidth.Default = new Unit(1, UnitType.Pixel);
                    descriptionHeader.Style.Font.Bold = true;
                    descriptionHeader.Style.Font.Name = "Ubuntu";
                    descriptionHeader.Style.VerticalAlign = VerticalAlign.Middle;
                    descriptionHeader.Style.TextAlign = HorizontalAlign.Center;

                    panel.Items.Add(descriptionHeader);

                    //Value
                    TextBox valueHeader = new TextBox();

                    valueHeader.Left = WidthUnit - new Unit(3, UnitType.Cm);
                    valueHeader.Top = descriptionHeader.Top;
                    valueHeader.Size = new SizeU(new Unit(3, UnitType.Cm), RowHeight);

                    if (RepetitonID != null)
                    {
                        valueHeader.Name = def.ID.ToString() + "__" + RepetitonID + "_HEADER_VALUE";
                    }
                    else
                    {
                        valueHeader.Name = def.ID.ToString() + "_HEADER_VALUE";
                    }

                    valueHeader.Value = TextProvider.Get("REPORT_TABLE_COLUMN_VALUE");
                    valueHeader.Style.BorderStyle.Default = BorderType.Solid;
                    valueHeader.Style.BorderColor.Default = Color.FromArgb(217, 217, 217);
                    valueHeader.Style.BorderWidth.Default = new Unit(1, UnitType.Pixel);
                    valueHeader.Style.Font.Bold = true;
                    valueHeader.Style.Font.Name = "Ubuntu";
                    valueHeader.Style.VerticalAlign = VerticalAlign.Middle;
                    valueHeader.Style.TextAlign = HorizontalAlign.Center;

                    panel.Items.Add(valueHeader);

                    var height = ((RowHeight + SeparatorUnit)) + new Unit(SeparatorHeight, UnitType.Cm);

                    if (!string.IsNullOrEmpty(label.Value))
                    {
                        height += label.Height + new Unit(0.1, UnitType.Cm);
                    }

                    panel.Height = height;
                }
                else
                {
                    HtmlTextBox textBox = new HtmlTextBox();

                    if (!string.IsNullOrEmpty(label.Value))
                    {
                        textBox.Top = HeightUnit + new Unit(0.1, UnitType.Cm);
                    }

                    textBox.Size = new SizeU(WidthUnit, HeightUnit);
                    textBox.Style.Font.Size = new Unit(14, UnitType.Pixel);

                    if (RepetitonID != null)
                    {
                        textBox.Name = def.ID.ToString() + "__" + RepetitonID;
                    }
                    else
                    {
                        textBox.Name = def.ID.ToString();
                    }

                    textBox.CanGrow = true;
                    textBox.Style.BorderStyle.Default = BorderType.Solid;
                    textBox.Style.BorderColor.Default = Color.FromArgb(217, 217, 217);
                    textBox.Style.BorderWidth.Default = new Unit(0, UnitType.Pixel);
                    textBox.Style.BorderWidth.Bottom = new Unit(0.5, UnitType.Pixel);
                    textBox.Style.VerticalAlign = VerticalAlign.Middle;
                    textBox.Style.Font.Name = "Ubuntu Light";

                    panel.Items.Add(textBox);

                    var height = textBox.Height + new Unit(SeparatorHeight, UnitType.Cm);

                    if (!string.IsNullOrEmpty(label.Value))
                    {
                        height += label.Height + new Unit(0.1, UnitType.Cm);
                    }

                    panel.Height = height;
                }
            }

            return panel;
        }
        private Panel GetTableRow(Unit WidthUnit, Unit HeightUnit, Guid RowID, string Description, string Value)
        {
            Panel panel = new Panel();

            //Description
            TextBox descriptionRow = new TextBox();

            descriptionRow.Left = new Unit(0, UnitType.Cm);
            descriptionRow.Size = new SizeU(WidthUnit - new Unit(3, UnitType.Cm), HeightUnit);
            descriptionRow.Name = "ROW_" + RowID.ToString() + "_DESCRIPTION";
            descriptionRow.Value = Description;
            descriptionRow.Style.BorderStyle.Default = BorderType.Solid;
            descriptionRow.Style.BorderColor.Default = Color.FromArgb(217, 217, 217);
            descriptionRow.Style.BorderWidth.Default = new Unit(1, UnitType.Pixel);
            descriptionRow.Style.VerticalAlign = VerticalAlign.Middle;
            descriptionRow.Style.TextAlign = HorizontalAlign.Center;
            descriptionRow.Style.Font.Name = "Ubuntu Light";

            panel.Items.Add(descriptionRow);

            //Value
            TextBox valueRow = new TextBox();

            valueRow.Left = WidthUnit - new Unit(3, UnitType.Cm);
            valueRow.Size = new SizeU(new Unit(3, UnitType.Cm), HeightUnit);
            valueRow.Name = "ROW_" + RowID.ToString() + "_VALUE";
            valueRow.Value = Value;
            valueRow.Style.BorderStyle.Default = BorderType.Solid;
            valueRow.Style.BorderColor.Default = Color.FromArgb(217, 217, 217);
            valueRow.Style.BorderWidth.Default = new Unit(1, UnitType.Pixel);
            valueRow.Style.VerticalAlign = VerticalAlign.Middle;
            valueRow.Style.TextAlign = HorizontalAlign.Center;
            valueRow.Style.Font.Name = "Ubuntu Light";

            panel.Items.Add(valueRow);

            panel.Height = HeightUnit;

            return panel;
        }
    }
}
