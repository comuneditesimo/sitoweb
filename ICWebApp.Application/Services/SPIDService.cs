using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.PagoPA.Classes;
using ICWebApp.DataStore.PagoPA.Domain;
using ICWebApp.DataStore.PagoPA.Interface;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.Spid;
using ICWebApp.Application.Provider;
using System.Text.Json;
using System.Net.Http.Headers;
using RestSharp;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace ICWebApp.Application.Services
{
    public class SPIDService : ISPIDService
    {
        private ICONFProvider ConfProvider;
        private IAUTHProvider AuthProvider;

        public SPIDService(ICONFProvider ConfProvider, IAUTHProvider AuthProvider)
        {
            this.ConfProvider = ConfProvider;
            this.AuthProvider = AuthProvider;
        }

        public async Task<string?> AuthenticateSpid(Guid Auth_Municipality_ID, string ReturnBaseUrl)
        {
            var config = await ConfProvider.GetSpidConfiguration(Auth_Municipality_ID);

            try
            {
                if (config != null)
                {
                    var body = new ICWebApp.Domain.Models.Spid.RequestBody();

                    body.applicationId = config.ApplicationID;
                    body.type = "spid";
                    body.returnURL = ReturnBaseUrl + "Spid/Return";
                    body.returnURLError = ReturnBaseUrl + "Spid/SpidError";
                    body.attrSet = "full";
                    body.serviceName = config.ServiceName;

                    if (config.AutoRedirectOnError != null)
                    {
                        body.autoRedirectOnError = config.AutoRedirectOnError.Value;
                    }
                    else
                    {
                        body.autoRedirectOnError = false;
                    }

                    string json = JsonSerializer.Serialize(body);

                    SetLog(Auth_Municipality_ID, "Spid - Start Token", json);

                    RestClient Client = new RestClient(config.BaseUrlApi);

                    string Address = "/api/v3/Auth/Token";

                    RestRequest Request = new RestRequest(Address, Method.POST);

                    Request.AddHeader("Content-Type", "application/json; charset=utf-8");

                    if (config.Secret != null)
                    {
                        Request.AddHeader("Authorization", @"spidSecret " + config.Secret);
                    }
                    else
                    {
                        Request.AddHeader("Authorization", "spidSecret ");
                    }

                    Request.AddJsonBody(json);

                    var Commitresponse = await Client.ExecuteAsync(Request, Method.POST);

                    SetLog(Auth_Municipality_ID, "Spid - Token Response", Commitresponse.Content);

                    if (Commitresponse.StatusCode == HttpStatusCode.OK)
                    {
                        if (Commitresponse.Content != null && Commitresponse.Content.Contains("\"success\":true"))
                        {
                            dynamic result = JsonConvert.DeserializeObject(Commitresponse.Content);

                            var token = result.data;

                            if (token != null)
                            {
                                string url = config.BaseUrlApi + "Profile?SpidToken=" + token;

                                SetLog(Auth_Municipality_ID, "Spid - Url", url);

                                return url;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                SetLog(Auth_Municipality_ID, "Spid - Error", ex.Message);
            }

            return null;
        }
        public async Task<ProfileData?> GetUserData(Guid Auth_Municipality_ID, string Token)
        {
            var config = await ConfProvider.GetSpidConfiguration(Auth_Municipality_ID);

            try
            {
                if (config != null)
                {
                    var body = new ICWebApp.Domain.Models.Spid.RequestBodyProfile();

                    body.token = Token;

                    string json = JsonSerializer.Serialize(body);

                    SetLog(Auth_Municipality_ID, "Spid - Get Profile", json);

                    RestClient Client = new RestClient(config.BaseUrlApi);

                    string Address = "api/v3/Auth/getProfile";

                    RestRequest Request = new RestRequest(Address, Method.POST);

                    Request.AddHeader("Content-Type", "application/json; charset=utf-8");

                    if (config.Secret != null)
                    {
                        Request.AddHeader("Authorization", @"spidSecret " + config.Secret);
                    }
                    else
                    {
                        Request.AddHeader("Authorization", "spidSecret ");
                    }

                    Request.AddJsonBody(json);

                    var Commitresponse = await Client.ExecuteAsync(Request, Method.POST);

                    SetLog(Auth_Municipality_ID, "Spid - Get Profile Response", Commitresponse.Content);

                    if (Commitresponse.StatusCode == HttpStatusCode.OK)
                    {
                        if (Commitresponse.Content != null && Commitresponse.Content.Contains("\"success\":true"))
                        {
                            dynamic data = JsonConvert.DeserializeObject(Commitresponse.Content);

                            if (data != null)
                            {
                                ProfileData result = new ProfileData();

                                result.sessionId = data.data.sessionId;
                                result.spidCode = data.data.spidCode;
                                result.name = data.data.name;
                                result.familyName = data.data.familyName;
                                result.placeOfBirth = data.data.placeOfBirth;
                                result.countyOfBirth = data.data.countyOfBirth;
                                result.dateOfBirth = data.data.dateOfBirth;
                                result.gender = data.data.gender;
                                result.companyName = data.data.companyName;
                                result.registeredOffice = data.data.registeredOffice;
                                result.fiscalNumber = data.data.fiscalNumber;
                                result.ivaCode = data.data.ivaCode;
                                result.idCard = data.data.idCard;
                                result.mobilePhone = data.data.mobilePhone;
                                result.email = data.data.email;
                                result.domicileStreetAddress = data.data.domicileStreetAddress;
                                result.domicilePostalCode = data.data.domicilePostalCode;
                                result.domicileMunicipality = data.data.domicileMunicipality;
                                result.domicileProvince = data.data.domicileProvince;
                                result.address = data.data.address;
                                result.domicileNation = data.data.domicileNation;
                                result.expirationDate = data.data.expirationDate;
                                result.digitalAddress = data.data.digitalAddress;

                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetLog(Auth_Municipality_ID, "Spid - Error", ex.Message);

            }

            return null;
        }

        public async void SetLog(Guid Auth_Municipality_ID, string Header, string Content)
        {
            var log = new AUTH_Spid_Log();

            log.ID = Guid.NewGuid();
            log.AUTH_Municipality_ID = Auth_Municipality_ID;
            log.CreationDate = DateTime.Now;
            log.Header = Header;
            log.Content = Content;

            await AuthProvider.SetSpidLog(log);
        }
    }
}
