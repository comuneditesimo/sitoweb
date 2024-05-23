using System.Net;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Mvc;

namespace ICWebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CanteenOpenDaysController : ControllerBase
    {
        private readonly ILogger<CanteenOpenDaysController> _logger;
        private readonly ICANTEENProvider CanteenProvider;
        private List<CANTEEN_Subscriber_Movements> NextMovements = new List<CANTEEN_Subscriber_Movements>();
        private List<CANTEEN_Subscriber> Subscribers = new List<CANTEEN_Subscriber>();

        public CanteenOpenDaysController(ILogger<CanteenOpenDaysController> logger, ICANTEENProvider canteenProvider)
        {
            _logger = logger;
            this.CanteenProvider = canteenProvider;
        }

        [HttpGet(Name = "GetOpenDays")]
        public string GetOpenDays(string parentpin = "00000000", string childcode = "0000",
            string operation = "GetDate", string language = "DE")
        {
            if ((parentpin.Length == 9 && parentpin.Substring(8) != "#") || (childcode.Length == 5 &&
                                                                             childcode.Substring(4) != "#"))
            {
                Response.StatusCode = 400;
                return "Error";
            }

            if (parentpin.Length == 9)
                parentpin = parentpin.Substring(0, 8);
            if (childcode.Length == 5)
                childcode = childcode.Substring(0, 4);

            if (parentpin.Length != 8 || !parentpin.All(char.IsDigit) || childcode.Length != 4 ||
                !childcode.All(char.IsDigit))
            {
                Response.StatusCode = 400;
                return "Error";
            }


            if (operation == "GetDate")
            {
                var format = language == "DE" ? " Taste {0}! " : " tasto {0}. ";
                string resultString = "";
                string? response = "";
                response = CanteenProvider.GetApiOperationsByPhoneCode(parentpin, childcode, "GetDate1");
                if (response == null)
                {
                    Response.StatusCode = 400;
                    return "Error";
                }

                resultString = " ";
                AddResponseToString(ref resultString, response, language);
                resultString = resultString + String.Format(format, 1);


                response = CanteenProvider.GetApiOperationsByPhoneCode(parentpin, childcode, "GetDate2");
                if (response != null)
                {
                    resultString = resultString + " ";
                    AddResponseToString(ref resultString, response, language);
                    resultString = resultString + String.Format(format, 2);
                }


                response = CanteenProvider.GetApiOperationsByPhoneCode(parentpin, childcode, "GetDate3");
                if (response != null)
                {
                    resultString = resultString + " ";
                    AddResponseToString(ref resultString, response, language);
                    resultString = resultString + String.Format(format, 3);
                }

                return "<dateresult><datestring>" + resultString + "</datestring></dateresult>";
            }


            Response.StatusCode = 400;
            return "Error";
        }

        private static void AddResponseToString(ref string resultString, string response, string language)
        {
            resultString += language == "DE" ? response : response.Replace(".", "/");
        }
    }
}