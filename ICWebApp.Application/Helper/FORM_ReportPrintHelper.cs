using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;

namespace ICWebApp.Application.Helper
{
    public class FORM_ReportPrintHelper : IFORM_ReportPrintHelper
    {
        private string BasePath = @"D:\Comunix\Reports\";
        public FORM_ReportPrintHelper(NavigationManager NavManager)
        {
            if (NavManager.BaseUri.Contains("localhost"))
            {
                BasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\";
            }
        }
        public MemoryStream GetResponsePDF(string LanguageID, string MunicipalityID, string ApplicationID)
        {
            var reportPackager = new ReportPackager();
            var reportSource = new InstanceReportSource();
            string reportPath = BasePath + "ResponseTemplate.trdp";
#if DEBUG
            reportPath = BasePath + "ResponseTemplate_Testserver.trdp";
#endif
            using (var sourceStream = System.IO.File.OpenRead(reportPath))
            {
                var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);

                if (report.ReportParameters["LanguageID"] != null)
                {
                    report.ReportParameters["LanguageID"].Value = LanguageID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("LanguageID", ReportParameterType.String, LanguageID));
                }

                if (report.ReportParameters["MunicipalityID"] != null)
                {
                    report.ReportParameters["MunicipalityID"].Value = MunicipalityID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("MunicipalityID", ReportParameterType.String, MunicipalityID));
                }

                if (report.ReportParameters["ApplicationID"] != null)
                {
                    report.ReportParameters["ApplicationID"].Value = ApplicationID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("ApplicationID", ReportParameterType.String, ApplicationID));
                }

                reportSource.ReportDocument = report;

                var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
                var deviceInfo = new System.Collections.Hashtable();

                deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

                Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

                return new MemoryStream(result.DocumentBytes);
            }
        }
        public MemoryStream GetPDF(string ReportName, string LanguageID, string MunicipalityID, string ApplicationID, bool NoForm = false)
        {
            var reportBook = new ReportBook();

            var reportPackager = new ReportPackager();
            var reportSource = new InstanceReportSource();
            string reportPath = BasePath + @"\FormTemplate.trdp";
#if DEBUG
            reportPath = BasePath + @"\FormTemplate_Testserver.trdp";
#endif

            using (var sourceStream = System.IO.File.OpenRead(reportPath))
            {
                var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
                report.PageNumberingStyle = PageNumberingStyle.Continue;

                if (report.ReportParameters["LanguageID"] != null)
                {
                    report.ReportParameters["LanguageID"].Value = LanguageID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("LanguageID", ReportParameterType.String, LanguageID));
                }

                if (report.ReportParameters["MunicipalityID"] != null)
                {
                    report.ReportParameters["MunicipalityID"].Value = MunicipalityID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("MunicipalityID", ReportParameterType.String, MunicipalityID));
                }

                if (report.ReportParameters["ApplicationID"] != null)
                {
                    report.ReportParameters["ApplicationID"].Value = ApplicationID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("ApplicationID", ReportParameterType.String, ApplicationID));
                }

                reportBook.ReportSources.Add(report);
            }

            using (var sourceStream = System.IO.File.OpenRead(BasePath + ReportName))
            {
                var reportbook = (Telerik.Reporting.ReportBook)reportPackager.UnpackageDocument(sourceStream);

                foreach (var report in reportbook.ReportSources)
                {
                    if (report.Parameters["LanguageID"] != null)
                    {
                        report.Parameters["LanguageID"].Value = LanguageID;
                    }
                    else
                    {
                        report.Parameters.Add(new Parameter("LanguageID", LanguageID));
                    }

                    if (report.Parameters["MunicipalityID"] != null)
                    {
                        report.Parameters["MunicipalityID"].Value = MunicipalityID;
                    }
                    else
                    {
                        report.Parameters.Add(new Parameter("MunicipalityID", MunicipalityID));
                    }

                    if (report.Parameters["ApplicationID"] != null)
                    {
                        report.Parameters["ApplicationID"].Value = ApplicationID;
                    }
                    else
                    {
                        report.Parameters.Add(new Parameter("ApplicationID", ApplicationID));
                    }
                }

                reportBook.ReportSources.Add(reportbook);              
            }

//            reportPath = BasePath + @"\FormTemplateFooter.trdp";
//#if DEBUG
//            reportPath = BasePath + @"\FormTemplateFooter_Testserver.trdp";
//#endif
            //using (var sourceStream = System.IO.File.OpenRead(BasePath + "FormTemplateFooter.trdp"))
            //{
            //    var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
            //    report.PageNumberingStyle = PageNumberingStyle.Continue;

            //    if (report.ReportParameters["LanguageID"] != null)
            //    {
            //        report.ReportParameters["LanguageID"].Value = LanguageID;
            //    }
            //    else
            //    {
            //        report.ReportParameters.Add(new ReportParameter("LanguageID", ReportParameterType.String, LanguageID));
            //    }

            //    if (report.ReportParameters["MunicipalityID"] != null)
            //    {
            //        report.ReportParameters["MunicipalityID"].Value = MunicipalityID;
            //    }
            //    else
            //    {
            //        report.ReportParameters.Add(new ReportParameter("MunicipalityID", ReportParameterType.String, MunicipalityID));
            //    }

            //    if (report.ReportParameters["ApplicationID"] != null)
            //    {
            //        report.ReportParameters["ApplicationID"].Value = ApplicationID;
            //    }
            //    else
            //    {
            //        report.ReportParameters.Add(new ReportParameter("ApplicationID", ReportParameterType.String, ApplicationID));
            //    }


            //    reportBook.ReportSources.Add(report);
            //}

            reportSource.ReportDocument = reportBook;

            var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
            var deviceInfo = new System.Collections.Hashtable();

            deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

            Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

            return new MemoryStream(result.DocumentBytes);
        }
        public void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }
    }
}
