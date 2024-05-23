using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ICWebApp.Application.Helper
{
    public class SpidHelper : ISpidHelper
    {
        private AUTH_External_Verification _Verification;
        private ITEXTProvider TextProvider;

        public SpidHelper(ITEXTProvider TextProvider)
        {
            this.TextProvider = TextProvider;
        }
        public AUTH_External_Verification Verification { get => _Verification; set => _Verification = value; }

        public string? CheckXml(MemoryStream MemoryStream, bool Localhost = false)
        {            
            string xml = Encoding.ASCII.GetString(MemoryStream.ToArray());

            XNamespace nsProtocol = "urn:oasis:names:tc:SAML:2.0:protocol";
            XNamespace nsAssertion = "urn:oasis:names:tc:SAML:2.0:assertion";
            XNamespace nsSignature = "http://www.w3.org/2000/09/xmldsig#";

            XDocument xDoc = XDocument.Parse(xml);
            
            //Response element
            var response = (from t in xDoc.Descendants(nsProtocol + "Response") select t).FirstOrDefault();

            if (response != null)
            {
                var signature = (from t in response.Descendants(nsSignature + "Signature") select t).FirstOrDefault();

                //02 Response non firmata
                if (signature == null)
                {
                    return "SPID_DEFAULT_ERROR";                        
                }

                //04 Firma non valida
                var X509Certificate = (from t in response.Descendants(nsSignature + "X509Certificate") select t.Value).FirstOrDefault();

                if (X509Certificate == null || !CheckCertificate(X509Certificate))
                {
                    return "SPID_DEFAULT_ERROR";
                }

                //08 ID non specificato
                //09 ID Mancante
                var IDAttribute = response.Attribute("ID");

                if(IDAttribute == null || string.IsNullOrEmpty(IDAttribute.Value))
                {
                    return "SPID_DEFAULT_ERROR";
                }

                //10 Versione diverso da 2.0
                var VersionAttribute = response.Attribute("Version");

                if (VersionAttribute == null || VersionAttribute.Value != "2.0")
                {
                    return "SPID_DEFAULT_ERROR";
                }

                //11 IssueInstant non specificato
                //12 IssueInstant Mancante
                //13 IssueInstant Formato non corretto
                var IssueInstantAttribute = response.Attribute("IssueInstant");
                var regex = new Regex("[[0-9]{4}-[0-9]{2}-[0-9]{2}[A-Za-z]{1}[0-9]{2}:[0-9]{2}:[0-9]{2}[A-Za-z]{1}");

                if (IssueInstantAttribute == null || string.IsNullOrEmpty(IssueInstantAttribute.Value) || !regex.IsMatch(IssueInstantAttribute.Value))
                {
                    return "SPID_DEFAULT_ERROR";
                }

                //14 IssueInstant precedente Request
                if (DateTime.Parse(IssueInstantAttribute.Value) <= DateTime.Now.AddMinutes(-10) || DateTime.Parse(IssueInstantAttribute.Value) >= DateTime.Now.AddMinutes(10))
                {
                    return "SPID_DEFAULT_ERROR";
                }

                //16 InResponseTo non specificato
                //17 InResponseTo mancante

                var InRespinseTo = response.Attribute("InResponseTo");

                if (InRespinseTo == null || string.IsNullOrEmpty(IDAttribute.Value))
                {
                    return "SPID_DEFAULT_ERROR";
                }
            }

            //Assertion element
            var assertion = (from t in xDoc.Descendants(nsAssertion + "Assertion") select t).FirstOrDefault();

            if (assertion != null)
            {
                var signature = (from t in assertion.Descendants(nsSignature + "Signature") select t).FirstOrDefault();

                //03 Assertion non firmata
                if (signature == null)
                {
                    return "SPID_DEFAULT_ERROR";
                }
            }

            return null;
        }
        private bool CheckCertificate(string Base64String)
        {
            try
            {
                byte[] RawData = Convert.FromBase64String(Base64String);

                X509Certificate2 x509 = new X509Certificate2(RawData);

                byte[] rawdata = x509.RawData;

                if (x509.NotBefore <= DateTime.Now && x509.NotAfter >= DateTime.Now)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}