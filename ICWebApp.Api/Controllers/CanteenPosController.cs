using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Canteen.POS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.Json;

namespace ICWebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CanteenPosController : ControllerBase
    {
        private ICANTEENProvider _CanteenProvider;
        private IMailerService _MailerService;
        private IUnitOfWork _unitOfWork;
        public CanteenPosController(ICANTEENProvider _CanteenProvider, IMailerService _MailerService, IUnitOfWork _unitOfWork)
        {
            this._CanteenProvider = _CanteenProvider;
            this._MailerService = _MailerService;
            this._unitOfWork = _unitOfWork;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(String UserName, String Password)
        {
            var response = await _CanteenProvider.CreatePOSSession(UserName, Password);

            return Content(JsonSerializer.Serialize(response));
        }
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout(String SessionToken)
        {
            var response = new LogoutResponse();

            if (String.IsNullOrEmpty(SessionToken))
            {
                response.Code = "101";
                response.Response = "No Token";

                return Content(JsonSerializer.Serialize(response));
            }

            await _CanteenProvider.ExpireSession(Guid.Parse(SessionToken));

            response.Code = "200";
            response.Response = "Session Logout Ok";

            return Content(JsonSerializer.Serialize(response));
        }
        [HttpGet("CheckSessionToken")]
        public async Task<IActionResult> CheckSessionToken(String SessionToken)
        {
            var response = new LoginResponse();

            if (String.IsNullOrEmpty(SessionToken))
            {
                response.Code = "101";
                response.Response = "No Token";

                return Content(JsonSerializer.Serialize(response));
            }

            var CheckSession = await _CanteenProvider.GetPOSSession(Guid.Parse(SessionToken));

            if (CheckSession == null || CheckSession.AUTH_Municipality_ID == null)
            {
                response.Code = "100";
                response.Response = "Session Expired";

                return Content(JsonSerializer.Serialize(response));
            }

            CheckSession.ExpirationDate = DateTime.Now.AddMonths(1);

            await _CanteenProvider.UpdateSession(CheckSession);

            response.Code = "200";
            response.Response = "Session Ok";

            if (CheckSession.SessionToken != null)
            {
                response.SessionToken = CheckSession.SessionToken.ToString();
            }

            return Content(JsonSerializer.Serialize(response));
        }
        [HttpGet("CheckCard")]
        public async Task<IActionResult> CheckCard(Guid SessionToken, String CardID, bool Manual = false)
        {
            if (!string.IsNullOrEmpty(CardID) && CardID.Length > 8)
            {
                CardID = CardID.Substring(0, 8);
            }

            var CheckSession = await _CanteenProvider.GetPOSSession(SessionToken);

            if(CheckSession == null || CheckSession.AUTH_Municipality_ID == null)
            {
                var response = new CardResponse();

                response.Code = "100";
                response.Response = "Session Expired";

                return Content(JsonSerializer.Serialize(response));
            }

            CheckSession.ExpirationDate = DateTime.Now.AddMonths(1);

            await _CanteenProvider.UpdateSession(CheckSession);

            var checkCard = await _CanteenProvider.CheckCard(CardID, CheckSession.AUTH_Municipality_ID.Value, Manual);

            if(checkCard.Code == "201" && checkCard.AUTH_Users_ID != null && checkCard.Card != null)
            {
                var userLanguage = LanguageSettings.German;

                var userSettings = await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(p => p.AUTH_UsersID == checkCard.AUTH_Users_ID.Value);

                if(userSettings != null && userSettings.LANG_Languages_ID != null)
                {
                    userLanguage = userSettings.LANG_Languages_ID.Value;
                }

                var user = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == checkCard.AUTH_Users_ID.Value);

                if (user != null)
                {
                    var mail = new MSG_Mailer();

                    mail.ID = Guid.NewGuid();
                    mail.ToAdress = user.Email;

                    mail.Subject = (await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(e => e.Code == "CANTEEN_POS_USED_WITH_EXCEPTION_MAIL_SUBJECT" && e.LANG_LanguagesID == userLanguage))?.Text;
                    mail.Body = (await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(e => e.Code == "CANTEEN_POS_USED_WITH_EXCEPTION_MAIL_BODY" && e.LANG_LanguagesID == userLanguage))?.Text;

                    if (mail.Body != null)
                    {
                        mail.Body = mail.Body.Replace("{Firstname}", checkCard.Card.Firstname);
                        mail.Body = mail.Body.Replace("{Lastname}", checkCard.Card.Lastname);
                        mail.Body = mail.Body.Replace("{Date}", DateTime.Now.ToString("dd.MM.yyyy"));

                        await _MailerService.SendMail(mail, null, CheckSession.AUTH_Municipality_ID.Value);
                    }
                }
            }

            return Content(JsonSerializer.Serialize(checkCard));
        }
        [HttpGet("TestCard")]
        public async Task<IActionResult> TestCard(Guid SessionToken, String CardID)
        {
            if (!string.IsNullOrEmpty(CardID) && CardID.Length > 8){
                CardID = CardID.Substring(0, 8);
            }

            var CheckSession = await _CanteenProvider.GetPOSSession(SessionToken);

            if (CheckSession == null || CheckSession.AUTH_Municipality_ID == null)
            {
                var response = new TestResponse();

                response.Response = "Session Expired";

                return Content(JsonSerializer.Serialize(response));
            }

            CheckSession.ExpirationDate = DateTime.Now.AddMonths(1);

            await _CanteenProvider.UpdateSession(CheckSession);

            var card = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.CardID == CardID);

            if(card != null)
            {
                var response = new TestResponse();

                response.Response = card.Firstname + " " + card.Lastname + "\n" + card.Taxnumber + "\n" + card.CardID;

                return Content(JsonSerializer.Serialize(response));
            }

            var finalresponse = new TestResponse();

            finalresponse.Response = "No User Found!";

            return Content(JsonSerializer.Serialize(finalresponse));
        }
        [HttpGet("GetChildren")]
        public async Task<IActionResult> GetChildren(Guid SessionToken, String Search)
        {

            var CheckSession = await _CanteenProvider.GetPOSSession(SessionToken);

            if (CheckSession == null || CheckSession.AUTH_Municipality_ID == null)
            {
                var response = new SearchResponse();

                response.Code = "510";

                response.Results = new List<SearchResponseItem>();

                return Content(JsonSerializer.Serialize(response));
            }

            CheckSession.ExpirationDate = DateTime.Now.AddMonths(1);

            await _CanteenProvider.UpdateSession(CheckSession);

            Search = Search.ToLower();

            var cardList = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().Where(p => (p.AUTH_Municipality_ID == CheckSession.AUTH_Municipality_ID) && ( p.Firstname.ToLower().Contains(Search) || p.Lastname.ToLower().Contains(Search) ||
                                                                                                        (p.Firstname + " " + p.Lastname).ToLower().Contains(Search) ||
                                                                                                        (p.Lastname + " " + p.Firstname).ToLower().Contains(Search))).Take(5).ToListAsync();

            if (cardList != null && cardList.Count() > 0)
            {
                var response = new SearchResponse();

                response.Results = new List<SearchResponseItem>();

                foreach (var card in cardList)
                {
                    var subscriberCard = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.CardID == card.CardID && p.AUTH_Municipality_ID == CheckSession.AUTH_Municipality_ID && p.CANTEEN_Subscriber_Card_Status_ID == CanteenCardStatus.Finished);

                    if (subscriberCard == null)
                    {
                        continue;
                    }

                    var subscriber = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.TaxNumber == subscriberCard.Taxnumber && p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted);

                    if (subscriber == null)
                    {
                        continue;
                    }

                    var datetoCheck = DateTime.Now;

                    var checkMovementState = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefaultAsync(p => p.CANTEEN_Subscriber_ID == subscriber.ID &&
                                                                                                                                   p.Date != null && p.RemovedDate == null &&
                                                                                                                                   p.Date.Value.Year == datetoCheck.Year &&
                                                                                                                                   p.Date.Value.Month == datetoCheck.Month &&
                                                                                                                                   p.Date.Value.Day == datetoCheck.Day &&
                                                                                                                                   p.CANTEEN_Subscriber_Movement_Type_ID == CanteenMovementType.Meal);

                    if (checkMovementState == null)
                    {
                        continue;
                    }

                    var sr = new SearchResponseItem();

                    if (card.DateOfBirth != null)
                    {
                        sr.BirthDate = card.DateOfBirth.Value.ToString("dd.MM.yyyy");
                    }
                    else
                    {
                        sr.BirthDate = DateTime.Now.ToString("dd.MM.yyyy");
                    }

                    sr.Name = card.Firstname + " " + card.Lastname;

                    sr.CardID = card.CardID;

                    response.Results.Add(sr);
                }
                
                response.Code = "200";

                return Content(JsonSerializer.Serialize(response));
            }

            var finalresponse = new SearchResponse();

            finalresponse.Code = "502";
            finalresponse.Results = new List<SearchResponseItem>();

            return Content(JsonSerializer.Serialize(finalresponse));
        }
    }
}