using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class PasswordHelper
    {

        public enum PasswordStrength
        {
            Blank = 0,
            VeryWeak = 1,
            Weak = 2,
            Medium = 3,
            Strong = 4,
            VeryStrong = 5
        }

        public string CreateMD5Hash(string Password)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }


     
        public  PasswordStrength GetPasswordStrength(string password)
        {
            int score = 0;
            if (String.IsNullOrEmpty(password) || String.IsNullOrEmpty(password.Trim())) return PasswordStrength.Blank;
            if (HasMinimumLength(password, 8)) score++;
            if (HasMinimumLength(password, 12)) score++;
            if (HasUpperCaseLetter(password) && HasLowerCaseLetter(password)) score++;
            if (HasDigit(password)) score++;
            if (HasSpecialChar(password)) score++;

            if(score == 0 && password.Length > 0)
            {
                score = 1;
            }

            if (IsValidPassword(password) == false && score >= 3)
            {
                score = 2;
            }

            return (PasswordStrength)score;
        }


        public static bool IsStrongPassword(string password)
        {
            return HasMinimumLength(password, 8)
                && HasUpperCaseLetter(password)
                && HasLowerCaseLetter(password)
                && (HasDigit(password) || HasSpecialChar(password));
        }

        public  bool IsValidPassword(string password)
        {
            return IsValidPassword(
                password,
                requiredLength: 8,
                requiredUniqueChars: 1,
                requireNonAlphanumeric: true,
                requireLowercase:true,
                requireUppercase:true,
                requireDigit:true);
        }


        public static bool IsValidPassword(
            string password,
            int requiredLength,
            int requiredUniqueChars,
            bool requireNonAlphanumeric,
            bool requireLowercase,
            bool requireUppercase,
            bool requireDigit)
        {
            if (!HasMinimumLength(password, requiredLength)) return false;
            if (!HasMinimumUniqueChars(password, requiredUniqueChars)) return false;
            if (requireNonAlphanumeric && !HasSpecialChar(password)) return false;
            if (requireLowercase && !HasLowerCaseLetter(password)) return false;
            if (requireUppercase && !HasUpperCaseLetter(password)) return false;
            if (requireDigit && !HasDigit(password)) return false;
            return true;
        }


        #region Helper Methods

        public static bool HasMinimumLength(string password, int minLength)
        {
            if (password == null)
                return false;
            return password.Length >= minLength;
        }

        public static bool HasMinimumUniqueChars(string password, int minUniqueChars)
        {
            if (password == null)
                return false;

            return password.Distinct().Count() >= minUniqueChars;
        }


        public static bool HasDigit(string password)
        {
            if (password == null)
                return false;

            return password.Any(c => char.IsDigit(c));
        }

        public static bool HasSpecialChar(string password)
        {
            if (password == null)
                return false;

            return password.IndexOfAny("!@#$%^&*?_~-£().,".ToCharArray()) != -1;
        }

        public static bool HasUpperCaseLetter(string password)
        {
            if (password == null)
                return false;

            return password.Any(c => char.IsUpper(c));
        }

        public static bool HasLowerCaseLetter(string password)
        {
            if (password == null)
                return false;

            return password.Any(c => char.IsLower(c));
        }
        #endregion
    }
}
