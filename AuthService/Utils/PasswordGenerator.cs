using System.Security.Cryptography;

namespace AuthService.Utils
{
    public static class PasswordGenerator
    {
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>/?";

        public static string Generate(int length = 16)
        {
            var all = Lower + Upper + Digits + Symbols;
            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = all[bytes[i] % all.Length];

           
            if (!chars.Any(char.IsLower)) chars[0] = Lower[bytes[0] % Lower.Length];
            if (!chars.Any(char.IsUpper)) chars[1] = Upper[bytes[1] % Upper.Length];
            if (!chars.Any(char.IsDigit)) chars[2] = Digits[bytes[2] % Digits.Length];
            if (!chars.Any(c => Symbols.Contains(c))) chars[3] = Symbols[bytes[3] % Symbols.Length];

            return new string(chars);
        }
    }
}
