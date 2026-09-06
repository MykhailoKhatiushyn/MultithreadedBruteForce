using System;
using System.Text;

namespace MultithreadedBruteForce
{
    public static class PasswordGenerator
    {
        // Printable character set used for generation and brute forcing
        public const string CharacterSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random RandomInstance = new Random();

        /// <summary>
        /// Generates a random target password with length in range [4, 6) -> 4 or 5 characters.
        /// </summary>
        public static string GenerateRandomPassword()
        {
            // Range [4, 6) produces lengths 4 or 5
            int length = RandomInstance.Next(4, 6);
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = RandomInstance.Next(CharacterSet.Length);
                sb.Append(CharacterSet[index]);
            }

            return sb.ToString();
        }
    }
}