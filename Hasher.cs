using System;
using System.Security.Cryptography;
using System.Text;

namespace MultithreadedBruteForce
{
    public static class Hasher
    {
        // Fixed static salt constant defined as required
        private const string StaticSalt = "BruteForce_StaticSalt_2026!#";

        /// <summary>
        /// Hashes a plaintext password combined with the static salt using SHA256.
        /// </summary>
        /// <param name="plainText">Candidate password string.</param>
        /// <returns>Hexadecimal hash string.</returns>
        public static string HashPassword(string plainText)
        {
            if (plainText == null) return string.Empty;

            string saltedInput = plainText + StaticSalt;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(saltedInput);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder builder = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
