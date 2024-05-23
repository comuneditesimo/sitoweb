using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Telerik.Reporting;

namespace ICWebApp.Application.Helper
{
    public interface IROOM_ReportRendererHelper
    {
        Task<Report> CreateReportDefinition(Guid bookingID);
        Task<string> ExecuteReport(Guid reportID, Guid bookingID);
        bool SaveReport(Report report,  bool CreateBackup = true);
        string SaveReport(Report report, Guid bookingID);
        Report? LoadReport();
        bool ReportExists();
    }

    public class ROOM_ReportRendererHelper : IROOM_ReportRendererHelper
    {
        private string BasePath = @"D:\Comunix\Reports\";
        private string ReportFileName = "RoomBookingFormTemplate";

        private readonly ILANGProvider LangProvider;
        private readonly ITEXTProvider TextProvider;
        private readonly IHostingEnvironment HostingEnv;
        private readonly IRoomProvider RoomProvider;
        public ROOM_ReportRendererHelper(IRoomProvider roomprovider, ILANGProvider LangProvider,
                                         ITEXTProvider TextProvider, IHostingEnvironment HostingEnv, NavigationManager NavManager)
        {
            this.RoomProvider = roomprovider;
            this.LangProvider = LangProvider;
            this.TextProvider = TextProvider;
            this.HostingEnv = HostingEnv;

            if (NavManager.BaseUri.Contains("localhost"))
            {
                BasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\";
            }
        }

        public async Task<Report> CreateReportDefinition(Guid bookingID)
        {
            var report = new Report();
            var reportPackager = new ReportPackager();
            using (var targetStream = System.IO.File.OpenRead(BasePath + @"\"+ ReportFileName + ".trdp"))
            {
                report = reportPackager.Unpackage(targetStream);
            }

            Telerik.Reporting.ReportParameter reportParameter1 = report.ReportParameters.FirstOrDefault();
            if (reportParameter1 != null)
            {
                reportParameter1.Value = bookingID.ToString();
            }

            return report;
        }

        public async Task<string> ExecuteReport(Guid reportID, Guid bookingID)
        {
           

            var report = new Report();
            var reportPackager = new ReportPackager();
            

            using (var targetStream = System.IO.File.OpenRead(BasePath + @"\" + ReportFileName + ".trdp"))
            {
                report = reportPackager.Unpackage(targetStream);
            }

            Telerik.Reporting.ReportParameter reportParameter1 = report.ReportParameters.FirstOrDefault();
            if (reportParameter1 != null)
            {
                reportParameter1.Value = bookingID.ToString();
            }


            var RelativePath = SaveReport(report, bookingID);

            return RelativePath;
        }
        public bool SaveReport(Report report,  bool CreateBackup = true)
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            var Path = BasePath +  "RoomBookingFormTemplate.trdp";

            if (File.Exists(Path) && CreateBackup)
            {
                var DestinationPath = BasePath + ReportFileName + "__BU_" + DateTime.Now.ToString("dd_MM_yyyy__HH_mm_ss") + ".trdp";

                File.Move(Path, DestinationPath);
            }

            var reportPackager = new ReportPackager();

            using (var targetStream = System.IO.File.Create(Path))
            {
                reportPackager.Package(report, targetStream);
            }

            return true;
        }

        public string SaveReport(Report report, Guid bookingID)
        {
            if (!Directory.Exists(BasePath + "/ExecutedReports"))
            {
                Directory.CreateDirectory(BasePath + "/ExecutedReports");
            }

            var Path = BasePath + "/ExecutedReports/" + ReportFileName + "_" + bookingID.ToString() + ".trdp";
            var RelativePath = "ExecutedReports/" + ReportFileName + "_" + bookingID.ToString() + ".trdp";

            var reportPackager = new ReportPackager();

            using (var targetStream = System.IO.File.Create(Path))
            {
                reportPackager.Package(report, targetStream);
            }

            return RelativePath;
        }
        public Report? LoadReport()
        {
            var report = new Report();
            var reportPackager = new ReportPackager();
            var Path = BasePath + ReportFileName + ".trdp";

            using (var targetStream = System.IO.File.OpenRead(Path))
            {
                report = reportPackager.Unpackage(targetStream);
            }

            return report;
        }
        public bool ReportExists()
        {
            bool exists = false;

            var Path = BasePath + ReportFileName + ".trdp";

            if (File.Exists(Path))
            {
                exists = true;
            }

            return exists;
        }
    }
}
