using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public static class CodiceFiscaleHelper
    {
        public static bool IsNotNullOrEmpty(this string s)
        {
            return !IsNullOrEmpty(s);
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return s == null || s.Length == 0;
        }

        public static string FormatWith(this string s, params object[] args)
        {
            return String.Format(s, args);
        }

        public static string GetCodiceFiscale(string name, string firstName, string astatCode, DateTime geburtsDatum, bool istMännlich)
        {
            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException("name");
            }

            if (firstName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("firstName");
            }

            if (astatCode.IsNullOrEmpty())
            {
                throw new ArgumentNullException("astatCode");
            }

            if (geburtsDatum == DateTime.MinValue)
            {
                throw new ArgumentNullException("geburtsDatum");
            }

            string codiceFiscale = GetCognomeConsonanti(name) + GetNomeConsonanti(firstName) + GetDatePart(geburtsDatum, istMännlich) + astatCode;

            return codiceFiscale + GetCheckDigit(codiceFiscale);
        }

        public static bool IsCheckDigitValid(string steuerNummer)
        {
            if (steuerNummer == null || steuerNummer.Length != 16)
            {
                return false;
            }

            string checkDigitOri = steuerNummer.Substring(15, 1);
            string checkDigitCalc = GetCheckDigit(steuerNummer.Substring(0, 15));

            if (checkDigitCalc == checkDigitOri)
            {
                return true;
            }

            return false;
        }

        private static string GetCheckDigit(string steuerNummer)
        {
            if (steuerNummer.IsNullOrEmpty())
            {
                throw new ArgumentNullException("steuerNummer");
            }

            if (steuerNummer.Length != 15)
            {
                throw new ArgumentException("The fiscal code '{0}' is invalid because it has not a lenght of 15.".FormatWith(steuerNummer));
            }

            string check = "";
            int sum = 0;
            for (int i = 1; i < 16; i++)
            {
                string pos = steuerNummer.Substring(i - 1, 1);

                if (i % 2 == 1)
                {
                    sum = sum + GetDispariValue(pos);
                }
                else
                {
                    sum = sum + GetPariValue(pos);
                }
            }
            sum = sum % 26;

            if (sum == 0) { check = "A"; }
            if (sum == 1) { check = "B"; }
            if (sum == 2) { check = "C"; }
            if (sum == 3) { check = "D"; }
            if (sum == 4) { check = "E"; }
            if (sum == 5) { check = "F"; }
            if (sum == 6) { check = "G"; }
            if (sum == 7) { check = "H"; }
            if (sum == 8) { check = "I"; }
            if (sum == 9) { check = "J"; }
            if (sum == 10) { check = "K"; }
            if (sum == 11) { check = "L"; }
            if (sum == 12) { check = "M"; }
            if (sum == 13) { check = "N"; }
            if (sum == 14) { check = "O"; }
            if (sum == 15) { check = "P"; }
            if (sum == 16) { check = "Q"; }
            if (sum == 17) { check = "R"; }
            if (sum == 18) { check = "S"; }
            if (sum == 19) { check = "T"; }
            if (sum == 20) { check = "U"; }
            if (sum == 21) { check = "V"; }
            if (sum == 22) { check = "W"; }
            if (sum == 23) { check = "X"; }
            if (sum == 24) { check = "Y"; }
            if (sum == 25) { check = "Z"; }

            return check;
        }

        private static string GetCognomeConsonanti(string text)
        {
            string consonanti = "";
            text = text.Replace(" ", "");

            foreach (char item in text)
            {
                string letter = item.ToString().ToUpper();
                if ((letter == "B") ||
                    (letter == "C") ||
                    (letter == "D") ||
                    (letter == "F") ||
                    (letter == "G") ||
                    (letter == "H") ||
                    (letter == "J") ||
                    (letter == "K") ||
                    (letter == "L") ||
                    (letter == "M") ||
                    (letter == "N") ||
                    (letter == "P") ||
                    (letter == "Q") ||
                    (letter == "R") ||
                    (letter == "S") ||
                    (letter == "T") ||
                    (letter == "ß") ||
                    (letter == "T") ||
                    (letter == "V") ||
                    (letter == "W") ||
                    (letter == "Y") ||
                    (letter == "X") ||
                    (letter == "Z"))
                {
                    consonanti = consonanti + item;
                }
            }

            if (consonanti.Length < 3)
            {
                foreach (char item in text)
                {
                    string letter = item.ToString().ToUpper();
                    if ((letter == "A") ||
                        (letter == "E") ||
                        (letter == "I") ||
                        (letter == "O") ||
                        (letter == "U"))
                    {
                        consonanti = consonanti + item;
                    }
                }
            }

            if (consonanti.Length < 3)
            {
                consonanti = consonanti + "X";
            }
            if (consonanti.Length < 3)
            {
                consonanti = consonanti + "X";
            }
            if (consonanti.Length < 3)
            {
                consonanti = consonanti + "X";
            }

            return consonanti.Substring(0, 3).ToUpper();
        }

        private static string GetDatePart(DateTime geburtsDatum, bool geschlecht)
        {
            string datepart = "";
            datepart = geburtsDatum.Year.ToString().Substring(2, 2);

            if (geburtsDatum.Month == 1) { datepart = datepart + "A"; }
            if (geburtsDatum.Month == 2) { datepart = datepart + "B"; }
            if (geburtsDatum.Month == 3) { datepart = datepart + "C"; }
            if (geburtsDatum.Month == 4) { datepart = datepart + "D"; }
            if (geburtsDatum.Month == 5) { datepart = datepart + "E"; }
            if (geburtsDatum.Month == 6) { datepart = datepart + "H"; }
            if (geburtsDatum.Month == 7) { datepart = datepart + "L"; }
            if (geburtsDatum.Month == 8) { datepart = datepart + "M"; }
            if (geburtsDatum.Month == 9) { datepart = datepart + "P"; }
            if (geburtsDatum.Month == 10) { datepart = datepart + "R"; }
            if (geburtsDatum.Month == 11) { datepart = datepart + "S"; }
            if (geburtsDatum.Month == 12) { datepart = datepart + "T"; }

            int day = geburtsDatum.Day;
            if (geschlecht == false)
            {
                day = day + 40;
            }

            if (day.ToString().Length < 2)
            {
                datepart = datepart + "0" + day.ToString();
            }
            else
            {
                datepart = datepart + day.ToString();
            }

            return datepart;
        }

        private static int GetDispariValue(string carattere)
        {
            int val = 0;
            if (carattere == "0") { val = 1; }
            if (carattere == "1") { val = 0; }
            if (carattere == "2") { val = 5; }
            if (carattere == "3") { val = 7; }
            if (carattere == "4") { val = 9; }
            if (carattere == "5") { val = 13; }
            if (carattere == "6") { val = 15; }
            if (carattere == "7") { val = 17; }
            if (carattere == "8") { val = 19; }
            if (carattere == "9") { val = 21; }
            if (carattere == "A") { val = 1; }
            if (carattere == "B") { val = 0; }
            if (carattere == "C") { val = 5; }
            if (carattere == "D") { val = 7; }
            if (carattere == "E") { val = 9; }
            if (carattere == "F") { val = 13; }
            if (carattere == "G") { val = 15; }
            if (carattere == "H") { val = 17; }
            if (carattere == "I") { val = 19; }
            if (carattere == "J") { val = 21; }
            if (carattere == "K") { val = 2; }
            if (carattere == "L") { val = 4; }
            if (carattere == "M") { val = 18; }
            if (carattere == "N") { val = 20; }
            if (carattere == "O") { val = 11; }
            if (carattere == "P") { val = 3; }
            if (carattere == "Q") { val = 6; }
            if (carattere == "R") { val = 8; }
            if (carattere == "S") { val = 12; }
            if (carattere == "T") { val = 14; }
            if (carattere == "U") { val = 16; }
            if (carattere == "V") { val = 10; }
            if (carattere == "W") { val = 22; }
            if (carattere == "X") { val = 25; }
            if (carattere == "Y") { val = 24; }
            if (carattere == "Z") { val = 23; }

            return val;
        }

        private static string GetNomeConsonanti(string text)
        {
            string consonanti = "";
            text = text.Replace(" ", "");

            foreach (char item in text)
            {
                string letter = item.ToString().ToUpper();
                if ((letter == "B") ||
                    (letter == "C") ||
                    (letter == "D") ||
                    (letter == "F") ||
                    (letter == "G") ||
                    (letter == "H") ||
                    (letter == "J") ||
                    (letter == "K") ||
                    (letter == "L") ||
                    (letter == "M") ||
                    (letter == "N") ||
                    (letter == "P") ||
                    (letter == "Q") ||
                    (letter == "R") ||
                    (letter == "S") ||
                    (letter == "T") ||
                    (letter == "ß") ||
                    (letter == "T") ||
                    (letter == "V") ||
                    (letter == "W") ||
                    (letter == "Y") ||
                    (letter == "X") ||
                    (letter == "Z"))
                {
                    consonanti = consonanti + item;
                }
            }

            if (consonanti.Length >= 4)
            {
                consonanti = consonanti.Substring(0, 1) + consonanti.Substring(2, 1) + consonanti.Substring(3, 1);
            }


            if (consonanti.Length < 3)
            {
                foreach (char item in text)
                {
                    string letter = item.ToString().ToUpper();
                    if ((letter == "A") ||
                        (letter == "E") ||
                        (letter == "I") ||
                        (letter == "O") ||
                        (letter == "U"))
                    {
                        consonanti = consonanti + item;
                    }
                }
            }

            if (consonanti.Length < 3)
            {
                consonanti = consonanti + "X";
            }

            if (consonanti.Length < 3)
            {
                consonanti = consonanti + "X";
            }

            if (consonanti.Length < 3)
            {
                consonanti = consonanti + "X";
            }

            return consonanti.Substring(0, 3).ToUpper();
        }

        private static int GetPariValue(string carattere)
        {
            int val = 0;
            if (carattere == "0") { val = 0; }
            if (carattere == "1") { val = 1; }
            if (carattere == "2") { val = 2; }
            if (carattere == "3") { val = 3; }
            if (carattere == "4") { val = 4; }
            if (carattere == "5") { val = 5; }
            if (carattere == "6") { val = 6; }
            if (carattere == "7") { val = 7; }
            if (carattere == "8") { val = 8; }
            if (carattere == "9") { val = 9; }
            if (carattere == "A") { val = 0; }
            if (carattere == "B") { val = 1; }
            if (carattere == "C") { val = 2; }
            if (carattere == "D") { val = 3; }
            if (carattere == "E") { val = 4; }
            if (carattere == "F") { val = 5; }
            if (carattere == "G") { val = 6; }
            if (carattere == "H") { val = 7; }
            if (carattere == "I") { val = 8; }
            if (carattere == "J") { val = 9; }
            if (carattere == "K") { val = 10; }
            if (carattere == "L") { val = 11; }
            if (carattere == "M") { val = 12; }
            if (carattere == "N") { val = 13; }
            if (carattere == "O") { val = 14; }
            if (carattere == "P") { val = 15; }
            if (carattere == "Q") { val = 16; }
            if (carattere == "R") { val = 17; }
            if (carattere == "S") { val = 18; }
            if (carattere == "T") { val = 19; }
            if (carattere == "U") { val = 20; }
            if (carattere == "V") { val = 21; }
            if (carattere == "W") { val = 22; }
            if (carattere == "X") { val = 23; }
            if (carattere == "Y") { val = 24; }
            if (carattere == "Z") { val = 25; }

            return val;
        }

        public static string GetIstatCode(string steuerNummer)
        {
            if (steuerNummer.IsNullOrEmpty())
            {
                throw new ArgumentNullException("steuerNummer");
            }

            if (steuerNummer.Length != 16)
            {
                throw new ArgumentException("The fiscal code '{0}' is invalid because it has not a lenght of 15.".FormatWith(steuerNummer));
            }

            string result = steuerNummer.Substring(11, 4);
            return result;
        }

        public static DateTime GetDateFromFiscalCode(string fiscalCode)
        {
            try
            {
                Dictionary<string, string> month = new Dictionary<string, string>();
                // To Upper
                fiscalCode = fiscalCode.ToUpper();
                month.Add("A", "01");
                month.Add("B", "02");
                month.Add("C", "03");
                month.Add("D", "04");
                month.Add("E", "05");
                month.Add("H", "06");
                month.Add("L", "07");
                month.Add("M", "08");
                month.Add("P", "09");
                month.Add("R", "10");
                month.Add("S", "11");
                month.Add("T", "12");
                // Get Date
                string date = fiscalCode.Substring(6, 5);
                int y = int.Parse(date.Substring(0, 2));
                int actyear = DateTime.Today.Year - 2000;

                string yy = ((y < actyear) ? "20" : "19") + y.ToString("00");
                string m = month[date.Substring(2, 1)];
                int d = int.Parse(date.Substring(3, 2));
                if (d > 31)
                    d -= 40;
                // Return Date
                return Convert.ToDateTime(string.Format("{0}/{1}/{2}", d.ToString("00"), m, yy));
            }
            catch
            {
                return Convert.ToDateTime("01/01/1900");
            }
        }
    }
}
