using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IFORM_ReportPrintHelper
    {
        public MemoryStream GetResponsePDF(string LanguageID, string MunicipalityID, string ApplicationID);
        public MemoryStream GetPDF(string ReportName, string LanguageID, string MunicipalityID, string ApplicationID, bool NoForm = false);
        public void CopyPages(PdfDocument from, PdfDocument to);
    }
}
