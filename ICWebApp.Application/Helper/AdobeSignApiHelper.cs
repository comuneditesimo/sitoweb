using AdobeSign.Rest.Client;
using ICWebApp.Domain.Models.AdobeSign;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ICWebApp.Application.Helper
{
    public static class AdobeSignApiHelper
    {
        public static async Task<Token?> GetAccessToken(string redirect_uri, string code, string ClientId, string ClientSecret, string ApiAccessPoint)
        {
            RestClient restClient = new RestClient(ApiAccessPoint);
            if (code == null)
            {
                throw new ApiException(500, "Missing required 'code'.");
            }

            RestRequest restRequest = new RestRequest("/oauth/v2/token", Method.POST);
            restRequest.AddParameter("grant_type", "authorization_code");
            restRequest.AddParameter("client_id", ClientId);
            restRequest.AddParameter("client_secret", ClientSecret);
            restRequest.AddParameter("redirect_uri", redirect_uri);
            restRequest.AddParameter("code", code);
            var restResponse = await restClient.ExecuteAsync(restRequest);

            if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
            {
                if (restResponse.Content != null && restResponse.ContentType != null && restResponse.ContentType.StartsWith("application/json"))
                {
                    throw new ApiException((int)restResponse.StatusCode, "Error calling /oauth/v2/token: " + restResponse.Content);
                }

                throw new ApiException((int)restResponse.StatusCode, "Error calling /oauth/v2/token: " + restResponse.Content, restResponse.Content);
            }

            if (restResponse.StatusCode == (HttpStatusCode)0)
            {
                throw new ApiException((int)restResponse.StatusCode, "Error calling /oauth/v2/token: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
            }

            var t = JsonConvert.DeserializeObject<Token>(restResponse.Content);

            return t;
        }
        public static async Task<Token> RefreshAccessToken(string RefreshToken, string ClientId, string ClientSecret, string ApiAccessPoint)
        {
            RestClient restClient = new RestClient(ApiAccessPoint);

            if (RefreshToken == null)
                throw new ApiException(500, "Missing required 'RefreshToken'.");

            RestClient RefreshrestClient = new RestClient(ApiAccessPoint);

            RestRequest RefreshrestRequest = new RestRequest("/oauth/v2/refresh", Method.POST);

            RefreshrestRequest.AddParameter("grant_type", "refresh_token");
            RefreshrestRequest.AddParameter("client_id", ClientId);
            RefreshrestRequest.AddParameter("client_secret", ClientSecret);
            RefreshrestRequest.AddParameter("refresh_token", RefreshToken);

            var RefreshrestResponse = await RefreshrestClient.ExecuteAsync(RefreshrestRequest, Method.POST);

            if (((int)RefreshrestResponse.StatusCode) >= 400)
            {
                if (RefreshrestResponse.Content != null && RefreshrestResponse.ContentType.StartsWith("application/json"))
                {
                    throw new ApiException((int)RefreshrestResponse.StatusCode, "Error calling /oauth/v2/token: ");
                }
                throw new ApiException((int)RefreshrestResponse.StatusCode, "Error calling /oauth/v2/token: " + RefreshrestResponse.Content, RefreshrestResponse.Content);
            }
            else if (((int)RefreshrestResponse.StatusCode) == 0)
                throw new ApiException((int)RefreshrestResponse.StatusCode, "Error calling /oauth/v2/token: " + RefreshrestResponse.ErrorMessage, RefreshrestResponse.ErrorMessage);

            var t = JsonConvert.DeserializeObject<Token>(RefreshrestResponse.Content);

            return t;

        }
        public static async Task<bool> RevokeAccessToken(string Accesstoken, string ClientId, string ClientSecret, string ApiAccessPoint)
        {
            var authRestClient = new RestClient(ApiAccessPoint);

            if (Accesstoken == null)
                throw new ApiException(500, "Missing required 'AccessToken'.");

            var request = new RestRequest("/oauth/v2/revoke", Method.POST);
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", ClientId);
            request.AddParameter("client_secret", ClientSecret);
            request.AddParameter("token", Accesstoken);

            var response = await authRestClient.ExecuteAsync(request);

            if (((int)response.StatusCode) >= 400)
            {
                if (response.Content != null && response.ContentType.StartsWith("application/json"))
                {
                    return false;
                }
                return false;
            }
            else if (((int)response.StatusCode) == 0)
                return false;

            return true;
        }
        internal static object Deserialize(string content, Type type, IList<Parameter> headers = null)
        {
            if (type == typeof(Object)) // return an object
            {
                return content;
            }

            if (type == typeof(Stream))
            {
                var filePath = Path.GetTempPath();

                var fileName = filePath + Guid.NewGuid();
                if (headers != null)
                {
                    var regex = new Regex(@"Content-Disposition:.*filename=['""]?([^'""\s]+)['""]?$");
                    var match = regex.Match(headers.ToString());
                    if (match.Success)
                        fileName = filePath + match.Value.Replace("\"", "").Replace("'", "");
                }
                File.WriteAllText(fileName, content);
                return new FileStream(fileName, FileMode.Open);

            }

            if (type.Name.StartsWith("System.Nullable`1[[System.DateTime")) // return a datetime object
            {
                return DateTime.Parse(content, null, System.Globalization.DateTimeStyles.RoundtripKind);
            }

            if (type == typeof(String) || type.Name.StartsWith("System.Nullable")) // return primitive type
            {
                return ConvertType(content, type);
            }
            // at this point, it must be a model (json)
            try
            {
                return JsonConvert.DeserializeObject(content, type);
            }
            catch (IOException e)
            {
                throw new ApiException(500, e.Message);
            }
        }
        internal static Object ConvertType(Object fromObject, Type toObject)
        {
            return Convert.ChangeType(fromObject, toObject);
        }
    }
}
