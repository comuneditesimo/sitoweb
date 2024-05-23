using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ICWebApp.Application.Helper
{
    public class CalendarHelper : ICalendarHelper
    {

        public string GetFilePath(Guid ID, DateTime DateStart, DateTime DateEnd, string Subject, string? Description = null, string? Location = null)
        {
            string Filename = ID.ToString().Replace("\"", "").Replace("/", "").Replace(":", "") + "_" + DateStart.ToString("ddMMyyyyHHmm") + "_" + DateEnd.ToString("ddMMyyyyHHmm") + ".ics";
            string nameDocument = HttpUtility.UrlEncode(Filename);

            if (System.IO.File.Exists("D:/Comunix/Calendar/" + nameDocument))
            {
                return "Calendar/" + nameDocument;
            }

            StringBuilder sb = new StringBuilder();

            //start the calendar item
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");

            //add the event
            sb.AppendLine("BEGIN:VEVENT");

            sb.AppendLine("SUMMARY:" + Subject);
            sb.AppendLine("DTSTART:" + DateStart.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("DTEND:" + DateEnd.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("DTSTAMP:" + DateTime.Now.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("UID:" + (Guid.NewGuid().ToString() + DateTime.Now.ToString("yyyyMMddTHHmm00")));

            if (!string.IsNullOrEmpty(Description))
            {
                sb.AppendLine("DESCRIPTION:" + Description.Replace("<p>", "").Replace("</p>", "") + "");
            }

            if (!string.IsNullOrEmpty(Location))
            {
                sb.AppendLine("LOCATION:" + Location);
            }

            sb.AppendLine("PRIORITY:0");
            sb.AppendLine("END:VEVENT");


            //end calendar item
            sb.AppendLine("END:VCALENDAR");

            System.IO.File.WriteAllText("D:/Comunix/Calendar/" + nameDocument, sb.ToString());

            return "Calendar/" + nameDocument;
        }
    }
}
