using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IFORM_ReportRendererHelper
    {
        public Task<int> CreateReportDefinition(Guid FORM_Definition_ID);
        public Task<string> ExecuteReport(Guid FORM_Definition_ID, Guid FORM_Application_ID);
        public bool SaveReport(Report report, Guid FORM_Definition_ID, bool CreateBackup = true);
        public Report? LoadReport(Guid FORM_Definition_ID);
        public bool ReportExists(Guid FORM_Definition_ID);
    }
}
