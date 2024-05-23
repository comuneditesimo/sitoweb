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
    public class CanteenGetSetStatusController : ControllerBase
    {        
        private readonly ILogger<CanteenGetSetStatusController> _logger;
        private readonly ICANTEENProvider CanteenProvider;
        private List<CANTEEN_Subscriber_Movements> NextMovements = new List<CANTEEN_Subscriber_Movements>();
        private List<CANTEEN_Subscriber> Subscribers = new List<CANTEEN_Subscriber>();

        public CanteenGetSetStatusController(ILogger<CanteenGetSetStatusController> logger, ICANTEENProvider canteenProvider)
        {
            _logger = logger;
            this.CanteenProvider = canteenProvider;
        }



        [HttpGet(Name = "GetSetGetStatus")]
        public string GetSetGetStatus(string parentpin = "00000000", string childcode = "0000", string operation = "GetStatus1")
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

            string resultString = "";
            string? response = "";

            //GetStatus1
            //SetStatus1

            response = CanteenProvider.GetApiOperationsByPhoneCode(parentpin, childcode, operation);
            if (response == null)
            {
                Response.StatusCode = 400;
                return "Error"; 
            }
            resultString = response;

            return "<status>" + resultString + "</status>";
        }

        [HttpGet("~/ParentCodeExists")]
        public string ParentCodeExists(string key, string parentpin = "00000000")
        {
            if (_key != key)
            {
                Response.StatusCode = 401;
                return "Unauthorized";
            }
            
            if(parentpin.Length == 9 && parentpin.Substring(8) != "#")
            {
                Response.StatusCode = 400;
                return "Error";
            }
            if (parentpin.Length == 9)
                parentpin = parentpin.Substring(0, 8);
            
            if (!parentpin.All(char.IsDigit) || parentpin.Length != 8)
            {
                Response.StatusCode = 400;
                return "Error";
            }
            
            bool exists = CanteenProvider.ParentCodeExists(parentpin);
            
            if (exists)
            {
                return "Ok";
            }

            Response.StatusCode = 400;
            return "Pin not found";
        }
        
        [HttpGet("~/ParentAndChildCodeExist")]
        public string ParentAndChildCodeExist(string key, string parentpin = "00000000", string childcode = "0000")
        {
            if (_key != key)
            {
                Response.StatusCode = 401;
                return "Unauthorized";
            }
            
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
            
            
            bool exists = CanteenProvider.ParentAndChildCodesExist(parentpin, childcode);
            
            if (exists)
            {
                return "Ok";
            }

            Response.StatusCode = 400;
            return "Pin combination not found";
        }
        
        [HttpGet("~/Monitoring")]
        public string Monitoring()
        {
            return "Ok";
        }

    }
}