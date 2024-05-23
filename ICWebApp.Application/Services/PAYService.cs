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
using ICWebApp.Application.Settings;

namespace ICWebApp.Application.Services
{
    public class PAYService : IPAYService
    {
        private ICONFProvider ConfProvider;
        private IAUTHProvider AuthProvider;
        private ISessionWrapper SessionWrapper;
        private IPagoPaRepository PagoPARepository;
        private NavigationManager NavManager;
        private ILANGProvider LANGProvider;
        private IUnitOfWork _unitOfWork;

        public PAYService(ICONFProvider ConfProvider, IAUTHProvider AuthProvider, ISessionWrapper SessionWrapper, IPagoPaRepository PagoPARepository, NavigationManager NavManager, ILANGProvider LANGProvider, IUnitOfWork _unitOfWork)
        {
            this.ConfProvider = ConfProvider;
            this.AuthProvider = AuthProvider;
            this.SessionWrapper = SessionWrapper;
            this.PagoPARepository = PagoPARepository;
            this.NavManager = NavManager;
            this.LANGProvider = LANGProvider;
            this._unitOfWork = _unitOfWork;
        }
        public async Task<Session?> CreateSession(Guid TransactionFamilyID, List<PAY_Transaction_Position> Positions, AUTH_Users_Anagrafic User_Anagrafic, string ReturnUrl, string CancelUrl)
        {
            if (User_Anagrafic != null && User_Anagrafic.STRIPE_Customer_ID != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                var conf = await ConfProvider.GetPayConfiguration(null);
                var municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                if (conf == null || municipality == null)
                {
                    return null;
                }

                if (conf.STRIPE_Secret == null)
                {
                    return null;
                }

                StripeConfiguration.ApiKey = conf.STRIPE_Secret;

                var LineItems = new List<SessionLineItemOptions>();

                foreach (var pos in Positions)
                {
                    if (pos.Amount != null && pos.Description != null)
                    {
                        var stringAmount = pos.Amount.Value.ToString("N2").ToString().Replace(",", "").Replace(".", "");

                        var PosText = WebUtility.HtmlDecode(Regex.Replace(pos.Description, "<[^>]*(>|$)", string.Empty));

                        LineItems.Add(new SessionLineItemOptions()
                        {
                            Quantity = 1,
                            PriceData = new SessionLineItemPriceDataOptions()
                            {
                                Currency = "eur",
                                UnitAmount = long.Parse(stringAmount),
                                ProductData = new SessionLineItemPriceDataProductDataOptions()
                                {
                                    Name = PosText,
                                    TaxCode = "txcd_00000000"
                                },
                                TaxBehavior = "inclusive"
                            }
                        });
                    }
                }

                var metadata = new Dictionary<string, string>();

                metadata.Add("Gemeinde", municipality.Name);

                var options = new SessionCreateOptions
                {
                    Customer = User_Anagrafic.STRIPE_Customer_ID,
                    CancelUrl = CancelUrl,
                    ClientReferenceId = TransactionFamilyID.ToString(),
                    SuccessUrl = ReturnUrl,
                    PaymentMethodTypes = new List<string> { "card", "sepa_debit" },
                    Mode = "payment",
                    PaymentIntentData = new SessionPaymentIntentDataOptions()
                    {
                        SetupFutureUsage = "on_session",
                        Metadata = metadata
                    },
                    LineItems = LineItems,
                    Metadata = metadata
                };

                var service = new Stripe.Checkout.SessionService();
                var result = service.Create(options);

                return result;
            }

            return null;
        }
        public async Task<Customer?> GetOrCreateCustomer(AUTH_Users_Anagrafic User_Anagrafic)
        {
            var conf = await ConfProvider.GetPayConfiguration(null);

            if (conf == null)
            {
                return null;
            }

            if (conf.STRIPE_Secret == null)
            {
                return null;
            }

            if (User_Anagrafic.STRIPE_Customer_ID != null)
            {
                StripeConfiguration.ApiKey = conf.STRIPE_Secret;
                var service = new CustomerService();

                var customer = service.Get(User_Anagrafic.STRIPE_Customer_ID);

                return customer;
            }
            else
            {

                StripeConfiguration.ApiKey = conf.STRIPE_Secret;

                var options = new CustomerCreateOptions
                {
                    Address = new AddressOptions()
                    {
                        City = User_Anagrafic.DomicileMunicipality,
                        Country = User_Anagrafic.DomicileProvince,
                        PostalCode = User_Anagrafic.DomicilePostalCode,
                        State = User_Anagrafic.DomicileNation,
                        Line1 = User_Anagrafic.DomicileStreetAddress
                    },
                    Description = User_Anagrafic.FiscalNumber,
                    Email = User_Anagrafic.Email,
                    Name = User_Anagrafic.FirstName + " " + User_Anagrafic.LastName,
                    Phone = User_Anagrafic.MobilePhone
                };

                var service = new CustomerService();

                var customer = service.Create(options);

                User_Anagrafic.STRIPE_Customer_ID = customer.Id;

                await AuthProvider.SetAnagrafic(User_Anagrafic);

                return customer;
            }
        }
        public async Task<Customer?> UpdateCustomer(AUTH_Users_Anagrafic User_Anagrafic)
        {
            var conf = await ConfProvider.GetPayConfiguration(null);

            if (conf == null)
            {
                return null;
            }

            if (conf.STRIPE_Secret == null)
            {
                return null;
            }

            if (User_Anagrafic.STRIPE_Customer_ID != null)
            {
                StripeConfiguration.ApiKey = conf.STRIPE_Secret;
                var service = new CustomerService();

                var customer = service.Get(User_Anagrafic.STRIPE_Customer_ID);

                var options = new CustomerUpdateOptions
                {
                    Address = new AddressOptions()
                    {
                        City = User_Anagrafic.DomicileMunicipality,
                        Country = User_Anagrafic.DomicileProvince,
                        PostalCode = User_Anagrafic.DomicilePostalCode,
                        State = User_Anagrafic.DomicileNation,
                        Line1 = User_Anagrafic.DomicileStreetAddress
                    },
                    Description = User_Anagrafic.FiscalNumber,
                    Email = User_Anagrafic.Email,
                    Name = User_Anagrafic.FirstName + " " + User_Anagrafic.LastName,
                    Phone = User_Anagrafic.MobilePhone
                };

                service.Update(User_Anagrafic.STRIPE_Customer_ID, options);

                return customer;
            }

            return null;
        }
        public async Task<Refund?> CreateRefund(string STRIPE_Session_ID, AUTH_Users_Anagrafic User_Anagrafic, long Amount)
        {
            if (User_Anagrafic != null && User_Anagrafic.STRIPE_Customer_ID != null && SessionWrapper.AUTH_Municipality_ID != null && STRIPE_Session_ID != null)
            {
                var conf = await ConfProvider.GetPayConfiguration(null);
                var municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                if (conf == null || municipality == null)
                {
                    return null;
                }

                if (conf.STRIPE_Secret == null)
                {
                    return null;
                }

                var metadata = new Dictionary<string, string>();

                metadata.Add("Gemeinde", municipality.Name);

                StripeConfiguration.ApiKey = conf.STRIPE_Secret;

                var SessionService = new SessionService();

                var session = SessionService.Get(STRIPE_Session_ID);

                if (session == null)
                    return null;

                if (session.PaymentIntentId == null)
                    return null;

                var service = new RefundService();

                var options = new RefundCreateOptions()
                {                    
                    Amount = Amount,
                    Metadata = metadata,
                    Reason = "requested_by_customer",
                    PaymentIntent = session.PaymentIntentId
                };
    
                var result = service.Create(options);

                return result;
            }

            return null;
        }
        public async Task<string?> PAGOPA_Create(CONF_PagoPA Configuration, string Family_ID, List<PAY_Transaction_Position> Positions, string PagoPANumero, string ReturnUrl, string CancelUrl)
        {
            var currentUserAnagrafic = await AuthProvider.GetAnagraficByUserID(SessionWrapper.CurrentUser.ID);

            if (currentUserAnagrafic != null)
            {
                PaymentRequest request = new PaymentRequest();

                request.PortaleID = Configuration.PortaleID;
                request.Funzione = "PAGAMENTO";
                request.URLDiRitorno = ReturnUrl;
                request.URLDiNotifica = NavManager.BaseUri + "PagoPa/Notification?Family_ID=" + Family_ID + "&ReturnUrl=" + Uri.EscapeDataString(ReturnUrl);
                request.URLBack = CancelUrl;
                request.CommitNotifica = "S";

                if(request.URLDiNotifica != null)
                {
                    request.URLDiNotifica = request.URLDiNotifica.Replace("https://localhost:7149/", "https://test.comunix.bz.it/");
                }

                if (request.URLDiRitorno != null)
                {
                    request.URLDiRitorno = request.URLDiRitorno.Replace("https://localhost:7149/", "https://test.comunix.bz.it/");
                }

                if (request.URLBack != null)
                {
                    request.URLBack = request.URLBack.Replace("https://localhost:7149/", "https://test.comunix.bz.it/");
                }

                var userData = new UserData();

                userData.EmailUtente = currentUserAnagrafic.Email;
                if (currentUserAnagrafic.FiscalNumber != null && currentUserAnagrafic.FiscalNumber.Length > 16)
                {
                    userData.IdentificativoUtente = currentUserAnagrafic.FiscalNumber.Substring(0, 16);
                }
                else
                {
                    userData.IdentificativoUtente = currentUserAnagrafic.FiscalNumber;
                }
                if ((currentUserAnagrafic.FirstName + " " + currentUserAnagrafic.LastName) != null && (currentUserAnagrafic.FirstName + " " + currentUserAnagrafic.LastName).Length > 16)
                {
                    userData.Denominazione = (currentUserAnagrafic.FirstName + " " + currentUserAnagrafic.LastName).Substring(0, 16);
                }
                else
                {
                    userData.Denominazione = currentUserAnagrafic.FirstName + " " + currentUserAnagrafic.LastName;
                }

                request.UserData = userData;

                var serviceData = new ServiceData();

                if (Configuration.CodiceUtente != null && Configuration.CodiceUtente.Length > 5)
                {
                    serviceData.CodiceUtente = Configuration.CodiceUtente.Substring(0, 5);
                }
                else
                {
                    serviceData.CodiceUtente = Configuration.CodiceUtente;
                }

                if (Configuration.CodiceEnte != null && Configuration.CodiceEnte.Length > 5)
                {
                    serviceData.CodiceEnte = Configuration.CodiceEnte.Substring(0, 5);
                }
                else
                {
                    serviceData.CodiceEnte = Configuration.CodiceEnte;
                }


                serviceData.TipoUfficio = "";      
                serviceData.CodiceUfficio = "";
                
                if(Configuration.IsTest != true) 
                { 
                    if (Positions.FirstOrDefault(e => e.TipologiaServizio != null) != null)
                    {
                        var typology = Positions.FirstOrDefault(e => e.TipologiaServizio != null)!.TipologiaServizio!;
                        serviceData.TipologiaServizio = typology.Length <= 3 ? typology : typology[..3];
                    }
                }

                if (serviceData.TipologiaServizio == null)
                {
                    if (Configuration.TipologiaServizio != null && Configuration.TipologiaServizio.Length > 3)
                    {
                        serviceData.TipologiaServizio = Configuration.TipologiaServizio.Substring(0, 3);
                    }
                    else
                    {
                        serviceData.TipologiaServizio = Configuration.TipologiaServizio;
                    }
                }

                serviceData.NumeroOperazione = Guid.NewGuid().ToString();

                if (PagoPANumero.Length > 20)
                {
                    serviceData.NumeroDocumento = PagoPANumero.Substring(0, 20);
                }
                else
                {
                    serviceData.NumeroDocumento = PagoPANumero;
                }

                serviceData.AnnoDocumento = "";
                serviceData.Valuta = "EUR";

                long sum = 0;

                var bolloPosition = Positions.Where(p => p.IsBollo == true).FirstOrDefault();

                if (bolloPosition != null && bolloPosition.BolloNumber != null)
                {
                    var marcaDaBollo = new MarcaDaBolloDigitale();

                    if (!Positions.Any(p => p.IsBollo != true))
                    {
                        var bolloIdent = await _unitOfWork.Repository<PAY_PagoPa_Identifier>().FirstOrDefaultAsync(e => e.IsBollo == true);

                        if(Configuration.IsTest != true) 
                        {
                            serviceData.TipologiaServizio = bolloIdent?.TipologiaServizio ?? "BLO"; //DB
                        }
                    }

                    marcaDaBollo.ImportoMarcaDaBolloDigitale = "1600";
                    marcaDaBollo.ProvinciaResidenza = "BZ";
                    marcaDaBollo.TipoBolloDaErogare = "01";

                    CoreCS cs = new CoreCS();

                    if (bolloPosition.BolloHashType == 0) 
                    {
                        bolloPosition.BolloHash = cs.ComputeSha256Hash(bolloPosition.BolloNumber);
                    }
                    else
                    {
                        if (bolloPosition.BolloFILE_FileInfo_ID != null)
                        {
                            var file = await _unitOfWork.Repository<FILE_FileStorage>().GetByIDAsync(bolloPosition.BolloFILE_FileInfo_ID.Value);

                            if(file != null && file.FileImage != null)
                            {
                                bolloPosition.BolloHash = cs.ComputeSha256Hash(Convert.ToBase64String(file.FileImage));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(bolloPosition.BolloHash))
                    {
                        marcaDaBollo.SegnaturaMarcaDaBolloDigitale = bolloPosition.BolloHash;
                        serviceData.MarcaDaBolloDigitale = marcaDaBollo;
                    }
                }

                if (Configuration.IsTest == false)
                {
                    var accountingData = new AccountingData();

                    var importiContabili = new List<ImportoContabile>();

                    foreach (var pos in Positions.GroupBy(p => p.PagoPA_Identification).ToList())
                    {
                        if (pos.Sum(p => p.Amount) != null)
                        {
                            var importoContaible = new ImportoContabile();

                            if (pos.Key != null && pos.Key != "")
                            {
                                importoContaible.Identificativo = pos.Key.Trim();
                            }
                            else
                            {
                                var defaultIdentifier = await _unitOfWork.Repository<PAY_PagoPa_Identifier>().Where(p => p.IsDefault == true).FirstOrDefaultAsync();

                                if (defaultIdentifier != null) 
                                {
                                    importoContaible.Identificativo = defaultIdentifier.PagoPA_Identifier.Trim();
                                }
                            }

                            var stringAmount = pos.Sum(p => p.Amount).Value.ToString("N2").ToString().Replace(",", "").Replace(".", "");

                            if (stringAmount != null)
                            {
                                importoContaible.Valore = stringAmount;

                                sum += long.Parse(stringAmount);

                                importiContabili.Add(importoContaible);
                            }
                        }
                    }

                    accountingData.ImportiContabili = importiContabili;

                    var enteDestinatario = new EnteDestinatario();

                    if (Configuration.CodiceEntePortaleEsterno != null && Configuration.CodiceEntePortaleEsterno.Length > 50)
                    {
                        enteDestinatario.CodiceEntePortaleEsterno = Configuration.CodiceEntePortaleEsterno.Substring(0, 50);
                    }
                    else
                    {
                        enteDestinatario.CodiceEntePortaleEsterno = Configuration.CodiceEntePortaleEsterno;
                    }

                    if (Configuration.DescrizioneEntePortaleEsterno != null && Configuration.DescrizioneEntePortaleEsterno.Length > 256)
                    {
                        enteDestinatario.DescrEntePortaleEsterno = Configuration.DescrizioneEntePortaleEsterno.Substring(0, 256);
                    }
                    else
                    {
                        enteDestinatario.DescrEntePortaleEsterno = Configuration.DescrizioneEntePortaleEsterno;
                    }
                    
                    enteDestinatario.Valore = sum.ToString();
                    enteDestinatario.Causale = "Comunix";

                    accountingData.EntiDestinatari = new List<EnteDestinatario>() { enteDestinatario };

                    request.AccountingData = accountingData;
                }
                else
                {
                    foreach (var pos in Positions.ToList())
                    {
                        if (pos.Amount != null)
                        {
                            var stringAmount = pos.Amount.Value.ToString("N2").ToString().Replace(",", "").Replace(".", "");

                            if (stringAmount != null)                            
                            {
                                sum += long.Parse(stringAmount);
                            }
                        }
                    }
                }

                serviceData.Importo = sum.ToString();
                request.ServiceData = serviceData;

                var Content = PagoPARepository.XMLSerialize(request);

                var log = new PAY_PagoPA_Log();

                log.ID = Guid.NewGuid();
                log.CreationDate = DateTime.Now;
                log.XMLData = Content;
                log.CallType = "Create";
                log.Incoming = false;

                await _unitOfWork.Repository<PAY_PagoPA_Log>().InsertOrUpdateAsync(log);

                //Save in DB

                if (LANGProvider.GetCurrentLanguageID() == LanguageSettings.German) //German
                {
                    return await PagoPARepository.InitializePayment(Configuration.BaseAddressDE, Configuration.IV, Configuration.Key, Configuration.PortaleID, Content);
                }
                else
                {
                    return await PagoPARepository.InitializePayment(Configuration.BaseAddressIT, Configuration.IV, Configuration.Key, Configuration.PortaleID, Content);
                }
            }

            return null;
        }
    }
}
